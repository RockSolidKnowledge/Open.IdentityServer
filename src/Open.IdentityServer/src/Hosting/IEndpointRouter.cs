// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Http;

namespace Open.IdentityServer.Hosting;

/// <summary>
/// The endpoint router
/// </summary>
public interface IEndpointRouter
{
    /// <summary>
    /// Finds a matching endpoint.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The <see cref="IEndpointHandler"/> that matches the request path, or <see langword="null"/> if no endpoint matches.</returns>
    IEndpointHandler Find(HttpContext context);
}