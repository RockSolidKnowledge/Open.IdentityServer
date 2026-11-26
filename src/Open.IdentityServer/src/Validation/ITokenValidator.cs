// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Interface for the token validator
/// </summary>
public interface ITokenValidator
{
    /// <summary>
    /// Validates an access token.
    /// </summary>
    /// <param name="validationContext">The context containing the access token and the scope it is expected to contain.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenValidationResult"/> indicating whether the access token is valid.
    /// </returns>
    Task<TokenValidationResult> ValidateAccessTokenAsync(AccessTokenValidationContext validationContext);

    /// <summary>
    /// Validates an identity token.
    /// </summary>
    /// <param name="validationContext">The context containing the identity token, the client identifier and whether to validate the lifetime.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenValidationResult"/> indicating whether the identity token is valid.
    /// </returns>
    Task<TokenValidationResult> ValidateIdentityTokenAsync(IdentityTokenValidationContext validationContext);
}
