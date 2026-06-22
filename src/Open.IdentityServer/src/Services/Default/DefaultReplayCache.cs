// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Open.IdentityServer.Services;

/// <summary>
/// Default implementation of the replay cache using IDistributedCache
/// </summary>
public class DefaultReplayCache : IReplayCache
{
    private const string Prefix = nameof(DefaultReplayCache) + ":";
        
    private readonly IDistributedCache _cache;
    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultReplayCache"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache used to store and look up replay-detection entries.</param>
    /// <param name="telemetry">The telemetry service</param>
    public DefaultReplayCache(IDistributedCache cache, ITelemetryService telemetry)
    {
        _cache = cache;
        _telemetry = telemetry;
    }
        
    /// <inheritdoc />
    public async Task AddAsync(string purpose, string handle, DateTimeOffset expiration)
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        };
            
        await _cache.SetAsync(Prefix + purpose + handle, new byte[] { }, options);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string purpose, string handle)
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        return (await _cache.GetAsync(Prefix + purpose + handle, default)) != null;
    }
}