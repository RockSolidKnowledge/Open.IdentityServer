// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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

public class CachingResourceStoreTests
{
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(5);

    private readonly IdentityServerOptions _options;
    private readonly Mock<IResourceStore> _inner = new();
    private readonly Mock<ICache<IEnumerable<IdentityResource>>> _identityCache = new();
    private readonly Mock<ICache<IEnumerable<ApiResource>>> _apiByScopeCache = new();
    private readonly Mock<ICache<IEnumerable<ApiResource>>> _apiResourceCache = new();
    private readonly Mock<ICache<IEnumerable<ApiScope>>> _apiScopeCache = new();
    private readonly Mock<ICache<Resources>> _allCache = new();
    private readonly Mock<ITelemetryService> _telemetry = new();
    private readonly Mock<ITrace> _trace = new();

    public CachingResourceStoreTests()
    {
        _options = new IdentityServerOptions
        {
            Caching = new CachingOptions { ResourceStoreExpiration = _expiration }
        };
    }

    private CachingResourceStore<IResourceStore> CreateSubject() =>
        new(_options, _inner.Object,
            _identityCache.Object,
            _apiByScopeCache.Object,
            _apiResourceCache.Object,
            _apiScopeCache.Object,
            _allCache.Object,
            _telemetry.Object,
            TestLogger.Create<CachingResourceStore<IResourceStore>>());

    [Fact]
    public async Task GetAllResources_OnCacheMiss_ShouldCacheAndReturnResources()
    {
        var expected = new Resources();
        _allCache.Setup(x => x.GetAsync("__all__")).ReturnsAsync((Resources)null);
        _inner.Setup(x => x.GetAllResourcesAsync()).ReturnsAsync(expected);
        _allCache.Setup(x => x.SetAsync("__all__", expected, _expiration)).Returns(Task.CompletedTask);

        var result = await CreateSubject().GetAllResourcesAsync();

        result.Should().BeSameAs(expected);
        _allCache.Verify(x => x.GetAsync("__all__"), Times.Once);
        _inner.Verify(x => x.GetAllResourcesAsync(), Times.Once);
        _allCache.Verify(x => x.SetAsync("__all__", expected, _expiration), Times.Once);
    }

    [Fact]
    public async Task FindApiResourcesByName_OnCacheMiss_ShouldCacheAndReturnApiResources()
    {
        var names = new[] { "resource2", "resource1" };
        var cacheKey = "resource1,resource2";
        var expected = new List<ApiResource> { new("resource1"), new("resource2") };

        _apiResourceCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<ApiResource>)null);
        _inner.Setup(x => x.FindApiResourcesByNameAsync(names)).ReturnsAsync(expected);
        _apiResourceCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var result = await CreateSubject().FindApiResourcesByNameAsync(names);

        result.Should().BeSameAs(expected);
        _apiResourceCache.Verify(x => x.GetAsync(cacheKey), Times.Once);
        _inner.Verify(x => x.FindApiResourcesByNameAsync(names), Times.Once);
        _apiResourceCache.Verify(x => x.SetAsync(cacheKey, expected, _expiration), Times.Once);
    }

    [Fact]
    public async Task FindIdentityResourcesByName_OnCacheMiss_ShouldCacheAndReturnIdentityResources()
    {
        var names = new[] { "profile", "openid" };
        var cacheKey = "openid,profile";
        var expected = new List<IdentityResource> { new() { Name = "openid" }, new() { Name = "profile" } };

        _identityCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<IdentityResource>)null);
        _inner.Setup(x => x.FindIdentityResourcesByScopeNameAsync(names)).ReturnsAsync(expected);
        _identityCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var result = await CreateSubject().FindIdentityResourcesByScopeNameAsync(names);

        result.Should().BeSameAs(expected);
        _identityCache.Verify(x => x.GetAsync(cacheKey), Times.Once);
        _inner.Verify(x => x.FindIdentityResourcesByScopeNameAsync(names), Times.Once);
        _identityCache.Verify(x => x.SetAsync(cacheKey, expected, _expiration), Times.Once);
    }

    [Fact]
    public async Task FindApiResourcesByScopeName_OnCacheMiss_ShouldCacheAndReturnApiResources()
    {
        var names = new[] { "scope2", "scope1" };
        var cacheKey = "scope1,scope2";
        var expected = new List<ApiResource> { new("api1"), new("api2") };

        _apiByScopeCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<ApiResource>)null);
        _inner.Setup(x => x.FindApiResourcesByScopeNameAsync(names)).ReturnsAsync(expected);
        _apiByScopeCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var result = await CreateSubject().FindApiResourcesByScopeNameAsync(names);

        result.Should().BeSameAs(expected);
        _apiByScopeCache.Verify(x => x.GetAsync(cacheKey), Times.Once);
        _inner.Verify(x => x.FindApiResourcesByScopeNameAsync(names), Times.Once);
        _apiByScopeCache.Verify(x => x.SetAsync(cacheKey, expected, _expiration), Times.Once);
    }

    [Fact]
    public async Task FindApiScopesByName_OnCacheMiss_ShouldCacheAndReturnApiScopes()
    {
        var names = new[] { "scopeB", "scopeA" };
        var cacheKey = "scopeA,scopeB";
        var expected = new List<ApiScope> { new("scopeA"), new("scopeB") };

        _apiScopeCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<ApiScope>)null);
        _inner.Setup(x => x.FindApiScopesByNameAsync(names)).ReturnsAsync(expected);
        _apiScopeCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var result = await CreateSubject().FindApiScopesByNameAsync(names);

        result.Should().BeSameAs(expected);
        _apiScopeCache.Verify(x => x.GetAsync(cacheKey), Times.Once);
        _inner.Verify(x => x.FindApiScopesByNameAsync(names), Times.Once);
        _apiScopeCache.Verify(x => x.SetAsync(cacheKey, expected, _expiration), Times.Once);
    }
    
    [Fact]
    public async Task GetAllResources_WhenCalled_ShouldTelemetryTrace()
    {
        var expected = new Resources();
        _allCache.Setup(x => x.GetAsync("__all__")).ReturnsAsync((Resources)null);
        _inner.Setup(x => x.GetAllResourcesAsync()).ReturnsAsync(expected);
        _allCache.Setup(x => x.SetAsync("__all__", expected, _expiration)).Returns(Task.CompletedTask);

        var subject = CreateSubject();
        
        await subject.GetAllResourcesAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Cache, subject, "GetAllResourcesAsync"));
    }

    [Fact]
    public async Task FindApiResourcesByName_WhenCalled_ShouldTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        var names = new[] { "resource2", "resource1" };
        var cacheKey = "resource1,resource2";
        var expected = new List<ApiResource> { new("resource1"), new("resource2") };

        _apiResourceCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<ApiResource>)null);
        _inner.Setup(x => x.FindApiResourcesByNameAsync(names)).ReturnsAsync(expected);
        _apiResourceCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var subject = CreateSubject();
        
        await subject.FindApiResourcesByNameAsync(names);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Cache, subject, "FindApiResourcesByNameAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task FindIdentityResourcesByName_WhenCalled_ShouldTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var names = new[] { "profile", "openid" };
        var cacheKey = "openid,profile";
        var expected = new List<IdentityResource> { new() { Name = "openid" }, new() { Name = "profile" } };

        _identityCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<IdentityResource>)null);
        _inner.Setup(x => x.FindIdentityResourcesByScopeNameAsync(names)).ReturnsAsync(expected);
        _identityCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var subject = CreateSubject();
        
        await subject.FindIdentityResourcesByScopeNameAsync(names);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Cache, subject, "FindIdentityResourcesByScopeNameAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task FindApiResourcesByScopeName_WhenCalled_ShouldTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var names = new[] { "scope2", "scope1" };
        var cacheKey = "scope1,scope2";
        var expected = new List<ApiResource> { new("api1"), new("api2") };

        _apiByScopeCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<ApiResource>)null);
        _inner.Setup(x => x.FindApiResourcesByScopeNameAsync(names)).ReturnsAsync(expected);
        _apiByScopeCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var subject = CreateSubject();
        
        await subject.FindApiResourcesByScopeNameAsync(names);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Cache, subject, "FindApiResourcesByScopeNameAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task FindApiScopesByName_WhenCalled_ShouldTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var names = new[] { "scopeB", "scopeA" };
        var cacheKey = "scopeA,scopeB";
        var expected = new List<ApiScope> { new("scopeA"), new("scopeB") };

        _apiScopeCache.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync((IEnumerable<ApiScope>)null);
        _inner.Setup(x => x.FindApiScopesByNameAsync(names)).ReturnsAsync(expected);
        _apiScopeCache.Setup(x => x.SetAsync(cacheKey, expected, _expiration)).Returns(Task.CompletedTask);

        var subject = CreateSubject();
        
        await subject.FindApiScopesByNameAsync(names);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Cache, subject, "FindApiScopesByNameAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}