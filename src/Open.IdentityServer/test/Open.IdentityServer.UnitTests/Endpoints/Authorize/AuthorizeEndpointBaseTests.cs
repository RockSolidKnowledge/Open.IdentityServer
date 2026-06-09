// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Events;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.Authorize;

public class AuthorizeEndpointBaseTests
{
    private const string Category = "Authorize Endpoint";

    private HttpContext _context;

    private TestEventService _fakeEventService = new TestEventService();

    private ILogger<TestAuthorizeEndpoint> _fakeLogger = TestLogger.Create<TestAuthorizeEndpoint>();

    private IdentityServerOptions _options = new IdentityServerOptions();

    private MockUserSession _mockUserSession = new MockUserSession();

    private NameValueCollection _params = new NameValueCollection();

    private StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new StubAuthorizeRequestValidator();

    private StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator = new StubAuthorizeResponseGenerator();

    private StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator = new StubAuthorizeInteractionResponseGenerator();

    private TestAuthorizeEndpoint _subject;

    private ClaimsPrincipal _user = new IdentityServerUser("bob").CreatePrincipal();

    private ValidatedAuthorizeRequest _validatedAuthorizeRequest;

    private Mock<ITelemetryService> _telemetry;

    public AuthorizeEndpointBaseTests()
    {
        Init();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task error_resurect_with_prompt_none_should_include_session_state_in_response()
    {
        _params.Add("prompt", "none");
        _stubAuthorizeRequestValidator.Result.ValidatedRequest.IsOpenIdRequest = true;
        _stubAuthorizeRequestValidator.Result.ValidatedRequest.ClientId = "client";
        _stubAuthorizeRequestValidator.Result.ValidatedRequest.SessionId = "some_session";
        _stubAuthorizeRequestValidator.Result.ValidatedRequest.RedirectUri = "http://redirect";
        _stubAuthorizeRequestValidator.Result.IsError = true;
        _stubAuthorizeRequestValidator.Result.Error = "login_required";

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        ((AuthorizeResult)result).Response.SessionState.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task authorize_request_validation_produces_error_should_display_error_page()
    {
        _stubAuthorizeRequestValidator.Result.IsError = true;
        _stubAuthorizeRequestValidator.Result.Error = "some_error";

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task interaction_generator_consent_produces_consent_should_show_consent_page()
    {
        _stubInteractionGenerator.Response.IsConsent = true;

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<ConsentPageResult>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task interaction_produces_error_should_show_error_page()
    {
        _stubInteractionGenerator.Response.Error = "error";

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task interaction_produces_error_should_show_error_page_with_error_description_if_present()
    {
        var errorDescription = "some error description";

        _stubInteractionGenerator.Response.Error = "error";
        _stubInteractionGenerator.Response.ErrorDescription = errorDescription;

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        var authorizeResult = ((AuthorizeResult)result);
        authorizeResult.Response.IsError.Should().BeTrue();
        authorizeResult.Response.ErrorDescription.Should().Be(errorDescription);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task interaction_produces_login_result_should_trigger_login()
    {
        _stubInteractionGenerator.Response.IsLogin = true;

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<LoginPageResult>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ProcessAuthorizeRequestAsync_custom_interaction_redirect_result_should_issue_redirect()
    {
        _mockUserSession.User = _user;
        _stubInteractionGenerator.Response.RedirectUrl = "http://foo.com";

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<CustomRedirectResult>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task successful_authorization_request_should_generate_authorize_result()
    {
        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task successful_authorization_request_should_raise_token_issued_success_event()
    {
        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        var evt = _fakeEventService.AssertEventWasRaised<TokenIssuedSuccessEvent>();
        evt.ClientId.Should().Be(_validatedAuthorizeRequest.ClientId);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task authorize_request_validation_produces_error_should_raise_token_issued_failure_event()
    {
        _stubAuthorizeRequestValidator.Result.IsError = true;
        _stubAuthorizeRequestValidator.Result.Error = "some_error";

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        var evt = _fakeEventService.AssertEventWasRaised<TokenIssuedFailureEvent>();
        evt.Error.Should().Be("some_error");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task interaction_produces_error_should_raise_token_issued_failure_event()
    {
        _stubInteractionGenerator.Response.Error = "interaction_error";
        _stubInteractionGenerator.Response.ErrorDescription = "something went wrong";

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        var evt = _fakeEventService.AssertEventWasRaised<TokenIssuedFailureEvent>();
        evt.Error.Should().Be("interaction_error");
        evt.ErrorDescription.Should().Be("something went wrong");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task authorize_response_is_error_should_raise_token_issued_failure_event()
    {
        _stubAuthorizeResponseGenerator.Response.Error = "access_denied";
        _stubAuthorizeResponseGenerator.Response.ErrorDescription = "user denied access";

        var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        result.Should().BeOfType<AuthorizeResult>();
        var evt = _fakeEventService.AssertEventWasRaised<TokenIssuedFailureEvent>();
        evt.Error.Should().Be("access_denied");
        evt.ErrorDescription.Should().Be("user denied access");
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task successful_authorization_request_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags =
        {
           new(TelemetryConstants.TagConstants.Client, "client"),
           new(TelemetryConstants.TagConstants.GrantType, "grant_type")
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountTokenIssued(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => actualTags = tags)
            .Verifiable(Times.Once);
        
        await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task authorize_request_validation_produces_error_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags =
        {
            new(TelemetryConstants.TagConstants.Client, "client"),
            new(TelemetryConstants.TagConstants.GrantType, "grant_type"),
            new(TelemetryConstants.TagConstants.Error, "some_error")
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountTokenIssued(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => actualTags = tags)
            .Verifiable(Times.Once);

        _stubAuthorizeRequestValidator.Result.IsError = true;
        _stubAuthorizeRequestValidator.Result.Error = "some_error";

        await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task interaction_produces_error_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags =
        {
            new(TelemetryConstants.TagConstants.Client, "client"),
            new(TelemetryConstants.TagConstants.GrantType, "grant_type"),
            new(TelemetryConstants.TagConstants.Error, "interaction_error")
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountTokenIssued(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => actualTags = tags)
            .Verifiable(Times.Once);

        _stubInteractionGenerator.Response.Error = "interaction_error";
        _stubInteractionGenerator.Response.ErrorDescription = "something went wrong";

        await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task authorize_response_is_error_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags =
        {
            new(TelemetryConstants.TagConstants.Client, "client"),
            new(TelemetryConstants.TagConstants.GrantType, "grant_type"),
            new(TelemetryConstants.TagConstants.Error, "access_denied")
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountTokenIssued(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => actualTags = tags)
            .Verifiable(Times.Once);

        _stubAuthorizeResponseGenerator.Response.Error = "access_denied";
        _stubAuthorizeResponseGenerator.Response.ErrorDescription = "user denied access";

        await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    internal void Init()
    {
        _context = new MockHttpContextAccessor().HttpContext;

        _validatedAuthorizeRequest = new ValidatedAuthorizeRequest()
        {
            RedirectUri = "http://client/callback",
            State = "123",
            ResponseMode = "fragment",
            ClientId = "client",
            Client = new Client
            {
                ClientId = "client",
                ClientName = "Test Client"
            },
            Raw = _params,
            Subject = _user,
            GrantType = "grant_type",
        };
        _stubAuthorizeResponseGenerator.Response.Request = _validatedAuthorizeRequest;

        _stubAuthorizeRequestValidator.Result = new AuthorizeRequestValidationResult(_validatedAuthorizeRequest);

        _telemetry = new();

        _subject = new TestAuthorizeEndpoint(
            _fakeEventService,
            _fakeLogger,
            _options,
            _stubAuthorizeRequestValidator,
            _stubInteractionGenerator,
            _stubAuthorizeResponseGenerator,
            _mockUserSession,
            _telemetry.Object);
    }

    internal class TestAuthorizeEndpoint : AuthorizeEndpointBase
    {
        public TestAuthorizeEndpoint(
            IEventService events,
            ILogger<TestAuthorizeEndpoint> logger,
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession,
            ITelemetryService telemetry)
            : base(events, logger, options, validator, interactionGenerator, authorizeResponseGenerator, userSession, telemetry)
        {
        }

        public override Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}