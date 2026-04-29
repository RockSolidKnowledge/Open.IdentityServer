// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;



namespace Open.IdentityServer.Hosting;

/// <summary>
/// Middleware that sets the IdentityServer base path on the current HTTP context from the request's path base.
/// </summary>
public class BaseUrlMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IdentityServerOptions _options;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUrlMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The IdentityServer options.</param>
    public BaseUrlMiddleware(RequestDelegate next, IdentityServerOptions options)
    {
        _next = next;
        _options = options;
    }

    /// <summary>
    /// Processes the HTTP request by setting the IdentityServer base path and invoking the next middleware.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
            
        context.SetIdentityServerBasePath(request.PathBase.Value.RemoveTrailingSlash());

        await _next(context);
    }
}