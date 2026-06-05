// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;

namespace Open.IdentityServer.Endpoints;

internal class PushedAuthorizationEndpoint : IEndpointHandler
{
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        return new PushedAuthorizationResult();
    }
}