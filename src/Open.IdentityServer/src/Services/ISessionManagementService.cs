// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Session management interface, defines methods for querying sessions and removing them.
/// </summary>
public interface ISessionManagementService
{
    /// <summary>
    /// Method for querying sessions
    /// </summary>
    /// <param name="filter">filter to be used</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>paginated query result of user sessions</returns>
    Task<QueryResult<UserSession>> QuerySessionsAsync(SessionQuery? filter, CancellationToken ct = default); 
    
    /// <summary>
    /// Method for removing sessions
    /// </summary>
    /// <param name="context">remove session context</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>void</returns>
    Task RemoveSessionsAsync(RemoveSessionsContext context, CancellationToken ct = default);
}