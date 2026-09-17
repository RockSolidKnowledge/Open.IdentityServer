// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Models;
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
    
    /// <inheritdoc />
    public async Task<QueryResult<IdentityServerServerSideSessions>> FilterSessions(SessionQuery? inputQuery, CancellationToken ct = default)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        SessionQuery query = inputQuery ?? new SessionQuery();

        IQueryable<Entities.IdentityServerServerSideSessions> filteredResults = ApplyFilter(query, dbContext.ServerSideSessions.AsQueryable());

        int count = await filteredResults.CountAsync(cancellationToken: ct);

        if (count < 1)
        {
            return new QueryResult<IdentityServerServerSideSessions>
            {
                TotalCount = count, TotalPages = 0, CurrentPage = 0, HasPrevResults = false, HasNextResults = false, 
                Results = [],
            };
        }
        
        int totalPages = (count / query.CountRequested) + (count % query.CountRequested != 0 ? 1 : 0);
        int currentPage = 1;

        if (!string.IsNullOrWhiteSpace(query.ResultsToken))
        {
            (long tokenFirst, long tokenLast) = ParseResultsToken(query);
            int elementsBeforeToken = await filteredResults.CountAsync(x => x.Id <= tokenFirst, cancellationToken: ct);
            currentPage = 1 + (elementsBeforeToken / query.CountRequested);

            if (query.RequestPriorResults)
            {
                filteredResults = filteredResults
                    .Where(x => x.Id >= tokenFirst).Take(query.CountRequested);
            }
            else
            {
                currentPage++;
                filteredResults = filteredResults
                    .Where(x => x.Id > tokenLast).Take(query.CountRequested);
            }
        }
        else
        {
            filteredResults = filteredResults.Take(query.CountRequested);
        }

        var results = filteredResults.ToList();
        
        return new QueryResult<IdentityServerServerSideSessions>
        {
            TotalCount = count,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            HasPrevResults = currentPage > 1,
            HasNextResults = currentPage < totalPages,
            ResultsToken = $"{results.First().Id},{results.Last().Id}",
            Results = results.Select(x => x.ToModel()).ToList(),
        };
    }

    private (long, long) ParseResultsToken(SessionQuery query)
    {
        long tokenFirst = 0;
        long tokenLast = 0;

        if (query.ResultsToken != null)
        {
            var split = query.ResultsToken.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (!long.TryParse(split.First(), out tokenFirst) || !long.TryParse(split.Last(), out tokenLast))
            {
                logger.LogError("Error occured parsing result token");
            }
        }

        return new ValueTuple<long, long>(tokenFirst, tokenLast);
    }

    private IQueryable<Entities.IdentityServerServerSideSessions> ApplyFilter(SessionQuery query,
        IQueryable<Entities.IdentityServerServerSideSessions> input)
    {
        if (!string.IsNullOrWhiteSpace(query.SubjectId))
        {
            input = input
                .Where(x => x.SubjectId.Contains(query.SubjectId));
        }

        if (!string.IsNullOrWhiteSpace(query.SessionId))
        {
            input = input.Where(x => x.SessionId != null && x.SessionId.Contains(query.SessionId));
        }

        if (!string.IsNullOrWhiteSpace(query.DisplayName))
        {
            input = input.Where(x => x.DisplayName != null && x.DisplayName.Contains(query.DisplayName));
        }

        return input.OrderBy(x => x.Id);
    }
}