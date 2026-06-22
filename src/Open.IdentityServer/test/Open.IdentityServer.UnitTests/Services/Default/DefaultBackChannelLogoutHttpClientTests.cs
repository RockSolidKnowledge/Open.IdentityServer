// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultBackChannelLogoutHttpClientTests
{
    private readonly HttpClient _client;
    private readonly Mock<HttpMessageHandler> _handler;
    private readonly Mock<ITelemetryService> _telemetry;
    private readonly Mock<ILoggerFactory> _loggerFactory;

    public DefaultBackChannelLogoutHttpClientTests()
    {
        _handler = new Mock<HttpMessageHandler>();
        _client = new HttpClient(_handler.Object);
        
        _loggerFactory = new Mock<ILoggerFactory>();
        _loggerFactory.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(TestLogger.Create<DefaultBackChannelLogoutHttpClient>());
        
        _telemetry = new Mock<ITelemetryService>();
    }

    private DefaultBackChannelLogoutHttpClient CreateSubject()
    {
        return new DefaultBackChannelLogoutHttpClient(
            _client,
            _telemetry.Object,
            _loggerFactory.Object);
    }
    
    [Fact]
    public async Task PostAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.PostAsync("https://example.com/logout", new Dictionary<string, string> { { "logout_token", "test-token" } });

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, subject, "PostAsync"));
    }
}