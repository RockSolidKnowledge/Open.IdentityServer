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

public class TokenRevocationEndpointTests
{
    private const string Category = "Endpoints - Token Revocation";

    private readonly Mock<IClientSecretValidator> _clientValidator = new();
    private readonly Mock<ITokenRevocationRequestValidator> _requestValidator = new();
    private readonly Mock<ITokenRevocationResponseGenerator> _responseGenerator = new();
    private readonly Mock<ITelemetryService> _telemetry = new();
    private readonly TestEventService _events = new();

    private TokenRevocationEndpoint CreateSubject() =>
        new TokenRevocationEndpoint(
            TestLogger.Create<TokenRevocationEndpoint>(),
            _clientValidator.Object,
            _requestValidator.Object,
            _responseGenerator.Object,
            _events,
            _telemetry.Object);

    private static HttpContext CreatePostFormContext(string formBody = "token=abc") 
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(formBody));
        return context;
    }

    private static Client TestClient() =>
        new Client { ClientId = "client", ClientName = "Test Client" };

    private static ClientSecretValidationResult ValidClientResult() =>
        new ClientSecretValidationResult { Client = TestClient(), IsError = false};

    private static TokenRevocationRequestValidationResult ValidRequestResult() =>
        new TokenRevocationRequestValidationResult
        {
            Client = TestClient(),
            Token = "abc",
            TokenTypeHint = OidcConstants.TokenTypes.AccessToken,
            IsError = false
        };

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_raise_token_revoked_success_event_when_token_is_found_and_revoked()
    {
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<Client>()))
            .ReturnsAsync(ValidRequestResult());

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<TokenRevocationRequestValidationResult>()))
            .ReturnsAsync(new TokenRevocationResponse { Success = true });

        var result = await subject.ProcessAsync(context);

        var evt = _events.AssertEventWasRaised<TokenRevokedSuccessEvent>();
        evt.ClientId.Should().Be("client");
        evt.TokenType.Should().Be(OidcConstants.TokenTypes.AccessToken);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_emit_telemetry_signal_when_token_is_found_and_revoked()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "client")
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountTokenRevocation(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<Client>()))
            .ReturnsAsync(ValidRequestResult());

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<TokenRevocationRequestValidationResult>()))
            .ReturnsAsync(new TokenRevocationResponse { Success = true });

        var result = await subject.ProcessAsync(context);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_emit_telemetry_signal_when_client_validation_fails()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "client"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, OidcConstants.TokenErrors.InvalidClient)
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountTokenRevocation(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        var clientResult = ValidClientResult();
        clientResult.IsError = true;
        clientResult.Error = OidcConstants.TokenErrors.InvalidClient;
        
        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(clientResult);

        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<Client>()))
            .ReturnsAsync(ValidRequestResult());

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<TokenRevocationRequestValidationResult>()))
            .ReturnsAsync(new TokenRevocationResponse { Success = true });

        var result = await subject.ProcessAsync(context);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_emit_telemetry_signal_when_request_validation_fails()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "client"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, OidcConstants.TokenErrors.InvalidRequest)
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountTokenRevocation(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var context = CreatePostFormContext();

        _clientValidator.Setup(x => x.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(ValidClientResult());

        var requestValidationResult = ValidRequestResult();
        requestValidationResult.IsError = true;
        requestValidationResult.Error = OidcConstants.TokenErrors.InvalidRequest;
        
        _requestValidator.Setup(x => x.ValidateRequestAsync(It.IsAny<System.Collections.Specialized.NameValueCollection>(), It.IsAny<Client>()))
            .ReturnsAsync(requestValidationResult);

        _responseGenerator.Setup(x => x.ProcessAsync(It.IsAny<TokenRevocationRequestValidationResult>()))
            .ReturnsAsync(new TokenRevocationResponse { Success = true });

        var result = await subject.ProcessAsync(context);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }
}