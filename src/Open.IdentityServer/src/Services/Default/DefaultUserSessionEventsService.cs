// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
/// <param name="serviceProvider">service provider</param>
/// <param name="idsOptions">IdentityServer options</param>
/// <param name="telemetry">telemetry service</param>
/// <param name="timeProvider">time provider</param>
/// <param name="logger">logger</param>
public class DefaultUserSessionEventsService(
    IClientStore clientStore,
    IPersistedGrantStore persistedGrantStore,
    IBackChannelLogoutService backChannelLogoutService,
    IServiceProvider serviceProvider,
    IdentityServerOptions idsOptions,
    ITelemetryService telemetry,
    TimeProvider timeProvider,
    ILogger<DefaultUserSessionEventsService> logger) : IUserSessionEventsService
{
    /// <inheritdoc />
    public async Task HandleUserSessionLogout(EndUserSessionEventContext sessionEventContext)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);
        
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SubjectId);
        
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
    public async Task HandleUserSessionExpiry(EndUserSessionEventContext sessionEventContext)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);
        
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionEventContext.SubjectId);
        
        string[]? clientToNotify = await EndSessionForClients(sessionEventContext);

        List<string> backChannelClients = (idsOptions.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout
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

    /// <inheritdoc />
    public async Task<bool> ValidateSession(ValidateUserSessionEventContext sessionEventContext)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);
        
        IServerSessionTicketStore? serverSessionTicketStore = serviceProvider.GetService<IServerSessionTicketStore>();
        IIdentityServerServerSideSessionStore? identityServerServerSideSessionStore = serviceProvider.GetService<IIdentityServerServerSideSessionStore>();
        
        if (serverSessionTicketStore == null || identityServerServerSideSessionStore == null ||
            !ShouldCoordinateLifetimes(sessionEventContext.Client))
        {
            return true;
        }

        List<AuthenticationTicketFilterResult> sessions =
            (await serverSessionTicketStore.FilterServerAuthenticationTickets(sessionEventContext.SubjectId,
                sessionEventContext.SessionId))
            .ToList();

        if (sessions.Count == 0 || sessions.All(x => x.Session.Expires.HasValue &&
                                                     x.Session.Expires < timeProvider.GetUtcNow()))
        {
            logger.LogDebug("");
            return false;
        }

        foreach (AuthenticationTicketFilterResult session in sessions)
        {
            TimeSpan? diff = session.Session.Expires - session.Session.Renewed;
            session.Session.Renewed = timeProvider.GetUtcNow().UtcDateTime;
            session.Session.Expires = session.Session.Renewed + diff;

            if (idsOptions.Authentication.CookieSlidingExpiration &&
                session.AuthTicket?.Properties is { IsPersistent: true, AllowRefresh: true or null })
            {
                session.AuthTicket.Properties.IssuedUtc = session.Session.Renewed;
                session.AuthTicket.Properties.ExpiresUtc = session.Session.Expires;
                session.AuthTicket.Properties.SetString(IdentityServerConstants.ForceCookieRefresh, string.Empty);
                await serverSessionTicketStore.RenewAsync(session.Session.Key, session.AuthTicket);
            }
            else
            {
                await identityServerServerSideSessionStore.UpdateSession(session.Session);
            }
        }

        return true;
    }

    private async Task<string[]?> EndSessionForClients(EndUserSessionEventContext sessionEventContext)
    {
        string[] clientIds = await ClientIdsToCoordinate(sessionEventContext).ToArrayAsync();

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

    private async IAsyncEnumerable<string> ClientIdsToCoordinate(EndUserSessionEventContext sessionEventContext)
    {
        foreach (string clientId in sessionEventContext.ClientIds ?? [])
        {
            Client? client = await clientStore.FindClientByIdAsync(clientId);

            if (ShouldCoordinateLifetimes(client))
            {
                yield return client.ClientId;
            }
        }
    }

    private bool ShouldCoordinateLifetimes(Client? client) => client != null && (client.CoordinateLifetimeWithUserSession ?? 
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession);
}