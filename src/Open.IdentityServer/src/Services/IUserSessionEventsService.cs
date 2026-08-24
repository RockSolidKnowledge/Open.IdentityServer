// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Service responsible for handling user session events
/// </summary>
public interface IUserSessionEventsService
{
    /// <summary>
    /// Triggered when the session logout occurs
    /// </summary>
    /// <param name="sessionEventContext">context needed for handling logout event</param>
    /// <returns></returns>
    public Task HandleUserSessionLogout(EndUserSessionEventContext sessionEventContext);
    
    /// <summary>
    /// Triggered when the session expires
    /// </summary>
    /// <param name="sessionEventContext">context needed for handling logout event</param>
    /// <returns></returns>
    public Task HandleUserSessionExpiry(EndUserSessionEventContext sessionEventContext);
    
    /// <summary>
    /// Checks for a valid session using the provided context
    /// </summary>
    /// <param name="sessionEventContext">context needed for handling session validation</param>
    /// <returns>boolean value to indicate if valid session exists</returns>
    public Task<bool> ValidateSession(ValidateUserSessionEventContext sessionEventContext);
}