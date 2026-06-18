// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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

public class TokenEndpointTests
{
    private const string Category = "Endpoints - Token";

    private readonly Mock<IClientSecretValidator> _clientValidator = new();
    private readonly Mock<ITokenRequestValidator> _requestValidator = new();
    private readonly Mock<ITokenResponseGenerator> _responseGenerator = new();
    private readonly Mock<ITelemetryService> _telemetry = new();
    private readonly TestEventService _events = new();

    private TokenEndpoint CreateSubject() =>
        new TokenEndpoint(
            _clientValidator.Object,
            _requestValidator.Object,
            _responseGenerator.Object,
            _events,
            _telemetry.Object,
            TestLogger.Create<TokenEndpoint>());

    private static HttpContext CreatePostFormContext(string formBody = "grant_type=client_credentials")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(formBody));
        return context;
    }

    private static ClientSecretValidationResult ValidClientResult() =>
        new ClientSecretValidationResult
        {
            Client = new Client { ClientId = "client", ClientName = "Test Client" }
        };

    private static TokenRequestValidationResult ValidRequestResult() =>
        new TokenRequestValidationResult(
            new ValidatedTokenRequest
            {
                Client = new Client { ClientId = "client", ClientName = "Test Client" },
                GrantType = OidcConstants.GrantTypes.ClientCredentials
            });

    private static TokenRequestValidationResult FailedRequestResult(string error, string errorDescription = null) =>
        new TokenRequestValidationResult(
            new ValidatedTokenRequest
            {
                Client = new Client { ClientId = "client", ClientName = "Test Client" },
                GrantType = OidcConstants.GrantTypes.ClientCredentials
            },
            error,
            errorDescription);

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_raise_token_issued_success_event_when_request_is_valid()
    {
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(ValidRequestResult());

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<TokenRequestValidationResult>()))
            .ReturnsAsync(new TokenResponse { AccessToken = "access_token" });

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<TokenResult>();
        _events.AssertEventWasRaised<TokenIssuedSuccessEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_raise_token_issued_failure_event_when_request_validation_fails()
    {
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(FailedRequestResult(OidcConstants.TokenErrors.InvalidGrant, "invalid_username_or_password"));

        var result = await subject.ProcessAsync(context);

        result.Should().BeOfType<TokenErrorResult>();
        var evt = _events.AssertEventWasRaised<TokenIssuedFailureEvent>();
        evt.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        evt.ErrorDescription.Should().Be("invalid_username_or_password");
        evt.ClientId.Should().Be("client");
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_emit_telemetry_signal_when_request_is_valid()
    {
        _telemetry.Setup(t => t.CountTokenIssued("client",  "client_credentials"))
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(ValidRequestResult());

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<TokenRequestValidationResult>()))
            .ReturnsAsync(new TokenResponse { AccessToken = "access_token" });

        await subject.ProcessAsync(context);

        _telemetry.Verify();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_emit_telemetry_signal_when_request_validation_fails()
    {
        _telemetry.Setup(t => t.CountTokenIssued("client", "client_credentials", "invalid_grant"))
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(FailedRequestResult(OidcConstants.TokenErrors.InvalidGrant, "invalid_username_or_password"));

        await subject.ProcessAsync(context);

        _telemetry.Verify();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_initiate_telemetry_trace()
    {
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<ClientSecretValidationResult>()))
            .ReturnsAsync(ValidRequestResult());

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<TokenRequestValidationResult>()))
            .ReturnsAsync(new TokenResponse { AccessToken = "access_token" });

        await subject.ProcessAsync(context);

        _telemetry.Verify(t => t.Trace(TelemetryConstants.TraceCategories.Basic, subject, "ProcessAsync"), Times.Once);
    }
}