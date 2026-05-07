// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace Open.IdentityServer.Hosting;

/// <summary>
/// Extension methods for configuring CORS middleware in the IdentityServer pipeline.
/// </summary>
public static class CorsMiddlewareExtensions
{
    /// <summary>
    /// Adds the CORS middleware to the application pipeline using the CORS policy name configured in <see cref="IdentityServerOptions"/>.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void ConfigureCors(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();
        app.UseCors(options.Cors.CorsPolicyName);
    }
}