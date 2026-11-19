// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Security.Claims;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Encapsulates the context for an authorize request, used for validation by <see cref="IAuthorizeRequestValidator"/>.
/// </summary>
public class AuthorizeRequestValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeRequestValidationContext"/> class.
    /// </summary>
    /// <param name="parameters">The query string or form parameters.</param>
    /// <param name="subject">The currently authenticated user, or <see langword="null"/> when the user has not yet signed in.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameters"/> is <see langword="null"/>.</exception>
    public AuthorizeRequestValidationContext(NameValueCollection parameters, ClaimsPrincipal subject = null)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Subject = subject;
    }

    /// <summary>
    /// Gets the query string or form parameters.
    /// </summary>
    public NameValueCollection Parameters { get; }

    /// <summary>
    /// Gets the currently authenticated user, or <see langword="null"/> when the user has not yet signed in.
    /// </summary>
    public ClaimsPrincipal Subject { get; }
}
