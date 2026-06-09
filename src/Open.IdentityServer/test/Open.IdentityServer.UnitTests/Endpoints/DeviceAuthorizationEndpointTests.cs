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
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints;

public class DeviceAuthorizationEndpointTests
{
    private const string Category = "Endpoints";

    private readonly Mock<IClientSecretValidator> _clientValidator = new();
    private readonly Mock<IDeviceAuthorizationRequestValidator> _requestValidator = new();
    private readonly Mock<IDeviceAuthorizationResponseGenerator> _responseGenerator = new();
    private readonly TestEventService _events = new();
    private readonly Mock<ITelemetryService> _telemetry = new();

    private DeviceAuthorizationEndpoint CreateSubject()
    {
        return new DeviceAuthorizationEndpoint(
            _clientValidator.Object,
            _requestValidator.Object,
            _responseGenerator.Object,
            _events,
            _telemetry.Object,
            TestLogger.Create<DeviceAuthorizationEndpoint>());
    }

    private HttpContext CreatePostContext(string body = "")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/x-www-form-urlencoded";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.Body = stream;

        return context;
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_error_for_get_request()
    {
        var subject = CreateSubject();
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<TokenErrorResult>();
        var errorResult = result as TokenErrorResult;
        errorResult.Response.Error.Should().Be(OidcConstants.TokenErrors.InvalidRequest);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_error_for_missing_content_type()
    {
        var subject = CreateSubject();
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/json";

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<TokenErrorResult>();
        var errorResult = result as TokenErrorResult;
        errorResult.Response.Error.Should().Be(OidcConstants.TokenErrors.InvalidRequest);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_error_when_client_validation_fails()
    {
        var subject = CreateSubject();
        var context = CreatePostContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ClientSecretValidationResult { IsError = true });

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<TokenErrorResult>();
        var errorResult = result as TokenErrorResult;
        errorResult.Response.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_error_when_request_validation_fails()
    {
        var subject = CreateSubject();
        var context = CreatePostContext();
        var client = new Open.IdentityServer.Models.Client { ClientId = "test-client" };

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ClientSecretValidationResult
            {
                IsError = false,
                Client = client
            });

        _requestValidator.Setup(x =>
                x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync((NameValueCollection nvc, ClientSecretValidationResult csrv) =>
                new DeviceAuthorizationRequestValidationResult
                (
                    new ValidatedDeviceAuthorizationRequest(){ Raw = nvc },
                    error: OidcConstants.TokenErrors.InvalidScope,
                    errorDescription: "Invalid scope requested"
                ));

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<TokenErrorResult>();
        var errorResult = result as TokenErrorResult;
        errorResult.Response.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
        errorResult.Response.ErrorDescription.Should().Be("Invalid scope requested");
        _events.AssertEventWasRaised<DeviceAuthorizationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_raise_telemetry_signal_when_request_validation_fails()
    {
        TelemetryTag[] actualTags = null;
        TelemetryTag[] expectedTags =
        [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "test-client"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, OidcConstants.TokenErrors.InvalidScope)
        ];
        _telemetry.Setup(x => x.CountDeviceAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostContext();
        var client = new Open.IdentityServer.Models.Client { ClientId = "test-client" };

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ClientSecretValidationResult
            {
                IsError = false,
                Client = client
            });

        _requestValidator.Setup(x =>
                x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync((NameValueCollection nvc, ClientSecretValidationResult csrv) =>
                new DeviceAuthorizationRequestValidationResult
                (
                    new ValidatedDeviceAuthorizationRequest(){ Raw = nvc },
                    error: OidcConstants.TokenErrors.InvalidScope,
                    errorDescription: "Invalid scope requested"
                ));

        await subject.ProcessAsync(context);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_device_authorization_result_on_success()
    {
        var subject = CreateSubject();
        var context = CreatePostContext();
        var client = new Open.IdentityServer.Models.Client { ClientId = "test-client" };
        
        var validatedRequest =
            new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest { Client = client });
            
        var response = new DeviceAuthorizationResponse
        {
            DeviceCode = "test-device-code",
            UserCode = "TEST-USER-CODE",
            VerificationUri = "https://example.com/device",
            DeviceCodeLifetime = 1800
        };

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ClientSecretValidationResult
            {
                IsError = false,
                Client = client
            });

        _requestValidator.Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(validatedRequest);

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<DeviceAuthorizationRequestValidationResult>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<DeviceAuthorizationResult>();
        var deviceResult = result as DeviceAuthorizationResult;
        deviceResult.Response.DeviceCode.Should().Be("test-device-code");
        deviceResult.Response.UserCode.Should().Be("TEST-USER-CODE");
        _events.AssertEventWasRaised<DeviceAuthorizationSuccessEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_raise_telemetry_result_on_success()
    {
        TelemetryTag[] actualTags = null;
        TelemetryTag[] expectedTags = [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "test-client")
        ];
        _telemetry.Setup(x => x.CountDeviceAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostContext();
        var client = new Open.IdentityServer.Models.Client { ClientId = "test-client" };
        
        var validatedRequest =
            new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest { Client = client });
            
        var response = new DeviceAuthorizationResponse
        {
            DeviceCode = "test-device-code",
            UserCode = "TEST-USER-CODE",
            VerificationUri = "https://example.com/device",
            DeviceCodeLifetime = 1800
        };

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ClientSecretValidationResult
            {
                IsError = false,
                Client = client
            });

        _requestValidator.Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(validatedRequest);

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<DeviceAuthorizationRequestValidationResult>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await subject.ProcessAsync(context);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_pass_base_url_to_response_generator()
    {
        var subject = CreateSubject();
        var context = CreatePostContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.PathBase = "/auth";

        var client = new Open.IdentityServer.Models.Client { ClientId = "test-client" };
        var validatedRequest = new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest { Client = client });
            
        var response = new DeviceAuthorizationResponse { DeviceCode = "code" };

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ClientSecretValidationResult { IsError = false, Client = client });

        _requestValidator.Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(validatedRequest);

        string capturedBaseUrl = null;
        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<DeviceAuthorizationRequestValidationResult>(), It.IsAny<string>()))
            .Callback<DeviceAuthorizationRequestValidationResult, string>((_, baseUrl) => capturedBaseUrl = baseUrl)
            .ReturnsAsync(response);

        await subject.ProcessAsync(context);

        capturedBaseUrl.Should().Be("https://example.com/");
    }
}