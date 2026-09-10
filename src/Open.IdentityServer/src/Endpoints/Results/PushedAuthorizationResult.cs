// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.ResponseHandling;

namespace Open.IdentityServer.Endpoints.Results;

internal record PushedAuthorizationResult(PushedAuthorizationResponse Response) : IEndpointResult
{
    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status201Created;
        context.Response.SetNoCache();
        
        await context.Response.WriteAsJsonAsync(Response);
    }
}