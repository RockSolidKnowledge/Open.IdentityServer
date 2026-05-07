// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Open.IdentityServer.Hosting;

/// <summary>
/// Endpoint result
/// </summary>
public interface IEndpointResult
{
    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that completes when the result has been written to the HTTP response.</returns>
    Task ExecuteAsync(HttpContext context);
}