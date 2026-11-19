// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Authorize endpoint request validator.
/// </summary>
public interface IAuthorizeRequestValidator
{
    /// <summary>
    ///  Validates authorize request parameters.
    /// </summary>
    /// <param name="parameters">The query string or form parameters from the authorize request.</param>
    /// <param name="subject">The currently authenticated user, or <see langword="null"/> when the user has not yet signed in.</param>
    /// <returns>An <see cref="AuthorizeRequestValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null);
}