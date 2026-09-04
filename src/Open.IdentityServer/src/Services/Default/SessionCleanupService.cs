// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.EntityFramework;

/// <summary>
/// Helper to clean up expired server-side sessions
/// </summary>
/// <param name="options">IdentityServer options</param>
/// <param name="serverSideSessionStore">server side sessions store</param>
/// <param name="userSessionEventsService">user session events service</param>
/// <param name="telemetry">telemetry service</param>
/// <param name="logger">logger</param>
public class SessionCleanupService(
    IdentityServerOptions options,
    IServerSessionTicketStore serverSideSessionStore,
    IUserSessionEventsService userSessionEventsService,
    ITelemetryService telemetry,
    ILogger<SessionCleanupService> logger)
{
    /// <summary>
    /// Method to clear expired server-side sessions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all expired grants and device codes have been removed.</returns>
    public async Task RemoveExpiredServerSideSessionsAsync()
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);
        
        try
        {
            logger.LogTrace("Querying for expired sessions to remove");

            await RemoveServerSideSessionsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Exception removing expired sessions: {Exception}", ex.Message);
        }
    }
    
    /// <summary>
    /// Removes the expired sessions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all batches of expired sessions have been deleted.</returns>
    private async Task RemoveServerSideSessionsAsync()
    {
        var found = Int32.MaxValue;

        while (found >= options.ServerSideSessions.RemoveExpiredSessionsBatchSize)
        {
            var expiredSessions = (await serverSideSessionStore
                .GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
                .ToList();
            
            found = expiredSessions.Count;
            logger.LogInformation("Removed {ExpiredSessionsCount} expired server side sessions", found);
            
            if (found > 0)
            {
                foreach (var expiredSession in expiredSessions)
                {
                    // TODO, finish implementing, should get auth ticket so clientIds can be populated
                    await userSessionEventsService.HandleUserSessionExpiry(new EndUserSessionEventContext()
                    {
                        SubjectId = expiredSession.Session.SubjectId,
                        SessionId = expiredSession.Session.SessionId,
                        ClientIds = expiredSession.AuthTicket?.Properties.GetClientList().ToArray() ?? [], 
                    });
                }
            }
        }
    }
}