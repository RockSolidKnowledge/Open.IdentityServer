// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Events;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints;

public class IntrospectionEndpointTests
{
    private const string Category = "Endpoints - Introspection";

    private readonly Mock<IApiSecretValidator> _apiSecretValidator = new();
    private readonly Mock<IIntrospectionRequestValidator> _requestValidator = new();
    private readonly Mock<IIntrospectionResponseGenerator> _responseGenerator = new();
    private readonly TestEventService _events = new();
    private readonly Mock<ITelemetryService> _telemetryService = new();

    private IntrospectionEndpoint CreateSubject()
    {
        return new IntrospectionEndpoint(
            _apiSecretValidator.Object,
            _requestValidator.Object,
            _responseGenerator.Object,
            _telemetryService.Object,
            _events,
            TestLogger.Create<IntrospectionEndpoint>());
    }

    private static HttpContext CreatePostFormContext(string formBody = "token=abc")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(formBody));
        return context;
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_raise_failure_event_when_request_validation_fails()
    {
        var subject = CreateSubject();
        var context = CreatePostFormContext("token=abc");
        var api = new ApiResource("api1");

        _apiSecretValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ApiSecretValidationResult
            {
                IsError = false,
                Resource = api
            });

        _requestValidator.Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), api))
            .ReturnsAsync(new IntrospectionRequestValidationResult
            {
                IsError = true,
                Error = "invalid_request",
                Api = api
            });

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<BadRequestResult>();
        _events.AssertEventWasRaised<TokenIntrospectionFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_send_telemetry_signal_when_request_validation_fails()
    {
        _telemetryService.Setup(t => t.CountTokenIntrospection("api1", null, "invalid_request"))
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostFormContext("token=abc");
        var api = new ApiResource("api1");

        _apiSecretValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ApiSecretValidationResult
            {
                IsError = false,
                Resource = api
            });

        _requestValidator.Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), api))
            .ReturnsAsync(new IntrospectionRequestValidationResult
            {
                IsError = true,
                Error = "invalid_request",
                Api = api
            });

        await subject.ProcessAsync(context);
        
        _telemetryService.Verify();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_initiate_telemetry_trace()
    {
        var subject = CreateSubject();
        var context = CreatePostFormContext("token=abc");
        
        _apiSecretValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ApiSecretValidationResult { });

        await subject.ProcessAsync(context);
        
        _telemetryService.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic,
            subject,
            "ProcessAsync"));
    }
}