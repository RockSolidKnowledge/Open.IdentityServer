// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services;

public class DefaultJwtRequestUriHttpClientTests
{
    private readonly HttpClient _client;
    private readonly IdentityServerOptions _options;
    private readonly Mock<ILoggerFactory> _loggerFactory;
    private readonly Mock<HttpMessageHandler> _handler;
    private readonly Mock<ITelemetryService> _telemetry;

    public DefaultJwtRequestUriHttpClientTests()
    {
        _handler = new Mock<HttpMessageHandler>();
        _client = new HttpClient(_handler.Object);
        
        _options = new IdentityServerOptions();
        _loggerFactory = new Mock<ILoggerFactory>();
        _loggerFactory.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(TestLogger.Create<DefaultJwtRequestUriHttpClient>());
        
        _telemetry = new Mock<ITelemetryService>();
    }

    private DefaultJwtRequestUriHttpClient CreateSubject()
    {
        return new DefaultJwtRequestUriHttpClient(
            _client,
            _options,
            _telemetry.Object,
            _loggerFactory.Object
        );
    }

    [Fact]
    public async Task GetJwtAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/jwt")
            });
        
        var subject = CreateSubject();
        
        await subject.GetJwtAsync("https://example.com/jwt", new Models.Client { ClientId = "test-client" });

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            subject, 
            "GetJwtAsync"));
    }
}