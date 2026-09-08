// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System.Threading.Tasks;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.ResponseHandling;

/// <summary>
/// Used to create a PAR response
/// </summary>
public interface IPushedAuthorizationResponseGenerator
{
    /// <summary>
    /// Creates a response to the pushed authorization request, generating the Unique URI for the request.
    /// </summary>
    /// <param name="request">The validated authorization request</param>
    /// <returns>A response that can be returned to the client</returns>
    Task<PushedAuthorizationResponse> CreateResponseAsync(ValidatedAuthorizeRequest request);
}