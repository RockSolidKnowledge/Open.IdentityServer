// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;

namespace Open.IdentityServer.Validation;

/// <summary>
/// The context for validating an access token.
/// </summary>
public class AccessTokenValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccessTokenValidationContext"/> class.
    /// </summary>
    /// <param name="token">The access token.</param>
    /// <param name="expectedScope">The scope the token is expected to contain, or <c>null</c> to skip scope validation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> is <c>null</c>.</exception>
    public AccessTokenValidationContext(string token, string? expectedScope = null)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpectedScope = expectedScope;
    }

    /// <summary>
    /// Gets the access token.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Gets the scope the token is expected to contain, or <c>null</c> to skip scope validation.
    /// </summary>
    public string? ExpectedScope { get; }
}
