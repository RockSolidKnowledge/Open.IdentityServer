// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

#nullable enable

namespace Open.IdentityServer.Stores;

/// <summary>
/// Provides retrieval of identity provider configuration
/// </summary>
public interface IIdentityProviderStore
{
    /// <summary>
    /// Get all identity provider configuration
    /// </summary>
    Task<IEnumerable<IdentityServerIdentityProvider>> GetAllIdentityProviders();
    
    /// <summary>
    /// Get identity provider configuration by scheme name
    /// </summary>
    /// <param name="scheme">The scheme name of the identity provider</param>
    Task<IdentityServerIdentityProvider?> GetIdentityProviderByScheme(string scheme);
}