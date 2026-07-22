// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Services;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class ReturnUrlParserTests
{
    private List<IReturnUrlParser> _parsers;
    private Mock<ITelemetryService> _telemetry;
    private Mock<ITrace> _trace;

    public ReturnUrlParserTests()
    {
        _parsers = new List<IReturnUrlParser>();
        _telemetry = new Mock<ITelemetryService>();
        _trace = new Mock<ITrace>();
    }

    private ReturnUrlParser CreateSubject()
    {
        return new ReturnUrlParser(_parsers, _telemetry.Object);
    }

    [Fact]
    public async Task ParseAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        await subject.ParseAsync("http://localhost/");

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "ParseAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public void IsValidReturnUrl_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSubject();
        
        subject.IsValidReturnUrl("http://localhost/");

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "IsValidReturnUrl"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}