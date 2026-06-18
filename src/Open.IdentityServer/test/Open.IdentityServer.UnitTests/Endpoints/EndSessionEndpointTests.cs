// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints;

public class EndSessionEndpointTests
{
    private Mock<IEndSessionRequestValidator> _endSessionRequestValidator = new();
    private Mock<IUserSession> _userSession = new();
    private Mock<ITelemetryService> _telemetry = new();
    
    public EndSessionEndpointTests()
    {
    }

    private EndSessionEndpoint CreateSubject()
    {
        return new EndSessionEndpoint(
            _endSessionRequestValidator.Object,
            _userSession.Object,
            _telemetry.Object,
            TestLogger.Create<EndSessionEndpoint>());
    }

    [Fact]
    public async Task process_should_initiate_telemetry_trace()
    {
        var subject = CreateSubject();
        var context = new DefaultHttpContext();
        
        await subject.ProcessAsync(context);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic, 
            subject,
            "ProcessAsync"), Times.Once);
    }
}