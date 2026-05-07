// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Interface for the token request validator
/// </summary>
public interface ITokenRequestValidator
{
    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="parameters">The form parameters from the token request.</param>
    /// <param name="clientValidationResult">The result of authenticating the client making the request.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenRequestValidationResult"/> indicating whether the token request is valid and carrying the validated request context.
    /// </returns>
    Task<TokenRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult);
}