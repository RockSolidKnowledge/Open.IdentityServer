// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Models making HTTP requests for JWTs from the authorize endpoint.
/// </summary>
public interface IJwtRequestUriHttpClient
{
    /// <summary>
    /// Gets a JWT from the url.
    /// </summary>
    /// <param name="url">The URI from which to fetch the JWT request object.</param>
    /// <param name="client">The client making the authorization request, used to apply any client-specific HTTP settings.</param>
    /// <returns>The raw JWT string retrieved from <paramref name="url"/>, or <see langword="null"/> when the request fails.</returns>
    Task<string> GetJwtAsync(string url, Client client);
}