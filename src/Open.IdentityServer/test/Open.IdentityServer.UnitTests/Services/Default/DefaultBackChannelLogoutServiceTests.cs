// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultBackChannelLogoutServiceTests
{
    private readonly TimeProvider _clock = new StubClock();
    private readonly Mock<ITokenCreationService> _tokenCreation = new();
    private readonly IdentityServerTools _tools;
    private readonly Mock<ILogoutNotificationService> _logoutNotification = new();
    private readonly Mock<IBackChannelLogoutHttpClient> _backChannelLogoutHttpClient = new();
    private readonly Mock<ITelemetryService> _telemetry = new();

    public DefaultBackChannelLogoutServiceTests()
    {
        _tools = new MockIdentityServerTools(Mock.Of<IHttpContextAccessor>(), _tokenCreation.Object, _clock);
    }

    private DefaultBackChannelLogoutService CreateSubject()
    {
        return new DefaultBackChannelLogoutService(
            _clock,
            _tools,
            _logoutNotification.Object,
            _backChannelLogoutHttpClient.Object,
            _telemetry.Object,
            TestLogger.Create<DefaultBackChannelLogoutService>()
        );
    }

    [Fact]
    public async Task SendLogoutNotificationAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _logoutNotification.Setup(ln => ln.GetBackChannelLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()))
            .ReturnsAsync([]);
        
        var subject = CreateSubject();

        await subject.SendLogoutNotificationsAsync(new LogoutNotificationContext());
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "SendLogoutNotificationsAsync"));
    }
}

internal class MockIdentityServerTools : IdentityServerTools
{
    private readonly ITokenCreationService _tokenCreation;

    public MockIdentityServerTools(IHttpContextAccessor contextAccessor, ITokenCreationService tokenCreation, TimeProvider clock) : base(contextAccessor, tokenCreation, clock)
    {
        _tokenCreation = tokenCreation;
    }

    public override Task<string> IssueJwtAsync(int lifetime, IEnumerable<Claim> claims)
    {
        return _tokenCreation.CreateTokenAsync(new Token());
    }

    public override Task<string> IssueJwtAsync(int lifetime, string issuer, IEnumerable<Claim> claims)
    {
        return _tokenCreation.CreateTokenAsync(new Token());
    }
}