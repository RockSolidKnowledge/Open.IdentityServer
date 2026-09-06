// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.Extensions.Caching.Distributed;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// The default device flow throttling service using IDistributedCache.
/// </summary>
/// <seealso cref="Open.IdentityServer.Services.IDeviceFlowThrottlingService" />
public class DistributedDeviceFlowThrottlingService : IDeviceFlowThrottlingService
{
    private readonly IDistributedCache _cache;
    private readonly IClientStore _clientStore;
    private readonly TimeProvider _clock;
    private readonly IdentityServerOptions _options;
    private readonly ITelemetryService _telemetry;

    private const string KeyPrefix = "devicecode_";

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedDeviceFlowThrottlingService"/> class.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="options">The options.</param>
    /// <param name="telemetry">The telemetry</param>
    public DistributedDeviceFlowThrottlingService(
        IDistributedCache cache,
        IClientStore clientStore,
        TimeProvider clock,
        IdentityServerOptions options, 
        ITelemetryService telemetry)
    {
        _cache = cache;
        _clientStore = clientStore;
        _clock = clock;
        _options = options;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Decides if the requesting client and device code needs to slow down.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <param name="details">The device code details.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the client is polling faster than the configured interval and should slow down; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">deviceCode</exception>
    public async Task<bool> ShouldSlowDown(string deviceCode, DeviceCode details)
    {
        if (deviceCode == null) throw new ArgumentNullException(nameof(deviceCode));
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Services, this);
        
        var key = KeyPrefix + deviceCode;
        var options = new DistributedCacheEntryOptions {AbsoluteExpiration = _clock.GetUtcNow().AddSeconds(details.Lifetime)};

        var lastSeenAsString = await _cache.GetStringAsync(key);

        // record new
        if (lastSeenAsString == null)
        {
            await _cache.SetStringAsync(key, _clock.GetUtcNow().ToString("O"), options);
            return false;
        }

        // check interval
        if (DateTime.TryParse(lastSeenAsString, out var lastSeen))
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(details.ClientId);
            if (_clock.GetUtcNow() < lastSeen.AddSeconds(client?.PollingInterval ?? _options.DeviceFlow.Interval))
            {
                await _cache.SetStringAsync(key, _clock.GetUtcNow().ToString("O"), options);
                return true;
            }
        }

        // store current and continue
        await _cache.SetStringAsync(key, _clock.GetUtcNow().ToString("O"), options);
        return false;
    }
}