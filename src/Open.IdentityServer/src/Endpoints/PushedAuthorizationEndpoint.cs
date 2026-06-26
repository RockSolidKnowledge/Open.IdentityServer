// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;

#nullable  enable
namespace Open.IdentityServer.Endpoints;

internal class PushedAuthorizationRequestEndpoint(
    IdentityServerOptions options,
    IClientSecretValidator clientSecretValidator,
    IPushedAuthorizationRequestValidator validator ,
    IPushedAuthorizationResponseGenerator responseGenerator,
    ILogger<PushedAuthorizationRequestEndpoint> logger) : IEndpointHandler
{
    public async Task<IEndpointResult> ProcessAsync(HttpContext requestContext)
    {
        if ( options.Endpoints.EnablePushedAuthorizationRequestEndpoint == false)
        {
            return new StatusCodeResult(HttpStatusCode.NotFound);
        }
        
        logger.LogDebug("Start processing pushed authorization request");
        if (!HttpMethods.IsPost(requestContext.Request.Method))
        {
            return Error(OidcConstants.TokenErrors.InvalidRequest);
        }

        ClientSecretValidationResult? clientValidationResult = await clientSecretValidator.ValidateAsync(requestContext);
        if ( clientValidationResult.IsError)
        {
            return Error(OidcConstants.TokenErrors.InvalidClient);
        }

        NameValueCollection? parParameters = await ParseForm(requestContext.Request);
        if (parParameters == null)
        {
            return Error(OidcConstants.TokenErrors.InvalidRequest);
        }
        var validationContext = new PushedAuthorizationRequestValidationContext(parParameters);
        return await ProcessRequest(requestContext, validationContext);
    }

    private async Task<IEndpointResult> ProcessRequest(
        HttpContext requestContext,
        PushedAuthorizationRequestValidationContext validationContext)
    {
        PushAuthorizationRequestValidationResult result = await validator
            .ValidateAsync(validationContext, requestContext.RequestAborted);

        if (result.IsError)
        {
            return new BadRequestResult(result.Error, result.ErrorDescription);
        }
    
        PushedAuthorizationResponse response = await responseGenerator
            .CreateResponseAsync(result.ValidatedAuthorizeRequest);
        
        
        logger.LogTrace("End processing pushed authorization request");
        return new PushedAuthorizationResult(response);
    }
    
    private async Task<NameValueCollection?> ParseForm(HttpRequest request)
    {
        try
        {
            IFormCollection form = await request.ReadFormAsync();
            NameValueCollection parParameters = form.AsNameValueCollection();

            return parParameters;
        }
        catch (InvalidOperationException  )
        {
            return null;
        }
    }
    
    private TokenErrorResult Error(string error, string? errorDescription = null, Dictionary<string, object>? custom = null)
    {
        var response = new TokenErrorResponse
        {
            Error = error,
            ErrorDescription = errorDescription,
            Custom = custom
        };

        logger.LogError("PushedAuthorizationRequest error: {error}:{errorDescriptions}", error, error ?? "-no message-");

        return new TokenErrorResult(response);
    }
}