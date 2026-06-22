// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultProfileServiceTests
{
    private Mock<ITelemetryService> _telemetry;
    
    public DefaultProfileServiceTests()
    {
        _telemetry = new();
    }

    private DefaultProfileService CreateSubject()
    {
        return new DefaultProfileService(
            _telemetry.Object,
            TestLogger.Create<DefaultProfileService>());
    }

    [Fact]
    public async Task GetProfileDataAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        ClaimsPrincipal sub = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("sub", "123")
        }, "mock"));
        Client client = new Client();
        
        var subject = CreateSubject();
        
        await subject.GetProfileDataAsync(new ProfileDataRequestContext
        {
            Client = client,
            Subject = sub,
            RequestedClaimTypes = Array.Empty<string>()
        });
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetProfileDataAsync"), Times.Once);
    }

    [Fact]
    public async Task IsActiveAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();

        ClaimsPrincipal sub = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("sub", "123")
        }, "mock"));
        Client client = new Client();
        await subject.IsActiveAsync(new IsActiveContext(sub, client, "caller"));
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "IsActiveAsync"), Times.Once);
    }
}