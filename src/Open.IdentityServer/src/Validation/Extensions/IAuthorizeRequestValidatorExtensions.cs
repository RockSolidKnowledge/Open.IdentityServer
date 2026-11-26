// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Extension methods for <see cref="IAuthorizeRequestValidator"/>.
/// </summary>
public static class IAuthorizeRequestValidatorExtensions
{
    /// <summary>
    ///  Validates authorize request parameters.
    ///  Convenience overload that creates an <see cref="AuthorizeRequestValidationContext"/>
    ///  and calls <see cref="IAuthorizeRequestValidator.ValidateAsync(AuthorizeRequestValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The authorize request validator.</param>
    /// <param name="parameters">The query string or form parameters from the authorize request.</param>
    /// <param name="subject">The currently authenticated user, or <see langword="null"/> when the user has not yet signed in.</param>
    /// <returns>An <see cref="AuthorizeRequestValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    public static Task<AuthorizeRequestValidationResult> ValidateAsync(
        this IAuthorizeRequestValidator validator,
        NameValueCollection parameters,
        ClaimsPrincipal? subject = null)
    {
        return validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters, subject));
    }
}
