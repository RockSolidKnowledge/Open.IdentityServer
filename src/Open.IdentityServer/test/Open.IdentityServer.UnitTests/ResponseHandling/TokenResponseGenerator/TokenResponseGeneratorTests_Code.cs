// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public class TokenResponseGeneratorTests_Code : TokenResponseGeneratorTests
{
    [Fact]
    public async Task ProcessAsync_AuthorizationCode_ReturnsAccessToken()
    {
        var capturedRequest = SetupTokenService("auth_code_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["resource"],
                IsOpenId = false
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("auth_code_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.Scope.Should().Be("resource");

        capturedRequest.Value.Should().NotBeNull();
        capturedRequest.Value!.Subject.Should().BeEquivalentTo(request.AuthorizationCode.Subject);
        capturedRequest.Value.ValidatedResources.Resources.ApiResources
            .Should().ContainSingle(x => x.Name == "api")
            .And.HaveCount(1);

        capturedRequest.Value.ValidatedResources.RawScopeValues.Should().BeEquivalentTo(request.AuthorizationCode.RequestedScopes);
        capturedRequest.Value.ResourceIndicatorsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCodeWithRequestedResourceIndicators_AndNoTokenResourceIndicator_ReturnsAccessTokenScopedToOriginalAuthCode()
    {
        var capturedRequest = SetupTokenService("auth_code_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["urn:valid.resource:Read", "urn:valid.resource:Write", "valid:Read", "resource"],
                IsOpenId = false,
                RequestedResourceIndicators = ["urn:valid.resource", "https://valid.resource.com"],
            },
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("auth_code_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.Scope.Should().Be("urn:valid.resource:Read urn:valid.resource:Write valid:Read resource");

        capturedRequest.Value.Should().NotBeNull();
        capturedRequest.Value!.Subject.Should().BeEquivalentTo(request.AuthorizationCode.Subject);
        capturedRequest.Value.ValidatedResources.Resources.ApiResources
            .Should().ContainSingle(x => x.Name == "urn:valid.resource")
            .And.ContainSingle(x => x.Name == "https://valid.resource.com")
            .And.ContainSingle(x => x.Name == "api")
            .And.HaveCount(3);

        capturedRequest.Value.ValidatedResources.RawScopeValues.Should().BeEquivalentTo("urn:valid.resource:Read", "urn:valid.resource:Write", "valid:Read", "resource");
        capturedRequest.Value.ResourceIndicatorsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_WhenResourceIndicatorPresent_ReturnsDownscopedAccessToken_BuiltWithResourceIndicator()
    {
        var capturedRequest = SetupTokenService("auth_code_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["urn:valid.resource:Read", "urn:valid.resource:Write", "valid:Read"],
                IsOpenId = false,
                RequestedResourceIndicators = ["urn:valid.resource", "https://valid.resource.com"],
            },
            RequestedResourceIndicator = "urn:valid.resource",
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("auth_code_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.Scope.Should().Be("urn:valid.resource:Read urn:valid.resource:Write");

        capturedRequest.Value.Should().NotBeNull();
        capturedRequest.Value!.Subject.Should().BeEquivalentTo(request.AuthorizationCode.Subject);
        capturedRequest.Value.ValidatedResources.Resources.ApiResources
            .Should().ContainSingle(x => x.Name == "urn:valid.resource")
            .And.HaveCount(1);

        capturedRequest.Value.ValidatedResources.RawScopeValues.Should().BeEquivalentTo("urn:valid.resource:Read", "urn:valid.resource:Write");
        capturedRequest.Value.ResourceIndicatorsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_WithReferenceToken_ReturnsReferenceToken_AndAddsTokenReferenceToAuthCode()
    {
        string fakeReferenceTokenHandle = "reference_token";
        Token fakeReferenceToken = new Token { Lifetime = 3600, AccessTokenType = AccessTokenType.Reference };
        SetupTokenService(fakeReferenceTokenHandle, fakeReferenceToken);

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCodeHandle = "auth_code_handle",
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["resource"],
                IsOpenId = false
            }
        };

        AuthorizationCode capturedAuthCode = null;
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.UpdateAuthorizationCodeAsync(request.AuthorizationCodeHandle, It.IsAny<AuthorizationCode>()))
            .Callback<string, AuthorizationCode>((_, authCode) => { capturedAuthCode = authCode; });

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be(fakeReferenceTokenHandle);

        capturedAuthCode.Should().NotBeNull();
        capturedAuthCode.Should().BeEquivalentTo(request.AuthorizationCode, cnf => cnf.Excluding(x => x.ChildGrant));
        capturedAuthCode.ChildGrant.Should().Be(fakeReferenceTokenHandle);
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_WithOfflineAccess_ReturnsRefreshToken_AndAddsTokenReferenceToAuthCode()
    {
        string fakeRefreshTokenHandle = "auth_refresh";
        Token fakeAccessToken = new Token { Lifetime = 3600 };
        SetupTokenService("auth_code_token", fakeAccessToken);
        var capturedRefreshTokenCreation = SetupRefreshTokenService(fakeRefreshTokenHandle);

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["resource", IdentityServerConstants.StandardScopes.OfflineAccess],
                IsOpenId = false
            }
        };

        AuthorizationCode capturedAuthCode = null;
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.UpdateAuthorizationCodeAsync(request.AuthorizationCodeHandle, It.IsAny<AuthorizationCode>()))
            .Callback<string, AuthorizationCode>((_, authCode) => { capturedAuthCode = authCode; });

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.RefreshToken.Should().Be(fakeRefreshTokenHandle);

        capturedRefreshTokenCreation.Value.Should().NotBeNull();
        capturedRefreshTokenCreation.Value!.AccessToken.Should().BeEquivalentTo(fakeAccessToken);
        capturedRefreshTokenCreation.Value.AuthorisedScopes.Should().BeEquivalentTo(request.AuthorizationCode.RequestedScopes);
        capturedRefreshTokenCreation.Value.AuthorisedResourceIndicators.Should().BeNullOrEmpty();
        capturedRefreshTokenCreation.Value.RequestedResourceIndicator.Should().BeNullOrEmpty();

        capturedAuthCode.Should().NotBeNull();
        capturedAuthCode.Should().BeEquivalentTo(request.AuthorizationCode, cnf => cnf.Excluding(x => x.ChildGrant));
        capturedAuthCode.ChildGrant.Should().Be(fakeRefreshTokenHandle);
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_WithOfflineAccessAndResourceIndicator_ReturnsRefreshToken()
    {
        Token fakeAccessToken = new Token { Lifetime = 3600 };
        SetupTokenService("auth_code_token", fakeAccessToken);
        var capturedRefreshTokenCreation = SetupRefreshTokenService("auth_refresh");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["urn:valid.resource:Read", "urn:valid.resource:Write", "valid:Read", IdentityServerConstants.StandardScopes.OfflineAccess],
                IsOpenId = false,
                RequestedResourceIndicators = ["urn:valid.resource", "https://valid.resource.com"],
            },
            RequestedResourceIndicator = "urn:valid.resource",
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.RefreshToken.Should().Be("auth_refresh");

        capturedRefreshTokenCreation.Value.Should().NotBeNull();
        capturedRefreshTokenCreation.Value!.AccessToken.Should().BeEquivalentTo(fakeAccessToken);
        capturedRefreshTokenCreation.Value.AuthorisedScopes.Should().BeEquivalentTo(request.AuthorizationCode.RequestedScopes);
        capturedRefreshTokenCreation.Value.AuthorisedResourceIndicators.Should().BeEquivalentTo(request.AuthorizationCode.RequestedResourceIndicators);
        capturedRefreshTokenCreation.Value.RequestedResourceIndicator.Should().BeEquivalentTo(request.RequestedResourceIndicator);
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_WithOpenId_ReturnsIdToken()
    {
        SetupTokenServiceWithIdToken("auth_code_token", "auth_id_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["openid", "resource"],
                IsOpenId = true,
                Nonce = "nonce_value",
                StateHash = "state_hash"
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.IdentityToken.Should().Be("auth_id_token");
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_WithOpenId_ClientNotFound_Throws()
    {
        SetupTokenService("auth_code_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "nonexistent_client",
                Subject = CreateSubject(),
                RequestedScopes = ["openid"],
                IsOpenId = true
            }
        };

        var sut = CreateSut();
        var act = () => sut.ProcessAsync(new TokenRequestValidationResult(request));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client does not exist anymore.");
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_WithoutOpenId_DoesNotReturnIdToken()
    {
        SetupTokenService("auth_code_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["resource"],
                IsOpenId = false
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.IdentityToken.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_AuthorizationCode_ClientNullId_Throws()
    {
        SetupTokenService();

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = null,
                Subject = CreateSubject(),
                RequestedScopes = ["resource"],
                IsOpenId = false
            }
        };

        var sut = CreateSut();
        var act = () => sut.ProcessAsync(new TokenRequestValidationResult(request));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}