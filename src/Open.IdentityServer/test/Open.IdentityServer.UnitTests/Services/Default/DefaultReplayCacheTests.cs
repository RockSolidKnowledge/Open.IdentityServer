// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Open.IdentityServer.Services;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultReplayCacheTests
{
    private Mock<IDistributedCache> _cache = new();
    private Mock<ITelemetryService> _telemetry = new();
    private Mock<ITrace> _trace = new();
    
    public DefaultReplayCacheTests()
    {
    }

    private DefaultReplayCache CreateSubject()
    {
        return new DefaultReplayCache(_cache.Object, _telemetry.Object);
    }

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.AddAsync("test-purpose", "test-handle", DateTimeOffset.UtcNow.AddMinutes(5));
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "AddAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new byte[] { 1 });
        
        var subject = CreateSubject();
        
        await subject.ExistsAsync("test-purpose", "test-handle");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "ExistsAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}