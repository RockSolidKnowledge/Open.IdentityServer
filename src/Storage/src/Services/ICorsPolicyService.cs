// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// Service that determines if CORS is allowed.
/// </summary>
public interface ICorsPolicyService
{
    /// <summary>
    /// Determines whether origin is allowed.
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <returns><see langword="true"/> if the origin is allowed to make cross-origin requests; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsOriginAllowedAsync(string origin);
}