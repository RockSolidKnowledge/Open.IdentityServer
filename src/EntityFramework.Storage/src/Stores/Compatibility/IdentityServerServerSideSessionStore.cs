// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using IdentityServerServerSideSessions = Open.IdentityServer.Models.IdentityServerServerSideSessions;

namespace Open.IdentityServer.EntityFramework.Stores;

/// <summary>
/// Storage and retrieval of server-side sessions using entity framework core
/// </summary>
public class IdentityServerServerSideSessionStore(
    IPersistedGrantDbContext dbContext,
    ITelemetryService telemetry,
    ILogger<IdentityServerServerSideSessionStore> logger) : IIdentityServerServerSideSessionStore
{
    /// <inheritdoc />
    public async Task<IdentityServerServerSideSessions?> GetSession(string key)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Entities.IdentityServerServerSideSessions? session = await dbContext.ServerSideSessions
            .SingleOrDefaultAsync(x => x.Key == key);

        return session?.ToModel();
    }

    /// <inheritdoc />
    public async Task CreateSession(IdentityServerServerSideSessions session)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(session.Key);

        Entities.IdentityServerServerSideSessions? existing = await dbContext.ServerSideSessions
            .SingleOrDefaultAsync(x => x.Key == session.Key);

        if (existing != null)
        {
            logger.LogError("failed storing '{SessionKey}' session in database, session with key already exists",
                session.Key);
            return;
        }

        Entities.IdentityServerServerSideSessions sessionEntity = session.ToEntity();

        await dbContext.ServerSideSessions.AddAsync(sessionEntity);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "exception storing '{SessionKey}' session in database", session.Key);
        }
    }

    /// <inheritdoc />
    public async Task UpdateSession(IdentityServerServerSideSessions session)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(session.Key);

        Entities.IdentityServerServerSideSessions? existing = await dbContext.ServerSideSessions
            .SingleOrDefaultAsync(x => x.Key == session.Key);

        if (existing == null)
        {
            logger.LogError("failed updating '{SessionKey}' session in database, session not found", session.Key);
            return;
        }

        session.UpdateEntity(existing);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "exception updating '{SessionKey}' session in database", session.Key);
        }
    }

    /// <inheritdoc />
    public async Task DeleteSession(string key)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Entities.IdentityServerServerSideSessions? existing = await dbContext.ServerSideSessions
            .SingleOrDefaultAsync(x => x.Key == key);

        if (existing == null)
        {
            logger.LogError("failed deleting '{SessionKey}' session in database, session not found", key);
            return;
        }

        dbContext.ServerSideSessions.Remove(existing);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "exception deleting '{SessionKey}' session in database", key);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IdentityServerServerSideSessions>> FilterSessions(string subjectId, string sessionId)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        return (await dbContext.ServerSideSessions
                .Where(x => x.SubjectId == subjectId && x.SessionId == sessionId)
                .ToListAsync())
            .Select(x => x.ToModel());
    }
}