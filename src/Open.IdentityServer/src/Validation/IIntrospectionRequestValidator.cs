// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Interface for the introspection request validator
/// </summary>
public interface IIntrospectionRequestValidator
{
    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="parameters">The form parameters from the introspection request.</param>
    /// <param name="api">The authenticated API resource making the introspection request.</param>
    /// <returns>
    /// A task that resolves to an <see cref="IntrospectionRequestValidationResult"/> indicating whether the token is active and carrying the associated claims.
    /// </returns>
    Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ApiResource api);
}