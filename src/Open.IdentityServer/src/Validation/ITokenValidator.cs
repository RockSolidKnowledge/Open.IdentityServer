// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Interface for the token validator
/// </summary>
public interface ITokenValidator
{
    /// <summary>
    /// Validates an access token.
    /// </summary>
    /// <param name="token">The access token.</param>
    /// <param name="expectedScope">The scope the token is expected to contain, or <c>null</c> to skip scope validation.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenValidationResult"/> indicating whether the access token is valid.
    /// </returns>
    Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null);
        
    /// <summary>
    /// Validates an identity token.
    /// </summary>
    /// <param name="token">The identity token to validate.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="validateLifetime">if set to <c>true</c> the lifetime gets validated. Otherwise not.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenValidationResult"/> indicating whether the identity token is valid.
    /// </returns>
    Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true);
}