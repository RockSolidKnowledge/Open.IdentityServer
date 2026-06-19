// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
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

namespace Open.IdentityServer.UnitTests.ResponseHandling.AuthorizeInteractionResponseGenerator;

public class AuthorizeInteractionResponseGeneratorTests
{
    private IdentityServerOptions _options = new IdentityServerOptions();
    private readonly Open.IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator _subject;
    private readonly MockConsentService _mockConsentService = new MockConsentService();
    private readonly StubClock _clock = new StubClock();
    private readonly Mock<ITelemetryService> _mockTelemetryService = new Mock<ITelemetryService>();

    public AuthorizeInteractionResponseGeneratorTests()
    {
        _subject = new Open.IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator(
            _clock,
            TestLogger.Create<Open.IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator>(),
            _mockConsentService,
            new MockProfileService(),
            _mockTelemetryService.Object);
    }

    [Fact]
    public async Task Authenticated_User_with_restricted_current_Idp_with_prompt_none_must_error()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ClientId = "foo",
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal(),
            Client = new Client
            {
                EnableLocalLogin = false,
                IdentityProviderRestrictions = new List<string>
                {
                    "some_idp"
                }
            },
            PromptModes = new[] { OidcConstants.PromptModes.None },
        };

        var result = await _subject.ProcessInteractionAsync(request);

        result.IsError.Should().BeTrue();
        result.IsLogin.Should().BeFalse();
    }

    [Fact]
    public async Task Authenticated_User_with_maxage_with_prompt_none_must_error()
    {
        _clock.UtcNowFunc = () => new DateTime(2020, 02, 03, 9, 0, 0);

        var request = new ValidatedAuthorizeRequest
        {
            ClientId = "foo",
            Subject = new IdentityServerUser("123")
            {
                AuthenticationTime = new DateTime(2020, 02, 01, 9, 0, 0),
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal(),
            Client = new Client
            {
                EnableLocalLogin = true,
            },
            PromptModes = new[] { OidcConstants.PromptModes.None },
            MaxAge = 3600
        };

        var result = await _subject.ProcessInteractionAsync(request);

        result.IsError.Should().BeTrue();
        result.IsLogin.Should().BeFalse();
    }

    [Fact]
    public async Task Authenticated_User_with_different_requested_Idp_with_prompt_none_must_error()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ClientId = "foo",
            Client = new Client(),
            AuthenticationContextReferenceClasses = new List<string>{
                "idp:some_idp"
            },
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal(),
            PromptModes = new[] { OidcConstants.PromptModes.None }
        };

        var result = await _subject.ProcessInteractionAsync(request);

        result.IsError.Should().BeTrue();
        result.IsLogin.Should().BeFalse();
    }

    [Fact]
    public async Task Authenticated_User_beyond_client_user_sso_lifetime_with_prompt_none_should_error()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ClientId = "foo",
            Client = new Client()
            {
                UserSsoLifetime = 3600 // 1h
            },
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = "local",
                AuthenticationTime = _clock.GetUtcNow().UtcDateTime.Subtract(TimeSpan.FromSeconds(3700))
            }.CreatePrincipal(),
            PromptModes = new[] { OidcConstants.PromptModes.None }
        };

        var result = await _subject.ProcessInteractionAsync(request);

        result.IsError.Should().BeTrue();
        result.IsLogin.Should().BeFalse();
    }

    [Fact]
    public async Task locally_authenticated_user_but_client_does_not_allow_local_with_prompt_none_should_error()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ClientId = "foo",
            Client = new Client()
            {
                EnableLocalLogin = false
            },
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal(),
            PromptModes = new[] { OidcConstants.PromptModes.None }
        };

        var result = await _subject.ProcessInteractionAsync(request);

        result.IsError.Should().BeTrue();
        result.IsLogin.Should().BeFalse();
    }

    [Fact]
    public async Task process_interaction_should_initiate_telemetry_trace()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ClientId = "foo",
            Client = new Client()
            {
                EnableLocalLogin = false
            },
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal(),
            PromptModes = new[] { OidcConstants.PromptModes.None }
        };

        await _subject.ProcessInteractionAsync(request);

        _mockTelemetryService.Verify(
            t => t.Trace(TelemetryConstants.TraceCategories.Basic,
                _subject,
                "ProcessInteractionAsync"));
    }
}