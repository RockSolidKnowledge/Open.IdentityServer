// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.Services;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Stores.Serialization;

#nullable enable
namespace Open.IdentityServer.EntityFramework.Stores;

/// <summary>
/// EF implementation of the Pushed Authorization Request Store
/// </summary>
public class PushedAuthorizationRequestStore(
    IPersistedGrantDbContext context,
    IPersistentGrantSerializer serializer,
    ITelemetryService telemetry,
    ILogger<PushedAuthorizationRequestStore> logger) : IPushedAuthorizationRequestStore
{
    /// <summary>
    /// Stores the passed pushed authorization request against the id used as a key.
    /// </summary>
    /// <param name="requestInformation">The pushed authorization request information to store</param>
    /// <returns>A task indicating the async lifetime of the method</returns>
    public async Task StorePushedAuthorizationRequestAsync(PushedAuthorizationMemento requestInformation)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        var parametersAsDictionary = requestInformation
            .Parameters
            .AllKeys
            .ToDictionary(k => k!, k=>requestInformation.Parameters.GetValues(k) ?? []);
        
        var entity = new PushedAuthorizationRequest()
        {
            ReferenceValueHash = requestInformation.Key,
            ExpiresAtUtc = requestInformation.ValidUntil.UtcDateTime,
            Parameters = serializer.Serialize(parametersAsDictionary)
        };

        logger.Log(LogLevel.Information,"Stored PAR {par_id}",requestInformation.Key);
        await context.PushedAuthorizationRequests.AddAsync(entity);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a pushed authorization request.
    /// </summary>
    /// <param name="id">The id of the stored request to retrieve</param>
    /// <returns>The stored request of null if no request matches the passed id</returns>
    public async Task<PushedAuthorizationMemento?> GetPushedAuthorizationRequestAsync(string id)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        PushedAuthorizationRequest? request = await context
            .PushedAuthorizationRequests
            .SingleOrDefaultAsync(par => par.ReferenceValueHash == id);

        if (request == null)
        {
            logger.Log(LogLevel.Information,"No PAR with id {par_id}",id);
            return null;
        }

        var parametersMap = serializer
            .Deserialize<Dictionary<string, string[]>>(request.Parameters);

        NameValueCollection parameters = new NameValueCollection();
        foreach (string kp in (parametersMap?.Keys ?? Enumerable.Empty<string>()))
        {
            foreach (string val in parametersMap![kp])
            {
                parameters.Add(kp,val);
            }
        }

        // Converts a DateTime from database to a DateTimeOffset
        var expiresAt = new DateTimeOffset(request.ExpiresAtUtc, TimeSpan.Zero);

        return new PushedAuthorizationMemento(id, expiresAt, parameters);
    }

    /// <summary>
    /// Removes the pushed authorization request from the store
    /// </summary>
    /// <param name="id">The id of the stored request to remove</param>
    /// <returns>A task, which is marked completed when the removal has been done</returns>
    public Task RemovePushedAuthorizationRequestAsync(string id)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        logger.Log(LogLevel.Information,"Removing PAR {par_id}",id);
        
        return context
            .PushedAuthorizationRequests
            .Where(par => par.ReferenceValueHash == id)
            .ExecuteDeleteAsync();
    }
}