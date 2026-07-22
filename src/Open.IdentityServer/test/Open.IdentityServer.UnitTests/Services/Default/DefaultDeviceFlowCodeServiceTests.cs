// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Services;
using Open.IdentityServer.Services.Default;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultDeviceFlowCodeServiceTests
{
    private readonly Mock<IDeviceFlowStore> _store = new();
    private readonly Mock<IHandleGenerationService> _handle = new();
    private readonly Mock<ITelemetryService> _telemetry = new();
    private readonly Mock<ITrace> _trace = new();

    public DefaultDeviceFlowCodeServiceTests()
    {
        _handle.Setup(h => h.GenerateAsync()).ReturnsAsync("handle");
    }

    private DefaultDeviceFlowCodeService CreateSubject()
    {
        return new DefaultDeviceFlowCodeService(
            _store.Object,
            _handle.Object,
            _telemetry.Object);
    }

    [Fact]
    public async Task StoreDeviceAuthorizationAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.StoreDeviceAuthorizationAsync("userCode", new Models.DeviceCode());
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "StoreDeviceAuthorizationAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task FindByUserCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.FindByUserCodeAsync("userCode");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "FindByUserCodeAsync"));   
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task FindByDeviceCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.FindByDeviceCodeAsync("deviceCode");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "FindByDeviceCodeAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task UpdateByUserCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.UpdateByUserCodeAsync("userCode", new Models.DeviceCode());
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "UpdateByUserCodeAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RemoveByDeviceCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        var subject = CreateSubject();
        
        await subject.RemoveByDeviceCodeAsync("deviceCode");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "RemoveByDeviceCodeAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}