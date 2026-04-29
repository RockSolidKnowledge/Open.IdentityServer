// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// Models a user's authentication session
/// </summary>
public interface IUserSession
{
    /// <summary>
    /// Creates a session identifier for the signin context and issues the session id cookie.
    /// </summary>
    /// <param name="principal">The authenticated claims principal for the current user.</param>
    /// <param name="properties">The authentication properties associated with the sign-in.</param>
    /// <returns>
    /// A task that resolves to the session identifier created for the current sign-in context.
    /// </returns>
    Task<string> CreateSessionIdAsync(ClaimsPrincipal principal, AuthenticationProperties properties);

    /// <summary>
    /// Gets the current authenticated user.
    /// </summary>
    /// <returns>
    /// A task that resolves to the current authenticated <see cref="ClaimsPrincipal"/>, or <c>null</c> if no user is authenticated.
    /// </returns>
    Task<ClaimsPrincipal> GetUserAsync();

    /// <summary>
    /// Gets the current session identifier.
    /// </summary>
    /// <returns>
    /// A task that resolves to the current session identifier, or <c>null</c> if no session exists.
    /// </returns>
    Task<string> GetSessionIdAsync();

    /// <summary>
    /// Ensures the session identifier cookie is present for the current session, creating it if necessary.
    /// </summary>
    Task EnsureSessionIdCookieAsync();

    /// <summary>
    /// Removes the session identifier cookie.
    /// </summary>
    Task RemoveSessionIdCookieAsync();

    /// <summary>
    /// Adds a client to the list of clients the user has signed in to during their session.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    Task AddClientIdAsync(string clientId);

    /// <summary>
    /// Gets the list of clients the user has signed in to during their session.
    /// </summary>
    /// <returns>
    /// A task that resolves to a collection of client identifiers the user has signed into during their current session.
    /// </returns>
    Task<IEnumerable<string>> GetClientListAsync();
}