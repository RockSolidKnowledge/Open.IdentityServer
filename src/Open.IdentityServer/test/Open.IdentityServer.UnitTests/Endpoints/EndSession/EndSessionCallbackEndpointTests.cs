// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Endpoints;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Services;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.EndSession;

public class EndSessionCallbackEndpointTests
{
    private const string Category = "End Session Callback Endpoint";

    private StubEndSessionRequestValidator _stubEndSessionRequestValidator = new StubEndSessionRequestValidator();
    private EndSessionCallbackEndpoint _subject;
    private Mock<ITelemetryService> _telemetry = new ();
    private Mock<ITrace> _trace = new ();

    public EndSessionCallbackEndpointTests()
    {
        _subject = new EndSessionCallbackEndpoint(
            _stubEndSessionRequestValidator,
            _telemetry.Object,
            new LoggerFactory().CreateLogger<EndSessionCallbackEndpoint>());
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ProcessAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var context = new DefaultHttpContext();

        await _subject.ProcessAsync(context);

        _telemetry.Verify(t => t.Trace(TelemetryConstants.TraceCategories.Basic, _subject, "ProcessAsync"), Times.Once);
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}