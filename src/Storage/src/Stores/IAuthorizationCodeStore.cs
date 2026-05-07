// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for the authorization code store
/// </summary>
public interface IAuthorizationCodeStore
{
    /// <summary>
    /// Stores the authorization code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>The handle (key) under which the authorization code was stored.</returns>
    Task<string> StoreAuthorizationCodeAsync(AuthorizationCode code);

    /// <summary>
    /// Gets the authorization code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>The <see cref="AuthorizationCode"/> associated with <paramref name="code"/>, or <see langword="null"/> when not found.</returns>
    Task<AuthorizationCode> GetAuthorizationCodeAsync(string code);

    /// <summary>
    /// Removes the authorization code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>A <see cref="Task"/> that completes once the authorization code has been removed.</returns>
    Task RemoveAuthorizationCodeAsync(string code);
}