// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Validation;

#nullable  enable
namespace Open.IdentityServer.Endpoints;

internal class PushedAuthorizationEndpoint(IPushedAuthorizationRequestValidator validator) : IEndpointHandler
{
    public async Task<IEndpointResult> ProcessAsync(HttpContext requestContext)
    {
        if (!HttpMethods.IsPost(requestContext.Request.Method))
        {
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        NameValueCollection? parParameters = await ParseForm(requestContext.Request);
        if (parParameters == null)
        {
            return new StatusCodeResult(HttpStatusCode.BadRequest);
        }
        PushedAuthorizationRequestValidationContext validationContext = new (parParameters);
        return await ProcessRequest(requestContext, validationContext);
    }

    private async Task<IEndpointResult> ProcessRequest(HttpContext requestContext,
        PushedAuthorizationRequestValidationContext validationContext)
    {
        PushAuthorizationRequestValidationResult result = await validator
            .ValidateAsync(validationContext, requestContext.RequestAborted);

        if (result.IsError)
        {
            return new BadRequestResult(result.Error, result.ErrorDescription);
        }
        
        // Generate URI
        // Store Validated Authorize Request
        
        return new PushedAuthorizationResult();
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
}