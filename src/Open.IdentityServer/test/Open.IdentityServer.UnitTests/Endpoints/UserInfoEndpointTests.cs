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
    private BearerTokenUsageValidator bearerTokenUsageValidator;
    private Mock<IUserInfoRequestValidator> userInfoRequestValidator;
    private Mock<IUserInfoResponseGenerator> userInfoResponseGenerator;
    private Mock<ITelemetryService> telemetry;
    
    public UserInfoEndpointTests()
    {
        bearerTokenUsageValidator = new BearerTokenUsageValidator(
            TestLogger.Create<BearerTokenUsageValidator>());
        userInfoRequestValidator = new Mock<IUserInfoRequestValidator>();
        userInfoResponseGenerator = new Mock<IUserInfoResponseGenerator>();
        telemetry = new Mock<ITelemetryService>();
    }

    private UserInfoEndpoint CreateSubject()
    {
        return new UserInfoEndpoint(
            bearerTokenUsageValidator,
            userInfoRequestValidator.Object,
            userInfoResponseGenerator.Object,
            telemetry.Object,
            TestLogger.Create<UserInfoEndpoint>());
    }

    [Fact]
    public async Task ProcessAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        var context = new DefaultHttpContext();

        await subject.ProcessAsync(context);

        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic,
            subject,
            "ProcessAsync"
            ), Times.Once);
    }
}