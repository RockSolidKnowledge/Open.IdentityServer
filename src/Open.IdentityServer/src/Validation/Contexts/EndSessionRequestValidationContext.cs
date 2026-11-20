// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Specialized;
using System.Security.Claims;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Encapsulates the context for an end session request, used for validation by <see cref="IEndSessionRequestValidator"/>.
/// </summary>
public class EndSessionRequestValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionRequestValidationContext"/> class.
    /// </summary>
    /// <param name="parameters">The query string or form parameters from the end-session request.</param>
    /// <param name="subject">The currently authenticated user, or <see langword="null"/> when the user is not authenticated.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameters"/> is <see langword="null"/>.</exception>
    public EndSessionRequestValidationContext(NameValueCollection parameters, ClaimsPrincipal? subject = null)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Subject = subject;
    }

    /// <summary>
    /// Gets the query string or form parameters from the end-session request.
    /// </summary>
    public NameValueCollection Parameters { get; }

    /// <summary>
    /// Gets the currently authenticated user, or <see langword="null"/> when the user is not authenticated.
    /// </summary>
    public ClaimsPrincipal? Subject { get; }
}
