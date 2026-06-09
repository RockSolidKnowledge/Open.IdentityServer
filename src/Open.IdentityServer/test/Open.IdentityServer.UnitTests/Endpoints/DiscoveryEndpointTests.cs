// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints;

public class DiscoveryEndpointTests
{
    private IdentityServerOptions _options;
    private Mock<IDiscoveryResponseGenerator> _responseGenerator;
    private Mock<ITelemetryService> _telemetry;
    private Mock<ITrace> _trace;
    
    public DiscoveryEndpointTests()
    {
        _options = new();
        _responseGenerator = new();
        _telemetry = new();
        _trace = new();
    }

    private DiscoveryEndpoint CreateSubject()
    {
        return new DiscoveryEndpoint(
            _options,
            _responseGenerator.Object,
            _telemetry.Object,
            TestLogger.Create<DiscoveryEndpoint>());
    }

    [Fact]
    public async Task Process_WhenCalled_ShouldTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
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