// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Encapsulates the context for validating a redirect URI or a post logout redirect URI, used by <see cref="IRedirectUriValidator"/>.
/// </summary>
public class RedirectUriValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUriValidationContext"/> class.
    /// </summary>
    /// <param name="requestedUri">The requested URI, or <see langword="null"/> when none was supplied.</param>
    /// <param name="client">The client.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public RedirectUriValidationContext(string? requestedUri, Client client)
    {
        RequestedUri = requestedUri;
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Gets the requested URI, or <see langword="null"/> when none was supplied.
    /// </summary>
    public string? RequestedUri { get; }

    /// <summary>
    /// Gets the client.
    /// </summary>
    public Client Client { get; }
}
