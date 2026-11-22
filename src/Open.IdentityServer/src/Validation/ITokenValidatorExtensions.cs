// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Extension methods for <see cref="ITokenValidator"/>.
/// </summary>
public static class ITokenValidatorExtensions
{
    /// <summary>
    /// Validates an access token.
    /// </summary>
    /// <param name="validator">The token validator.</param>
    /// <param name="token">The access token.</param>
    /// <param name="expectedScope">The scope the token is expected to contain, or <c>null</c> to skip scope validation.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenValidationResult"/> indicating whether the access token is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="validator"/> or <paramref name="token"/> is <c>null</c>.</exception>
    public static Task<TokenValidationResult> ValidateAccessTokenAsync(
        this ITokenValidator validator,
        string token,
        string? expectedScope = null)
    {
        ArgumentNullException.ThrowIfNull(validator);

        return validator.ValidateAccessTokenAsync(new AccessTokenValidationContext(token, expectedScope));
    }

    /// <summary>
    /// Validates an identity token.
    /// </summary>
    /// <param name="validator">The token validator.</param>
    /// <param name="token">The identity token to validate.</param>
    /// <param name="clientId">The client identifier, or <c>null</c> to read it from the token.</param>
    /// <param name="validateLifetime">if set to <c>true</c> the lifetime gets validated. Otherwise not.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenValidationResult"/> indicating whether the identity token is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="validator"/> or <paramref name="token"/> is <c>null</c>.</exception>
    public static Task<TokenValidationResult> ValidateIdentityTokenAsync(
        this ITokenValidator validator,
        string token,
        string? clientId = null,
        bool validateLifetime = true)
    {
        ArgumentNullException.ThrowIfNull(validator);

        return validator.ValidateIdentityTokenAsync(new IdentityTokenValidationContext(token, clientId, validateLifetime));
    }
}
