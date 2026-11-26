// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
    /// <param name="validationContext">The context encapsulating the query string or form parameters from the authorize request and the currently authenticated user (<see langword="null"/> when the user has not yet signed in).</param>
    /// <returns>An <see cref="AuthorizeRequestValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    Task<AuthorizeRequestValidationResult> ValidateAsync(AuthorizeRequestValidationContext validationContext);
}