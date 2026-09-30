// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DistributedDeviceFlowThrottlingServiceTests
{
    private TestCache cache = new();

    private readonly IdentityServerOptions options = new() {DeviceFlow = new DeviceFlowOptions {Interval = 5}};
    private readonly DeviceCode deviceCode = new()
    {
        Lifetime = 300,
        CreationTime = DateTime.UtcNow
    };

    private const string CacheKey = "devicecode_";
    private readonly DateTime testDate = new(2018, 06, 28, 13, 37, 42);

    private Mock<ITelemetryService> _telemetry = new();
    private Mock<ITrace> _trace = new();
    private Mock<IClientStore> _clientStore;
    private Client _client;

    public DistributedDeviceFlowThrottlingServiceTests()
    {
        _clientStore = new Mock<IClientStore>();
        _client = new Client
        {
            ClientId = "client"
        };
        _clientStore.Setup(x => x.FindClientByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_client);
    }

    [Fact]
    public async Task First_Poll()
    {
        var handle = Guid.NewGuid().ToString();
        var service = CreateSubject();

        var result = await service.ShouldSlowDown(handle, deviceCode);

        result.Should().BeFalse();

        CheckCacheEntry(handle);
    }

    private DistributedDeviceFlowThrottlingService CreateSubject()
    {
        return new DistributedDeviceFlowThrottlingService(
            cache, 
            _clientStore.Object,
            new StubClock {UtcNowFunc = () => testDate}, 
            options, 
            _telemetry.Object);
    }

    [Fact] 
    public async Task ShouldSlowDown_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var handle = Guid.NewGuid().ToString();
        var service = CreateSubject();
        
        await service.ShouldSlowDown(handle, deviceCode);

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            service,
            "ShouldSlowDown"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task Second_Poll_Too_Fast_For_Options()
    {
        var handle = Guid.NewGuid().ToString();
        var service = CreateSubject();
        _client.PollingInterval = null;

        await cache.SetAsync(
            CacheKey + handle, 
            Encoding.UTF8.GetBytes(testDate.AddSeconds(-1).ToString("O")), 
            TestContext.Current.CancellationToken);

        var result = await service.ShouldSlowDown(handle, deviceCode);

        result.Should().BeTrue();
            
        CheckCacheEntry(handle);
    }

    [Fact]
    public async Task Second_Poll_Too_Fast_For_Client()
    {
        var handle = Guid.NewGuid().ToString();
        var service = CreateSubject();
        _client.PollingInterval = 4;

        await cache.SetAsync(
            CacheKey + handle,
            Encoding.UTF8.GetBytes(testDate.AddSeconds(-2).ToString("O")),
            TestContext.Current.CancellationToken);

        var result = await service.ShouldSlowDown(handle, deviceCode);

        result.Should().BeTrue();

        CheckCacheEntry(handle);
    }

    [Fact]
    public async Task Second_Poll_After_Options_Interval()
    {
        var handle = Guid.NewGuid().ToString();
            
        var service = CreateSubject();
        _client.PollingInterval = null;

        await cache.SetAsync(
            $"devicecode_{handle}", 
            Encoding.UTF8.GetBytes(testDate.AddSeconds(-deviceCode.Lifetime - 1).ToString("O")), 
            TestContext.Current.CancellationToken);

        var result = await service.ShouldSlowDown(handle, deviceCode);

        result.Should().BeFalse();

        CheckCacheEntry(handle);
    }

    [Fact]
    public async Task Second_Poll_After_Client_Interval()
    {
        var handle = Guid.NewGuid().ToString();

        var service = CreateSubject();
        _client.PollingInterval = 4;

        await cache.SetAsync(
            $"devicecode_{handle}",
            Encoding.UTF8.GetBytes(testDate.AddSeconds(-deviceCode.Lifetime - 2).ToString("O")),
            TestContext.Current.CancellationToken);

        var result = await service.ShouldSlowDown(handle, deviceCode);

        result.Should().BeFalse();

        CheckCacheEntry(handle);
    }

    /// <summary>
    /// Addresses race condition from #3860
    /// </summary>
    [Fact]
    public async Task Expired_Device_Code_Should_Not_Have_Expiry_in_Past()
    {
        var handle = Guid.NewGuid().ToString();
        deviceCode.CreationTime = testDate.AddSeconds(-deviceCode.Lifetime * 2);

        var service = CreateSubject();

        var result = await service.ShouldSlowDown(handle, deviceCode);
            
        result.Should().BeFalse();

        cache.Items.TryGetValue(CacheKey + handle, out var values).Should().BeTrue();
        values?.Item2.AbsoluteExpiration.Should().BeOnOrAfter(testDate);
    }

    private void CheckCacheEntry(string handle)
    {
        cache.Items.TryGetValue(CacheKey + handle, out var values).Should().BeTrue();

        var dateTimeAsString = Encoding.UTF8.GetString(values?.Item1);
        var dateTime = DateTime.Parse(dateTimeAsString);
        dateTime.Should().Be(testDate);

        values?.Item2.AbsoluteExpiration.Should().BeCloseTo(testDate.AddSeconds(deviceCode.Lifetime), TimeSpan.FromSeconds(1));
    }
}

internal class TestCache : IDistributedCache
{
    public readonly Dictionary<string, Tuple<byte[], DistributedCacheEntryOptions>> Items = new();

    public byte[] Get(string key)
    {
        if (Items.TryGetValue(key, out var value)) return value.Item1;
        return null;
    }

    public Task<byte[]> GetAsync(string key, CancellationToken token = new())
    {
        return Task.FromResult(Get(key));
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        Items.Remove(key);

        Items.Add(key, new Tuple<byte[], DistributedCacheEntryOptions>(value, options));
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new())
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string key, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }
}