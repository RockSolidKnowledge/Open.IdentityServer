// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultDeviceFlowInteractionServiceTests
{
    private readonly Mock<IClientStore> _clientStore = new();
    private readonly Mock<IUserSession> _userSession = new();
    private readonly Mock<IDeviceFlowCodeService> _deviceFlowCodeService = new();
    private readonly Mock<IResourceStore> _resourceStore = new();
    private readonly Mock<IScopeParser> _scopeParser = new();
    private readonly Mock<ITelemetryService> _telemetry = new();
    private readonly Mock<ITrace> _trace = new();

    public DefaultDeviceFlowInteractionServiceTests()
    {
    }

    private DefaultDeviceFlowInteractionService CreateSubject()
    {
        return new DefaultDeviceFlowInteractionService(
            _clientStore.Object,
            _userSession.Object,
            _deviceFlowCodeService.Object,
            _resourceStore.Object,
            _scopeParser.Object,
            _telemetry.Object,
            TestLogger.Create<DefaultDeviceFlowInteractionService>()
        );
    }

    [Fact]
    public async Task GetAuthorizationContextAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.GetAuthorizationContextAsync("test");

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "GetAuthorizationContextAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.HandleRequestAsync("test", new ConsentResponse());
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "HandleRequestAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}