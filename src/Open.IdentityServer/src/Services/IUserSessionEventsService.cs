// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Service responsible handling user session events
/// </summary>
public interface IUserSessionEventsService
{
    /// <summary>
    /// Triggered when session logout occurs
    /// </summary>
    /// <param name="sessionEventContext">context needed for handling logout event</param>
    /// <returns></returns>
    public Task HandleUserSessionLogout(UserSessionEventContext sessionEventContext);
    
    /// <summary>
    /// Triggered when session expires
    /// </summary>
    /// <param name="sessionEventContext">context needed for handling logout event</param>
    /// <returns></returns>
    public Task HandleUserSessionExpiry(UserSessionEventContext sessionEventContext);
}