// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Interface for the token revocation request validator
/// </summary>
public interface ITokenRevocationRequestValidator
{
    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="validationContext">The context encapsulating the form parameters from the token revocation request and the authenticated client making the request.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenRevocationRequestValidationResult"/> indicating whether the revocation request is valid.
    /// </returns>
    Task<TokenRevocationRequestValidationResult> ValidateRequestAsync(TokenRevocationRequestValidationContext validationContext);
}