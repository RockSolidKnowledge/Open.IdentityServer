// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Services;

/// <summary>
/// Default Session management service, has methods for querying sessions and removing them.
/// </summary>
/// <param name="persistedGrantStore">persisted grant store</param>
/// <param name="backChannelLogoutService">back channel logout service</param>
/// <param name="serverSessionTicketStore">auth ticket store</param>
/// <param name="serverSessionStore">server session store</param>
/// <param name="telemetry">telemetry service</param>
public class DefaultSessionManagementService(
    IPersistedGrantStore persistedGrantStore,
    IBackChannelLogoutService backChannelLogoutService,
    IServerSessionTicketStore serverSessionTicketStore,
    IIdentityServerServerSideSessionStore serverSessionStore,
    ITelemetryService telemetry): ISessionManagementService
{
    /// <inheritdoc />
    public async Task<QueryResult<UserSession>> QuerySessionsAsync(SessionQuery? filter, CancellationToken ct = default)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);

        QueryResult<AuthenticationTicketFilterResult> results = await serverSessionTicketStore.FilterServerAuthenticationTickets(filter, ct);

        return results.MapTo<UserSession>(x => x.ToUserSession());
    }

    /// <inheritdoc />
    public async Task RemoveSessionsAsync(RemoveSessionsContext context, CancellationToken ct = default)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);

        if (context.SendBackchannelLogoutNotification)
        {
            var sessions = await serverSessionTicketStore.FilterServerAuthenticationTickets(context.SubjectId, context.SessionId);
            foreach (var sess in sessions)
            {
                List<string>? sessionClientList = sess.AuthTicket?.Properties.GetClientList().ToList();
                string[] clientIds = [];

                if (!sessionClientList.IsNullOrEmpty() && !context.ClientIds.IsNullOrEmpty())
                {
                    clientIds = sessionClientList!.Where(x => context.ClientIds!.Contains(x)).ToArray();
                }
                
                await backChannelLogoutService.SendLogoutNotificationsAsync(new LogoutNotificationContext
                {
                    SubjectId = sess.Session.SubjectId,
                    SessionId = sess.Session.SessionId,
                    ClientIds = clientIds,
                });
            }
        }
        
        if (context.RevokeTokens || context.RevokeConsents)
        {
            List<string> typeFilter = [];

            if (context.RevokeTokens)
            {
                typeFilter.AddRange(IdentityServerConstants.PersistedGrantTypes.PersistedGrantTokenTypes);
            }

            if (context.RevokeConsents)
            {
                typeFilter.Add(IdentityServerConstants.PersistedGrantTypes.UserConsent);
            }
            
            await persistedGrantStore.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = context.SubjectId,
                SessionId = context.SessionId,
                ClientIds = context.ClientIds?.ToArray() ?? [],
                Types = typeFilter.ToArray(),
            });
        }
        
        if (context.RemoveServerSideSession)
        {
            await serverSessionStore.DeleteSessions(context.SubjectId, context.SessionId);
        }
    }
}