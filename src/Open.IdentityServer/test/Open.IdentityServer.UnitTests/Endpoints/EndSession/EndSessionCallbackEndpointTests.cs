// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
    private Mock<ITelemetryService> _mockTelemetryService = new Mock<ITelemetryService>();

    public EndSessionCallbackEndpointTests()
    {
        _subject = new EndSessionCallbackEndpoint(
            _stubEndSessionRequestValidator,
            _mockTelemetryService.Object,
            new LoggerFactory().CreateLogger<EndSessionCallbackEndpoint>());
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ProcessAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var context = new DefaultHttpContext();

        await _subject.ProcessAsync(context);

        _mockTelemetryService.Verify(t => t.Trace(TelemetryConstants.TraceCategories.Basic, _subject, "ProcessAsync"), Times.Once);
    }
}