// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.EntityFramework.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.EntityFramework.Mappers;

namespace Open.IdentityServer.EntityFramework.Stores;

/// <summary>
/// EF implementation of IIdentityProviderStore
/// </summary>
public class IdentityServerIdentityProviderStore : IIdentityProviderStore
{
    private readonly IConfigurationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityServerIdentityProviderStore"/> class.
    /// </summary>
    /// <param name="dbContext">The configuration database context.</param>
    public IdentityServerIdentityProviderStore(IConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets all identity provider configuration
    /// </summary>
    public async Task<IEnumerable<IdentityServerIdentityProvider>> GetAllIdentityProviders()
    {
        return await _dbContext.IdentityProviders
            .Select(x => x.ToModel())
            .ToListAsync();
    }

    /// <summary>
    /// Gets identity provider configuration by scheme name
    /// </summary>
    /// <param name="scheme">The scheme name of the identity provider.</param>
    /// <returns>The identity provider configuration.</returns>
    public async Task<IdentityServerIdentityProvider> GetIdentityProviderByScheme(string scheme)
    {
        return await _dbContext.IdentityProviders
            .Where(x => x.Scheme == scheme)
            .Select(x => x.ToModel())
            .FirstOrDefaultAsync();
    }
}