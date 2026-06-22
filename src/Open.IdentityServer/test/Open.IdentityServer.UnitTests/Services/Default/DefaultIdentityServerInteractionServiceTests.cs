// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultIdentityServerInteractionServiceTests
{
    private readonly DefaultIdentityServerInteractionService _subject;

    private readonly IdentityServerOptions _options = new();
    private readonly MockHttpContextAccessor _mockMockHttpContextAccessor;
    private readonly MockMessageStore<LogoutNotificationContext> _mockEndSessionStore = new();
    private readonly MockMessageStore<LogoutMessage> _mockLogoutMessageStore = new();
    private readonly MockMessageStore<ErrorMessage> _mockErrorMessageStore = new();
    private readonly MockConsentMessageStore _mockConsentStore = new();
    private readonly MockPersistedGrantService _mockPersistedGrantService = new();
    private readonly MockUserSession _mockUserSession = new();
    private readonly MockReturnUrlParser _mockReturnUrlParser = new();
    private readonly Mock<ITelemetryService> _telemetry = new();

    private readonly ResourceValidationResult _resourceValidationResult;

    public DefaultIdentityServerInteractionServiceTests()
    {
        _mockMockHttpContextAccessor = new MockHttpContextAccessor(_options, _mockUserSession, _mockEndSessionStore);

        _subject = new DefaultIdentityServerInteractionService(new StubClock(), 
            _mockMockHttpContextAccessor,
            _mockLogoutMessageStore,
            _mockErrorMessageStore,
            _mockConsentStore,
            _mockPersistedGrantService,
            _mockUserSession,
            _mockReturnUrlParser,
            _telemetry.Object,
            TestLogger.Create<DefaultIdentityServerInteractionService>()
        );

        _resourceValidationResult = new ResourceValidationResult();
        _resourceValidationResult.Resources.IdentityResources.Add(new IdentityResources.OpenId());
        _resourceValidationResult.ParsedScopes.Add(new ParsedScopeValue("openid"));
    }
        
    [Fact]
    public async Task GetLogoutContextAsync_valid_session_and_logout_id_should_not_provide_signout_iframe()
    {
        // for this, we're just confirming that since the session has changed, there's not use in doing the iframe and thsu SLO
        _mockUserSession.SessionId = null;
        _mockLogoutMessageStore.Messages.Add("id", new Message<LogoutMessage>(new LogoutMessage() { SessionId = "session" }));

        var context = await _subject.GetLogoutContextAsync("id");

        context.SignOutIFrameUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetLogoutContextAsync_valid_session_no_logout_id_should_provide_iframe()
    {
        _mockUserSession.Clients.Add("foo");
        _mockUserSession.SessionId = "session";
        _mockUserSession.User = new IdentityServerUser("123").CreatePrincipal();

        var context = await _subject.GetLogoutContextAsync(null);

        context.SignOutIFrameUrl.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLogoutContextAsync_without_session_should_not_provide_iframe()
    {
        _mockUserSession.SessionId = null;
        _mockLogoutMessageStore.Messages.Add("id", new Message<LogoutMessage>(new LogoutMessage()));

        var context = await _subject.GetLogoutContextAsync("id");

        context.SignOutIFrameUrl.Should().BeNull();
    }

    [Fact]
    public async Task CreateLogoutContextAsync_without_session_should_not_create_session()
    {
        var context = await _subject.CreateLogoutContextAsync();

        context.Should().BeNull();
        _mockLogoutMessageStore.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateLogoutContextAsync_with_session_should_create_session()
    {
        _mockUserSession.Clients.Add("foo");
        _mockUserSession.User = new IdentityServerUser("123").CreatePrincipal();
        _mockUserSession.SessionId = "session";

        var context = await _subject.CreateLogoutContextAsync();

        context.Should().NotBeNull();
        _mockLogoutMessageStore.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GrantConsentAsync_should_throw_if_granted_and_no_subject()
    {
        Func<Task> act = () => _subject.GrantConsentAsync(
            new AuthorizationRequest(), 
            new ConsentResponse() { ScopesValuesConsented = ["openid"] });

        (await act.Should().ThrowAsync<ArgumentNullException>()).And.Message.Should().Contain("subject");
    }

    [Fact]
    public async Task GrantConsentAsync_should_allow_deny_for_anonymous_users()
    {
        var req = new AuthorizationRequest()
        {
            Client = new Client { ClientId = "client" },
            ValidatedResources = _resourceValidationResult
        };
        await _subject.GrantConsentAsync(req, new ConsentResponse { Error = AuthorizationError.AccessDenied });
    }

    [Fact]
    public async Task GrantConsentAsync_should_use_current_subject_and_create_message()
    {
        _mockUserSession.User = new IdentityServerUser("bob").CreatePrincipal();

        var req = new AuthorizationRequest() { 
            Client = new Client { ClientId = "client" },
            ValidatedResources = _resourceValidationResult
        };
        await _subject.GrantConsentAsync(req, new ConsentResponse());

        _mockConsentStore.Messages.Should().NotBeEmpty();
        var consentRequest = new ConsentRequest(req, "bob");
        _mockConsentStore.Messages.First().Key.Should().Be(consentRequest.Id);
    }

    [Fact]
    public async Task GetAuthorizationContextAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        await _subject.GetAuthorizationContextAsync("return-url");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.GetAuthorizationContextAsync)));
    }

    [Fact]
    public void IsValidReturnUrl_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _subject.IsValidReturnUrl("return-url");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.IsValidReturnUrl)));
    }

    [Fact]
    public async Task GetErrorContextAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        await _subject.GetErrorContextAsync("error-id");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.GetErrorContextAsync)));
    }

    [Fact]
    public async Task GetLogoutContextAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        await _subject.GetLogoutContextAsync("logout-id");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.GetLogoutContextAsync)));
    }

    [Fact]
    public async Task CreateLogoutContextAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        await _subject.CreateLogoutContextAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.CreateLogoutContextAsync)));
    }

    [Fact]
    public async Task GrantConsentAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var request = CreateValidRequest();
        await _subject.GrantConsentAsync(request, new ConsentResponse(), "subject");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.GrantConsentAsync)));
    }
    
    private AuthorizationRequest CreateValidRequest()
    {
        return new AuthorizationRequest()
        {
            Client = new Client { ClientId = "client" },
            ValidatedResources = _resourceValidationResult
        };
    }

    [Fact]
    public async Task DenyAuthorizationAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var request = CreateValidRequest();
        await _subject.DenyAuthorizationAsync(request, AuthorizationError.AccessDenied);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.DenyAuthorizationAsync)));
    }

    [Fact]
    public async Task GetAllUserGrantsAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        await _subject.GetAllUserGrantsAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.GetAllUserGrantsAsync)));
    }

    [Fact]
    public async Task RevokeUserConsentAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        await _subject.RevokeUserConsentAsync("client-id");
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.RevokeUserConsentAsync)));
    }

    [Fact]
    public async Task RevokeTokensForCurrentSessionAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        await _subject.RevokeTokensForCurrentSessionAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, 
            _subject, 
            nameof(_subject.RevokeTokensForCurrentSessionAsync)));
    }
}