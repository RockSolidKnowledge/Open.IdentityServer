// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Services;

/// <summary>
/// Default Session management service, has methods for querying sessions and removing them.
/// </summary>
/// <param name="persistedGrantService"></param>
/// <param name="backChannelLogoutService"></param>
/// <param name="serverSessionTicketStore"></param>
/// <param name="telemetry"></param>
/// <param name="logger"></param>
public class DefaultSessionManagementService(
    IPersistedGrantService persistedGrantService,
    IBackChannelLogoutService backChannelLogoutService,
    IServerSessionTicketStore serverSessionTicketStore,
    ITelemetryService telemetry,
    ILogger<DefaultSessionManagementService> logger): ISessionManagementService
{
    /// <inheritdoc />
    public Task<QueryResult<UserSession>> QuerySessionsAsync(SessionQuery? filter, CancellationToken ct = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task RemoveSessionsAsync(RemoveSessionsContext context, CancellationToken ct = default)
    {
        throw new System.NotImplementedException();
    }
}