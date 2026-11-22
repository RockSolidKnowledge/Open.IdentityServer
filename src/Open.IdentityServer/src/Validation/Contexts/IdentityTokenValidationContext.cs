// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;

namespace Open.IdentityServer.Validation;

/// <summary>
/// The context for validating an identity token.
/// </summary>
public class IdentityTokenValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityTokenValidationContext"/> class.
    /// </summary>
    /// <param name="token">The identity token to validate.</param>
    /// <param name="clientId">The client identifier, or <c>null</c> to read it from the token.</param>
    /// <param name="validateLifetime">if set to <c>true</c> the lifetime gets validated. Otherwise not.</param>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> is <c>null</c>.</exception>
    public IdentityTokenValidationContext(string token, string? clientId = null, bool validateLifetime = true)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ClientId = clientId;
        ValidateLifetime = validateLifetime;
    }

    /// <summary>
    /// Gets the identity token to validate.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Gets the client identifier, or <c>null</c> if it should be read from the token.
    /// </summary>
    public string? ClientId { get; }

    /// <summary>
    /// Gets a value indicating whether the lifetime of the token gets validated.
    /// </summary>
    public bool ValidateLifetime { get; }
}
