// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Specialized;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Encapsulates the context for a token revocation request, used for validation by <see cref="ITokenRevocationRequestValidator"/>.
/// </summary>
public class TokenRevocationRequestValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRevocationRequestValidationContext"/> class.
    /// </summary>
    /// <param name="parameters">The form parameters from the token revocation request.</param>
    /// <param name="client">The authenticated client making the revocation request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameters"/> or <paramref name="client"/> is <see langword="null"/>.</exception>
    public TokenRevocationRequestValidationContext(NameValueCollection parameters, Client client)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Gets the form parameters from the token revocation request.
    /// </summary>
    public NameValueCollection Parameters { get; }

    /// <summary>
    /// Gets the authenticated client making the revocation request.
    /// </summary>
    public Client Client { get; }
}
