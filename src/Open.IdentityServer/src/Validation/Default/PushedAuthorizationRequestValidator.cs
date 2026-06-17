// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Validation;

internal class PushedAuthorizationRequestValidator(
    IAuthorizeRequestValidator authorizeRequestValidator,
    ILogger<PushedAuthorizationRequestValidator> logger) : IPushedAuthorizationRequestValidator
{
    public async Task<PushAuthorizationRequestValidationResult> ValidateAsync(PushedAuthorizationRequestValidationContext validationContext, CancellationToken ct)
    {
        logger.LogDebug("Starting pushed authorization request validation");
       
        if (validationContext.RequestParameters.GetValues("request_uri") != null)
        {
            return new PushAuthorizationRequestValidationResult( "request_uri not allowed" , "request_uri can only be used at the authorization endpoint");
        }

        AuthorizeRequestValidationResult authorizeRequestValidationResult = await authorizeRequestValidator.ValidateAsync(validationContext.RequestParameters, null);
        
        if (authorizeRequestValidationResult.IsError)
        {
            return new PushAuthorizationRequestValidationResult(authorizeRequestValidationResult.Error, 
                authorizeRequestValidationResult.ErrorDescription);
        }
        
        logger.LogTrace("Pushed authorization request validation completed. Success.");
        
        return new PushAuthorizationRequestValidationResult(authorizeRequestValidationResult.ValidatedRequest);
    }
}