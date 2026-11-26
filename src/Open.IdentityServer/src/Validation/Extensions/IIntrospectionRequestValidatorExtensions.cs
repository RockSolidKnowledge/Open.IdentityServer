// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Specialized;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Extension methods for <see cref="IIntrospectionRequestValidator"/>.
/// </summary>
public static class IIntrospectionRequestValidatorExtensions
{
    /// <summary>
    /// Validates the request.
    /// Convenience overload that creates an <see cref="IntrospectionRequestValidationContext"/>
    /// and calls <see cref="IIntrospectionRequestValidator.ValidateAsync(IntrospectionRequestValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The introspection request validator.</param>
    /// <param name="parameters">The form parameters from the introspection request.</param>
    /// <param name="api">The authenticated API resource making the introspection request.</param>
    /// <returns>
    /// A task that resolves to an <see cref="IntrospectionRequestValidationResult"/> indicating whether the token is active and carrying the associated claims.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="parameters"/> is <see langword="null"/>.</exception>
    public static Task<IntrospectionRequestValidationResult> ValidateAsync(
        this IIntrospectionRequestValidator validator,
        NameValueCollection parameters,
        ApiResource? api)
    {
        return validator.ValidateAsync(new IntrospectionRequestValidationContext(parameters, api));
    }
}
