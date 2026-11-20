// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Specialized;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Encapsulates the context for an introspection request, used for validation by <see cref="IIntrospectionRequestValidator"/>.
/// </summary>
public class IntrospectionRequestValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntrospectionRequestValidationContext"/> class.
    /// </summary>
    /// <param name="parameters">The form parameters from the introspection request.</param>
    /// <param name="api">The authenticated API resource making the introspection request, or <see langword="null"/> when not available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameters"/> is <see langword="null"/>.</exception>
    public IntrospectionRequestValidationContext(NameValueCollection parameters, ApiResource? api = null)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Api = api;
    }

    /// <summary>
    /// Gets the form parameters from the introspection request.
    /// </summary>
    public NameValueCollection Parameters { get; }

    /// <summary>
    /// Gets the authenticated API resource making the introspection request, or <see langword="null"/> when not available.
    /// </summary>
    public ApiResource? Api { get; }
}
