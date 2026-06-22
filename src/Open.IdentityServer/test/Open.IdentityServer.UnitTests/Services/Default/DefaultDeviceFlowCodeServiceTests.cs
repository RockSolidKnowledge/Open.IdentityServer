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
    private readonly Mock<IDeviceFlowStore> store = new();
    private readonly Mock<IHandleGenerationService> handle = new();
    private readonly Mock<ITelemetryService> telemetry = new();

    public DefaultDeviceFlowCodeServiceTests()
    {
        handle.Setup(h => h.GenerateAsync()).ReturnsAsync("handle");
    }

    private DefaultDeviceFlowCodeService CreateSubject()
    {
        return new DefaultDeviceFlowCodeService(
            store.Object,
            handle.Object,
            telemetry.Object);
    }

    [Fact]
    public async Task StoreDeviceAuthorizationAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.StoreDeviceAuthorizationAsync("userCode", new Models.DeviceCode());
        
        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "StoreDeviceAuthorizationAsync"));
    }

    [Fact]
    public async Task FindByUserCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.FindByUserCodeAsync("userCode");
        
        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "FindByUserCodeAsync"));   
    }

    [Fact]
    public async Task FindByDeviceCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.FindByDeviceCodeAsync("deviceCode");
        
        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "FindByDeviceCodeAsync"));
    }

    [Fact]
    public async Task UpdateByUserCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.UpdateByUserCodeAsync("userCode", new Models.DeviceCode());
        
        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "UpdateByUserCodeAsync"));
    }

    [Fact]
    public async Task RemoveByDeviceCodeAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.RemoveByDeviceCodeAsync("deviceCode");
        
        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "RemoveByDeviceCodeAsync"));
    }
}