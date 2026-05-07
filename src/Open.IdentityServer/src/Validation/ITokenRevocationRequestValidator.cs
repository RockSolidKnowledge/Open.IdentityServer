// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Interface for the token revocation request validator
/// </summary>
public interface ITokenRevocationRequestValidator
{
    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="parameters">The form parameters from the token revocation request.</param>
    /// <param name="client">The authenticated client making the revocation request.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenRevocationRequestValidationResult"/> indicating whether the revocation request is valid.
    /// </returns>
    Task<TokenRevocationRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, Client client);
}