// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Services.Default;

/// <summary>
/// Default user session event handler for Open.IdentityServer
/// </summary>
/// <param name="clientStore">client store</param>
/// <param name="persistedGrantStore">persisted grant store</param>
/// <param name="backChannelLogoutService">back channel logout service</param>
/// <param name="idsOptions">IdentityServer options</param>
/// <param name="telemetry">telemetry service</param>
/// <param name="logger">logger</param>
public class DefaultUserSessionEventsService(
    IClientStore clientStore,
    IPersistedGrantStore persistedGrantStore,
    IBackChannelLogoutService backChannelLogoutService,
    IdentityServerOptions idsOptions,
    ITelemetryService telemetry,
    ILogger<DefaultUserSessionEventsService> logger) : IUserSessionEventsService
{
    /// <inheritdoc />
    public async Task HandleUserSessionLogout(UserSessionEventContext sessionEventContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SubjectId);
        
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);
        
        if (sessionEventContext.ClientIds.Length == 0)
        {
            logger.LogInformation("no clients linked to session, nothing to be done");
            return;
        }
        
        await EndSessionForClients(sessionEventContext);
        
        await backChannelLogoutService.SendLogoutNotificationsAsync(new LogoutNotificationContext
        {
            SubjectId = sessionEventContext.SubjectId,
            SessionId = sessionEventContext.SessionId,
            ClientIds = sessionEventContext.ClientIds,
        });
    }

    /// <inheritdoc />
    public async Task HandleUserSessionExpiry(UserSessionEventContext sessionEventContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SubjectId);
        
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);
        
        var clientToNotify = await EndSessionForClients(sessionEventContext);

        var backChannelClients = (idsOptions.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout
            ? sessionEventContext.ClientIds
            : clientToNotify ?? []).ToList();

        if (backChannelClients.Count == 0)
        {
            logger.LogInformation("no backchannel clients to notify");
            return;
        }
        
        await backChannelLogoutService.SendLogoutNotificationsAsync(new LogoutNotificationContext
        {
            SubjectId = sessionEventContext.SubjectId,
            SessionId = sessionEventContext.SessionId,
            ClientIds = backChannelClients,
        });
    }

    private async Task<string[]?> EndSessionForClients(UserSessionEventContext sessionEventContext)
    {
        var clientIds = await ClientIdsToCoordinate(sessionEventContext).ToArrayAsync();

        if (clientIds.Length == 0)
        {
            logger.LogInformation("no clients to remove grants for");
            return null;
        }

        await persistedGrantStore.RemoveAllAsync(new PersistedGrantFilter
        {
            SubjectId = sessionEventContext.SubjectId,
            SessionId = sessionEventContext.SessionId,
            ClientIds = sessionEventContext.ClientIds,
            Types = IdentityServerConstants.PersistedGrantTypes.PersistedGrantTokenTypes
        });

        return clientIds;
    }

    private async IAsyncEnumerable<string> ClientIdsToCoordinate(UserSessionEventContext sessionEventContext)
    {
        foreach (string clientId in sessionEventContext.ClientIds ?? [])
        {
            var client = await clientStore.FindClientByIdAsync(clientId);

            if (client != null && (client.CoordinateLifetimeWithUserSession ??
                                   idsOptions.Authentication.CoordinateClientLifetimesWithUserSession))
            {
                yield return client.ClientId;
            }
        }
    }
}