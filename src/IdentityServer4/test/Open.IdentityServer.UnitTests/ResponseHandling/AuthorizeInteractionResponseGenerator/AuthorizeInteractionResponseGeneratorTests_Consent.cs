// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityModel;
using IdentityServer.UnitTests.Common;
using Open.IdentityServer;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.AuthorizeInteractionResponseGenerator;

public class AuthorizeInteractionResponseGeneratorTests_Consent
{
    private readonly Open.IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator _subject;
    private readonly MockConsentService _mockConsent = new();
    private readonly MockProfileService _fakeUserService = new();

    private void RequiresConsent(bool value)
    {
        _mockConsent.RequiresConsentResult = value;
    }

    private void AssertUpdateConsentNotCalled()
    {
        _mockConsent.ConsentClient.Should().BeNull();
        _mockConsent.ConsentSubject.Should().BeNull();
        _mockConsent.ConsentScopes.Should().BeNull();
    }

    private void AssertUpdateConsentCalled(Client client, ClaimsPrincipal user, params string[] scopes)
    {
        _mockConsent.ConsentClient.Should().BeSameAs(client);
        _mockConsent.ConsentSubject.Should().BeSameAs(user);
        _mockConsent.ConsentScopes.Should().BeEquivalentTo(scopes);
    }

    private static IEnumerable<IdentityResource> GetIdentityScopes()
    {
        return
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        ];
    }

    private static IEnumerable<ApiResource> GetApiResources()
    {
        return
        [
            new()
            {
                Name = "api",
                Scopes = { "read", "write", "forbidden" }
            }
        ];
    }

    private static IEnumerable<ApiScope> GetScopes()
    {
        return
        [
            new()
            {
                Name = "read",
                DisplayName = "Read data",
                Emphasize = false
            },
            new()
            {
                Name = "write",
                DisplayName = "Write data",
                Emphasize = true
            },
            new()
            {
                Name = "forbidden",
                DisplayName = "Forbidden scope",
                Emphasize = true
            }
        ];
    }

    public AuthorizeInteractionResponseGeneratorTests_Consent()
    {
        _subject = new Open.IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator(
            new StubClock(),
            TestLogger.Create<Open.IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator>(),
            _mockConsent,
            _fakeUserService);
    }

    private static ResourceValidationResult GetValidatedResources(params string[] scopes)
    {
        var resources = new Resources(GetIdentityScopes(), GetApiResources(), GetScopes());
        return new ResourceValidationResult(resources).Filter(scopes);
    }


    [Fact]
    public async Task ProcessConsentAsync_NullRequest_Throws()
    {
        Func<Task> act = () => _subject.ProcessConsentAsync(null, new ConsentResponse());

        (await act.Should().ThrowAsync<ArgumentNullException>())
            .And.ParamName.Should().Be("request");
    }
        
    [Fact]
    public async Task ProcessConsentAsync_AllowsNullConsent()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.Consent],
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        await _subject.ProcessConsentAsync(request);
    }

    [Fact]
    public async Task ProcessConsentAsync_PromptModeIsLogin_Throws()
    {
        RequiresConsent(true);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.Login],
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };

        Func<Task> act = () => _subject.ProcessConsentAsync(request);

        (await act.Should().ThrowAsync<ArgumentException>()).And.Message.Should().Contain("PromptMode");
    }

    [Fact]
    public async Task ProcessConsentAsync_PromptModeIsSelectAccount_Throws()
    {
        RequiresConsent(true);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.SelectAccount],
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };

        Func<Task> act = () => _subject.ProcessConsentAsync(request);

        (await act.Should().ThrowAsync<ArgumentException>()).And.Message.Should().Contain("PromptMode");
    }


    [Fact]
    public async Task ProcessConsentAsync_RequiresConsentButPromptModeIsNone_ReturnsErrorResult()
    {
        RequiresConsent(true);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.None],
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var result = await _subject.ProcessConsentAsync(request);

        request.WasConsentShown.Should().BeFalse();
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.ConsentRequired);
        AssertUpdateConsentNotCalled();
    }
        
    [Fact]
    public async Task ProcessConsentAsync_PromptModeIsConsent_NoPriorConsent_ReturnsConsentResult()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.Consent],
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var result = await _subject.ProcessConsentAsync(request);
        request.WasConsentShown.Should().BeFalse();
        result.IsConsent.Should().BeTrue();
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_NoPriorConsent_ReturnsConsentResult()
    {
        RequiresConsent(true);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.Consent],
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var result = await _subject.ProcessConsentAsync(request);
        request.WasConsentShown.Should().BeFalse();
        result.IsConsent.Should().BeTrue();
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessConsentAsync_PromptModeIsConsent_ConsentNotGranted_ReturnsErrorResult()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.Consent],
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };

        var consent = new ConsentResponse
        {
            RememberConsent = false,
            ScopesValuesConsented = []
        };
        var result = await _subject.ProcessConsentAsync(request, consent);
        request.WasConsentShown.Should().BeTrue();
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentNotGranted_ReturnsErrorResult()
    {
        RequiresConsent(true);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var consent = new ConsentResponse
        {
            RememberConsent = false,
            ScopesValuesConsented = []
        };
        var result = await _subject.ProcessConsentAsync(request, consent);
        request.WasConsentShown.Should().BeTrue();
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGrantedButMissingRequiredScopes_ReturnsErrorResult()
    {
        RequiresConsent(true);
        var client = new Client();
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
            Client = client
        };

        var consent = new ConsentResponse
        {
            RememberConsent = false,
            ScopesValuesConsented = ["read"]
        };

        var result = await _subject.ProcessConsentAsync(request, consent);
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
    {
        RequiresConsent(true);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            Client = new Client {
                AllowRememberConsent = false
            },
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var consent = new ConsentResponse
        {
            RememberConsent = false,
            ScopesValuesConsented = ["openid", "read"]
        };
        var result = await _subject.ProcessConsentAsync(request, consent);
        request.ValidatedResources.Resources.IdentityResources.Count().Should().Be(1);
        request.ValidatedResources.Resources.ApiScopes.Count().Should().Be(1);
        "openid".Should().Be(request.ValidatedResources.Resources.IdentityResources.Select(x => x.Name).First());
        "read".Should().Be(request.ValidatedResources.Resources.ApiScopes.First().Name);
        request.WasConsentShown.Should().BeTrue();
        result.IsConsent.Should().BeFalse();
        AssertUpdateConsentNotCalled();
    }
        
    [Fact]
    public async Task ProcessConsentAsync_PromptModeConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
    {
        RequiresConsent(true);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            Client = new Client {
                AllowRememberConsent = false
            },
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var consent = new ConsentResponse
        {
            RememberConsent = false,
            ScopesValuesConsented = ["openid", "read"]
        };
        var result = await _subject.ProcessConsentAsync(request, consent);
        request.ValidatedResources.Resources.IdentityResources.Count().Should().Be(1);
        request.ValidatedResources.Resources.ApiScopes.Count().Should().Be(1);
        "read".Should().Be(request.ValidatedResources.Resources.ApiScopes.First().Name);
        request.WasConsentShown.Should().BeTrue();
        result.IsConsent.Should().BeFalse();
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessConsentAsync_AllowConsentSelected_SavesConsent()
    {
        RequiresConsent(true);
        var client = new Client { AllowRememberConsent = true };
        var user = new ClaimsPrincipal();
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            Client = client,
            Subject = user,
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var consent = new ConsentResponse
        {
            RememberConsent = true,
            ScopesValuesConsented = ["openid", "read"]
        };
        await _subject.ProcessConsentAsync(request, consent);
        AssertUpdateConsentCalled(client, user, "openid", "read");
    }

    [Fact]
    public async Task ProcessConsentAsync_NotRememberingConsent_DoesNotSaveConsent()
    {
        RequiresConsent(true);
        var client = new Client { AllowRememberConsent = true };
        var user = new ClaimsPrincipal();
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            Client = client,
            Subject = user,
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };
        var consent = new ConsentResponse
        {
            RememberConsent = false,
            ScopesValuesConsented = ["openid", "read"]
        };
        await _subject.ProcessConsentAsync(request, consent);
        AssertUpdateConsentCalled(client, user);
    }
    
    [Fact]
    public async Task ProcessInteractionAsync_AuthenticatedUserWithDeniedConsentAndNoConsentRequirement_ShouldReturnAccessDenied()
    {
        RequiresConsent(false);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal()
        };
        var consent = new ConsentResponse
        {
            Error = AuthorizationError.AccessDenied
        };
        var result = await _subject.ProcessInteractionAsync(request, consent);
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
        
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessInteractionAsync_AuthenticatedUserWithDeniedConsentAndErrorDescription_ShouldPropagateErrorDescription()
    {
        RequiresConsent(false);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal()
        };
        var consent = new ConsentResponse
        {
            Error = AuthorizationError.AccessDenied,
            ErrorDescription = "user declined the terms of service"
        };
        var result = await _subject.ProcessInteractionAsync(request, consent);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
        result.ErrorDescription.Should().Be("user declined the terms of service");

        AssertUpdateConsentNotCalled();
    }

    [Theory]
    [InlineData(AuthorizationError.LoginRequired, OidcConstants.AuthorizeErrors.LoginRequired)]
    [InlineData(AuthorizationError.InteractionRequired, OidcConstants.AuthorizeErrors.InteractionRequired)]
    [InlineData(AuthorizationError.AccountSelectionRequired, OidcConstants.AuthorizeErrors.AccountSelectionRequired)]
    [InlineData(AuthorizationError.ConsentRequired, OidcConstants.AuthorizeErrors.ConsentRequired)]
    public async Task ProcessInteractionAsync_AuthenticatedUserWithDeniedConsentAndNoConsentRequirement_ShouldReturnCorrectError(
        AuthorizationError consentError, string expectedOidcError)
    {
        RequiresConsent(false);
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal()
        };
        var consent = new ConsentResponse
        {
            Error = consentError
        };
        var result = await _subject.ProcessInteractionAsync(request, consent);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(expectedOidcError);

        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessInteractionAsync_AnonymousUserWithDeniedConsent_ShouldReturnError()
    {
        var request = new ValidatedAuthorizeRequest
        {
            Subject = Principal.Anonymous
        };

        var consent = new ConsentResponse
        {
            Error = AuthorizationError.AccessDenied
        };

        var result = await _subject.ProcessInteractionAsync(request, consent);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);

        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessInteractionAsync_AuthenticatedUserWithGrantedConsentAndNoConsentRequirement_ShouldNotError()
    {
        RequiresConsent(false);

        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            Client = new Client
            {
                EnableLocalLogin = true
            },
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal(),
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };

        var consent = new ConsentResponse
        {
            ScopesValuesConsented = ["openid", "read", "write"]
        };

        var result = await _subject.ProcessInteractionAsync(request, consent);

        result.IsError.Should().BeFalse();
        result.IsLogin.Should().BeFalse();
        result.IsConsent.Should().BeFalse();
        AssertUpdateConsentNotCalled();
    }

    [Fact]
    public async Task ProcessInteractionAsync_AuthenticatedUserWithNullConsentAndNoConsentRequirement_ShouldNotError()
    {
        RequiresConsent(false);

        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            Client = new Client
            {
                EnableLocalLogin = true
            },
            Subject = new IdentityServerUser("123")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider
            }.CreatePrincipal(),
            RequestedScopes = ["openid", "read", "write"],
            ValidatedResources = GetValidatedResources("openid", "read", "write"),
        };

        var result = await _subject.ProcessInteractionAsync(request, consent: null);

        result.IsError.Should().BeFalse();
        result.IsLogin.Should().BeFalse();
        result.IsConsent.Should().BeFalse();
        AssertUpdateConsentNotCalled();
    }
}