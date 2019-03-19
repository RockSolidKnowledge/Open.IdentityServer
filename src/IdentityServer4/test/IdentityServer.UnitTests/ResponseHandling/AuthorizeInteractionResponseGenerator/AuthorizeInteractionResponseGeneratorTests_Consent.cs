// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using IdentityServer.UnitTests.Common;

namespace IdentityServer4.UnitTests.ResponseHandling
{
    public class AuthorizeInteractionResponseGeneratorTests_Consent
    {
        private AuthorizeInteractionResponseGenerator _subject;
        private IdentityServerOptions _options = new IdentityServerOptions();
        private MockConsentService _mockConsent = new MockConsentService();
        private MockProfileService _fakeUserService = new MockProfileService();

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
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }

        private static IEnumerable<ApiResource> GetApiScopes()
        {
            return new ApiResource[]
            {
                new ApiResource
                {
                    Name = "api",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "read",
                            DisplayName = "Read data",
                            Emphasize = false
                        },
                        new Scope
                        {
                            Name = "write",
                            DisplayName = "Write data",
                            Emphasize = true
                        },
                        new Scope
                        {
                            Name = "forbidden",
                            DisplayName = "Forbidden scope",
                            Emphasize = true
                        }
                    }
                }
             };
        }
        
        public AuthorizeInteractionResponseGeneratorTests_Consent()
        {
            _subject = new AuthorizeInteractionResponseGenerator(
                new StubClock(),
                TestLogger.Create<AuthorizeInteractionResponseGenerator>(),
                _mockConsent,
                _fakeUserService);
        }

        [Fact]
        public void ProcessConsentAsync_NullRequest_Throws()
        {
            Func<Task> act = () => _subject.ProcessConsentAsync(null, new ConsentResponse());

            act.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("request");
        }
        
        [Fact]
        public void ProcessConsentAsync_AllowsNullConsent()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            }; 
            var result = _subject.ProcessConsentAsync(request, null).Result;
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsLogin_Throws()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Login
            };

            Func<Task> act = () => _subject.ProcessConsentAsync(request);

            act.Should().Throw<ArgumentException>()
                .And.Message.Should().Contain("PromptMode");
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsSelectAccount_Throws()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.SelectAccount
            };

            Func<Task> act = () => _subject.ProcessConsentAsync(request);

            act.Should().Throw<ArgumentException>()
                .And.Message.Should().Contain("PromptMode");
        }


        [Fact]
        public void ProcessConsentAsync_RequiresConsentButPromptModeIsNone_ReturnsErrorResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.None
            };
            var result = _subject.ProcessConsentAsync(request).Result;

            request.WasConsentShown.Should().BeFalse();
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.ConsentRequired);
            AssertUpdateConsentNotCalled();
        }
        
        [Fact]
        public void ProcessConsentAsync_PromptModeIsConsent_NoPriorConsent_ReturnsConsentResult()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            };
            var result = _subject.ProcessConsentAsync(request).Result;
            request.WasConsentShown.Should().BeFalse();
            result.IsConsent.Should().BeTrue();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_NoPriorConsent_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            };
            var result = _subject.ProcessConsentAsync(request).Result;
            request.WasConsentShown.Should().BeFalse();
            result.IsConsent.Should().BeTrue();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsConsent_ConsentNotGranted_ReturnsErrorResult()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            };

            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] {}
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentNotGranted_ReturnsErrorResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback"
            };
            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] {}
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGrantedButMissingRequiredScopes_ReturnsErrorResult()
        {
            RequiresConsent(true);
            var client = new Client {};
            var scopeValidator = new ScopeValidator(new InMemoryResourcesStore(GetIdentityScopes(), GetApiScopes()), TestLogger.Create<ScopeValidator>());
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                RequestedScopes = new List<string> { "openid", "read" },
                ValidatedScopes = scopeValidator,
                Client = client
            };
            var valid = scopeValidator.AreScopesValidAsync(request.RequestedScopes).Result;

            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] { "read" }
            };

            var result = _subject.ProcessConsentAsync(request, consent).Result;
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public async Task ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                ValidatedScopes = new ScopeValidator(new InMemoryResourcesStore(GetIdentityScopes(), GetApiScopes()), new LoggerFactory().CreateLogger<ScopeValidator>()),
                Client = new Client {
                    AllowRememberConsent = false
                }
            };
            await request.ValidatedScopes.AreScopesValidAsync(new string[] { "read", "write" });
            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] { "read" }
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.ValidatedScopes.GrantedResources.ApiResources.SelectMany(x=>x.Scopes).Count().Should().Be(1);
            "read".Should().Be(request.ValidatedScopes.GrantedResources.ApiResources.SelectMany(x=>x.Scopes).First().Name);
            request.WasConsentShown.Should().BeTrue();
            result.IsConsent.Should().BeFalse();
            AssertUpdateConsentNotCalled();
        }
        
        [Fact]
        public async Task ProcessConsentAsync_PromptModeConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                ValidatedScopes = new ScopeValidator(new InMemoryResourcesStore(GetIdentityScopes(), GetApiScopes()), new LoggerFactory().CreateLogger<ScopeValidator>()),
                Client = new Client {
                    AllowRememberConsent = false
                }
            };
            await request.ValidatedScopes.AreScopesValidAsync(new string[] { "read", "write" });
            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] { "read" }
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.ValidatedScopes.GrantedResources.ApiResources.SelectMany(x=>x.Scopes).Count().Should().Be(1);
            "read".Should().Be(request.ValidatedScopes.GrantedResources.ApiResources.SelectMany(x => x.Scopes).First().Name);
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
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                ValidatedScopes = new ScopeValidator(new InMemoryResourcesStore(GetIdentityScopes(), GetApiScopes()), new LoggerFactory().CreateLogger<ScopeValidator>()),
                Client = client,
                Subject = user
            };
            await request.ValidatedScopes.AreScopesValidAsync(new string[] { "read", "write" });
            var consent = new ConsentResponse
            {
                RememberConsent = true,
                ScopesConsented = new string[] { "read" }
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            AssertUpdateConsentCalled(client, user, "read");
        }

    }
}