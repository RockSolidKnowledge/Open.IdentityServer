// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.Caching;

public class CachingClientStoreTests
{
    private IdentityServerOptions _options;
    private Mock<IClientStore> _inner;
    private Mock<ICache<Client>> _cache;
    private Mock<ITelemetryService> _telemetry;
    private Mock<ITrace> _trace;
    
    private TimeSpan expiration = TimeSpan.FromMinutes(5);

    public CachingClientStoreTests()
    {
        _options = new IdentityServerOptions
        {
            Caching = new CachingOptions
            {
                ClientStoreExpiration = expiration
            }
        };
        
        _inner = new Mock<IClientStore>();
        _cache = new Mock<ICache<Client>>();
        _telemetry = new Mock<ITelemetryService>();
        _trace = new Mock<ITrace>();
    }

    private CachingClientStore<IClientStore> CreateSubject()
    {
        return new CachingClientStore<IClientStore>(
            _options,
            _inner.Object,
            _cache.Object,
            _telemetry.Object,
            TestLogger.Create<CachingClientStore<IClientStore>>());
    }
    
    [Fact]
    public async Task FindClientById_OnCacheMiss_ShouldStoreAndReturnClient()
    {
        var clientId = "client";
        var expectedClient = new Client { ClientId = clientId };
        
        _cache.Setup(x => x.GetAsync(clientId))
            .ReturnsAsync((Client)null);

        _inner.Setup(x => x.FindClientByIdAsync(clientId))
            .ReturnsAsync(expectedClient);

        _cache.Setup(x => x.SetAsync(clientId, expectedClient, expiration))
            .Returns(Task.CompletedTask);

        var subject = CreateSubject();

        var result = await subject.FindClientByIdAsync(clientId);

        result.Should().BeSameAs(expectedClient);
        _cache.Verify(x => x.GetAsync(clientId), Times.Once);
        _inner.Verify(x => x.FindClientByIdAsync(clientId), Times.Once);
        _cache.Verify(x => x.SetAsync(clientId, expectedClient, expiration), Times.Once);
    }

    [Fact]
    public async Task FindClientById_WhenCalled_ShouldTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var clientId = "client";
        var expectedClient = new Client { ClientId = clientId };
        
        _cache.Setup(x => x.GetAsync(clientId))
            .ReturnsAsync((Client)null);

        _inner.Setup(x => x.FindClientByIdAsync(clientId))
            .ReturnsAsync(expectedClient);

        _cache.Setup(x => x.SetAsync(clientId, expectedClient, expiration))
            .Returns(Task.CompletedTask);

        var subject = CreateSubject();

        await subject.FindClientByIdAsync(clientId);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Cache, subject, "FindClientByIdAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}