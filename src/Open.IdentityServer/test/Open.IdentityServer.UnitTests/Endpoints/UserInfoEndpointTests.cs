// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints;

public class UserInfoEndpointTests
{
    private BearerTokenUsageValidator _bearerTokenUsageValidator;
    private Mock<IUserInfoRequestValidator> _userInfoRequestValidator;
    private Mock<IUserInfoResponseGenerator> _userInfoResponseGenerator;
    private Mock<ITelemetryService> _telemetry;
    private Mock<ITrace> _trace;
    
    public UserInfoEndpointTests()
    {
        _bearerTokenUsageValidator = new BearerTokenUsageValidator(
            TestLogger.Create<BearerTokenUsageValidator>());
        _userInfoRequestValidator = new Mock<IUserInfoRequestValidator>();
        _userInfoResponseGenerator = new Mock<IUserInfoResponseGenerator>();
        _telemetry = new Mock<ITelemetryService>();
        _trace = new Mock<ITrace>();
    }

    private UserInfoEndpoint CreateSubject()
    {
        return new UserInfoEndpoint(
            _bearerTokenUsageValidator,
            _userInfoRequestValidator.Object,
            _userInfoResponseGenerator.Object,
            _telemetry.Object,
            TestLogger.Create<UserInfoEndpoint>());
    }

    [Fact]
    public async Task ProcessAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        var context = new DefaultHttpContext();

        await subject.ProcessAsync(context);

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic,
            subject,
            "ProcessAsync"
            ), Times.Once);
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}