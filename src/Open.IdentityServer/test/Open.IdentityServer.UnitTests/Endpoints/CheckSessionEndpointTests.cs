// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints;

public class CheckSessionEndpointTests
{
    private Mock<ITelemetryService> _telemetry;
    private Mock<ITrace> _trace;
    
    public CheckSessionEndpointTests()
    {
        _telemetry = new Mock<ITelemetryService>();
        _trace = new Mock<ITrace>();
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
    }

    private CheckSessionEndpoint CreateSubject()
    {
        return new CheckSessionEndpoint(
            _telemetry.Object,
            TestLogger.Create<CheckSessionEndpoint>());
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
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}