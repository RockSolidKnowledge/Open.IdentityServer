// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class LogoutNotificationServiceTests
{
    private Mock<IClientStore> _clientStore;
    private Mock<IHttpContextAccessor> _httpContextAccessor;
    private Mock<ITelemetryService> _telemetry;
    
    public LogoutNotificationServiceTests()
    {
        _clientStore = new Mock<IClientStore>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _telemetry = new Mock<ITelemetryService>();
    }

    private LogoutNotificationService CreateSubject()
    {
        return new LogoutNotificationService(
            _clientStore.Object,
            _httpContextAccessor.Object,
            _telemetry.Object,
            TestLogger.Create<LogoutNotificationService>()
        );
    }

    [Fact]
    public async Task GetFrontChannelLogoutNotificationsUrlsAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.GetFrontChannelLogoutNotificationsUrlsAsync(new LogoutNotificationContext
        {
            ClientIds = new[] { "client1", "client2" },
            SessionId = "session1"
        });

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetFrontChannelLogoutNotificationsUrlsAsync"));
    }

    [Fact]
    public async Task GetBackChannelLogoutNotificationsAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.GetBackChannelLogoutNotificationsAsync(new LogoutNotificationContext
        {
            ClientIds = new[] { "client1", "client2" },
            SessionId = "session1"
        });

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetBackChannelLogoutNotificationsAsync"));
    }
}