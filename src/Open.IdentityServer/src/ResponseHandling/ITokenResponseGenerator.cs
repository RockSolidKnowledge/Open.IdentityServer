// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.ResponseHandling;

/// <summary>
/// Interface for the token response generator
/// </summary>
public interface ITokenResponseGenerator
{
    /// <summary>
    /// Processes the response.
    /// </summary>
    /// <param name="validationResult">The validation result.</param>
    /// <returns>A task that resolves to a <see cref="TokenResponse"/> containing the issued tokens for the requested grant type.</returns>
    Task<TokenResponse> ProcessAsync(TokenRequestValidationResult validationResult);
}