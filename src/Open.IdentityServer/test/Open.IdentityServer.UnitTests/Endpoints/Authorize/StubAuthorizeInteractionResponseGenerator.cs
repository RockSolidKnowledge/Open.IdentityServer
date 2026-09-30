// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.Endpoints.Authorize;

internal class StubAuthorizeInteractionResponseGenerator : IAuthorizeInteractionResponseGenerator
{
    internal InteractionResponse Response { get; set; } = new InteractionResponse();
    public ConsentResponse SpiedConsent { get; private set; }

    public Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null)
    {
        SpiedConsent = consent;
        
        return Task.FromResult(Response);
    }
}