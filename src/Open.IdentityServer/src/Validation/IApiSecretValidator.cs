// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Validator for handling API client authentication.
/// </summary>
public interface IApiSecretValidator
{
    /// <summary>
    /// Tries to authenticate an API client based on the incoming request
    /// </summary>
    /// <param name="context">The HTTP context containing the incoming request to authenticate.</param>
    /// <returns>
    /// A task that resolves to an <see cref="ApiSecretValidationResult"/> indicating whether the API client was successfully authenticated.
    /// </returns>
    Task<ApiSecretValidationResult> ValidateAsync(HttpContext context);
}