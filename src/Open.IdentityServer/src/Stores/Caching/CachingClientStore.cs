// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Cache decorator for IClientStore
/// </summary>
/// <typeparam name="T">The underlying <see cref="IClientStore"/> implementation being decorated with caching.</typeparam>
/// <seealso cref="Open.IdentityServer.Stores.IClientStore" />
public class CachingClientStore<T> : IClientStore
    where T : IClientStore
{
    private readonly IdentityServerOptions _options;
    private readonly ICache<Client> _cache;
    private readonly ITelemetryService _telemetry;
    private readonly IClientStore _inner;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingClientStore{T}"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="inner">The inner.</param>
    /// <param name="cache">The cache.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <param name="logger">The logger.</param>
    public CachingClientStore(
        IdentityServerOptions options, 
        T inner, 
        ICache<Client> cache, 
        ITelemetryService telemetry,
        ILogger<CachingClientStore<T>> logger)
    {
        _options = options;
        _inner = inner;
        _cache = cache;
        _telemetry = telemetry;
        _logger = logger;
    }

    /// <summary>
    /// Finds a client by id
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>
    /// The client
    /// </returns>
    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Cache, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Client, clientId);
        
        var client = await _cache.GetAsync(clientId,
            _options.Caching.ClientStoreExpiration,
            async () => await _inner.FindClientByIdAsync(clientId),
            _logger);

        return client;
    }
}