// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for persisting any type of grant.
/// </summary>
public interface IPersistedGrantStore
{
    /// <summary>
    /// Stores the grant.
    /// </summary>
    /// <param name="grant">The grant.</param>
    /// <returns>A <see cref="Task"/> that completes once the grant has been persisted.</returns>
    Task StoreAsync(PersistedGrant grant);

    /// <summary>
    /// Gets the grant.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="PersistedGrant"/> matching <paramref name="key"/>, or <see langword="null"/> when not found.</returns>
    Task<PersistedGrant> GetAsync(string key);

    /// <summary>
    /// Gets all grants based on the filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>All <see cref="PersistedGrant"/> records matching the supplied <paramref name="filter"/>.</returns>
    Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter);

    /// <summary>
    /// Removes the grant by key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>A <see cref="Task"/> that completes once the grant has been removed.</returns>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all grants based on the filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>A <see cref="Task"/> that completes once all matching grants have been removed.</returns>
    Task RemoveAllAsync(PersistedGrantFilter filter);
}