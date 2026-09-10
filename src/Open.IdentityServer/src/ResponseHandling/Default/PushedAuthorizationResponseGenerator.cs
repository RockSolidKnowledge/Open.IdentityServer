// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Threading.Tasks;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.ResponseHandling;

#nullable enable

/// <summary>
/// Default implementation of the pushed authorization response generator
/// </summary>
/// <param name="service">The service used to manage the storing of the pushed authorization request for later retrieval</param>
public class PushedAuthorizationResponseGenerator(IPushedAuthorizationRequestService service) : IPushedAuthorizationResponseGenerator
{
    /// <summary>
    /// Generates the Pushed Authorization Request response
    /// </summary>
    /// <param name="request">The request for which to generate a response</param>
    /// <returns>The generated response</returns>
    public async Task<PushedAuthorizationResponse> CreateResponseAsync(ValidatedAuthorizeRequest request)
    {
        PushedAuthorization response = await service.CreateAsync(request.Client, request.Raw);

        return new PushedAuthorizationResponse(response.Key, (long)response.ExpiresIn.TotalSeconds);
    }
}