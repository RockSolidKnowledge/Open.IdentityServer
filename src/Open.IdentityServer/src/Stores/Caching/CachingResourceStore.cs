// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Caching decorator for IResourceStore
/// </summary>
/// <typeparam name="T">The underlying <see cref="IResourceStore"/> implementation being decorated with caching.</typeparam>
/// <seealso cref="Open.IdentityServer.Stores.IResourceStore" />
public class CachingResourceStore<T> : IResourceStore
    where T : IResourceStore
{
    private const string AllKey = "__all__";

    private readonly IdentityServerOptions _options;
        
    private readonly ICache<IEnumerable<IdentityResource>> _identityCache;
    private readonly ICache<IEnumerable<ApiResource>> _apiByScopeCache;
    private readonly ICache<IEnumerable<ApiScope>> _apiScopeCache;
    private readonly ICache<IEnumerable<ApiResource>> _apiResourceCache;
    private readonly ICache<Resources> _allCache;
        
    private readonly IResourceStore _inner;
    private readonly ITelemetryService _telemetry;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingResourceStore{T}"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="inner">The inner.</param>
    /// <param name="identityCache">The identity cache.</param>
    /// <param name="apiByScopeCache">The API by scope cache.</param>
    /// <param name="apisCache">The API cache.</param>
    /// <param name="scopeCache">The cache for API scope lookups by scope name.</param>
    /// <param name="allCache">All cache.</param>
    /// <param name="telemetry">The telemetry</param>
    /// <param name="logger">The logger.</param>
    public CachingResourceStore(IdentityServerOptions options, T inner, 
        ICache<IEnumerable<IdentityResource>> identityCache, 
        ICache<IEnumerable<ApiResource>> apiByScopeCache,
        ICache<IEnumerable<ApiResource>> apisCache,
        ICache<IEnumerable<ApiScope>> scopeCache,
        ICache<Resources> allCache,
        ITelemetryService telemetry,
        ILogger<CachingResourceStore<T>> logger)
    {
        _options = options;
        _inner = inner;
        _identityCache = identityCache;
        _apiByScopeCache = apiByScopeCache;
        _apiResourceCache = apisCache;
        _apiScopeCache = scopeCache;
        _allCache = allCache;
        _telemetry = telemetry;
        _logger = logger;
    }

    private string GetKey(IEnumerable<string> names)
    {
        if (names == null || !names.Any()) return string.Empty;
        return names.OrderBy(x => x).Aggregate((x, y) => x + "," + y);
    }

    /// <inheritdoc/>
    public async Task<Resources> GetAllResourcesAsync()
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Cache, this);
        
        var key = AllKey;

        var all = await _allCache.GetAsync(key,
            _options.Caching.ResourceStoreExpiration,
            async () => await _inner.GetAllResourcesAsync(),
            _logger);

        return all;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Cache, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Api, GetKey(apiResourceNames));

        var key = GetKey(apiResourceNames);

        var apis = await _apiResourceCache.GetAsync(key,
            _options.Caching.ResourceStoreExpiration,
            async () => await _inner.FindApiResourcesByNameAsync(apiResourceNames),
            _logger);

        return apis;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> names)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Cache, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Scope, GetKey(names));

        var key = GetKey(names);

        var identities = await _identityCache.GetAsync(key,
            _options.Caching.ResourceStoreExpiration,
            async () => await _inner.FindIdentityResourcesByScopeNameAsync(names),
            _logger);

        return identities;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> names)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Cache, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Scope, GetKey(names));

        var key = GetKey(names);

        var apis = await _apiByScopeCache.GetAsync(key,
            _options.Caching.ResourceStoreExpiration,
            async () => await _inner.FindApiResourcesByScopeNameAsync(names),
            _logger);

        return apis;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Cache, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Scope, GetKey(scopeNames));

        var key = GetKey(scopeNames);

        var apis = await _apiScopeCache.GetAsync(key,
            _options.Caching.ResourceStoreExpiration,
            async () => await _inner.FindApiScopesByNameAsync(scopeNames),
            _logger);

        return apis;
    }
}