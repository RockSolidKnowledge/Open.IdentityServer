// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityModel;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.AuthorizeResponseGenerator;

public class AuthorizeResponseGeneratorTests_Hybrid : AuthorizeResponseGeneratorTests
{
    [Fact]
    public async Task CreateResponseAsync_HybridFlow_WithCodeAndIdToken_ReturnsCodeAndIdToken()
    {
        var request = CreateHybridRequest($"{OidcConstants.ResponseTypes.Code} {OidcConstants.ResponseTypes.IdToken}");

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .ReturnsAsync("code_id");

        var idToken = new Token();
        Mock.Get(tokenService)
            .Setup(x => x.CreateIdentityTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(idToken);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(idToken))
            .ReturnsAsync("id_token_jwt");

        var sut = CreateSut();
        var response = await sut.CreateResponseAsync(request);

        response.Code.Should().Be("code_id");
        response.IdentityToken.Should().Be("id_token_jwt");
    }

    [Fact]
    public async Task CreateResponseAsync_HybridFlow_WithCodeAndToken_ReturnsCodeAndAccessToken()
    {
        var request = CreateHybridRequest($"{OidcConstants.ResponseTypes.Code} {OidcConstants.ResponseTypes.Token}");

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .ReturnsAsync("code_id");

        var accessToken = new Token { Lifetime = 3600 };
        Mock.Get(tokenService)
            .Setup(x => x.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(accessToken);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(accessToken))
            .ReturnsAsync("access_token_value");

        var sut = CreateSut();
        var response = await sut.CreateResponseAsync(request);

        response.Code.Should().Be("code_id");
        response.AccessToken.Should().Be("access_token_value");
        response.AccessTokenLifetime.Should().Be(3600);
    }

    [Fact]
    public async Task CreateResponseAsync_HybridFlow_StoresAuthorizationCode()
    {
        var request = CreateHybridRequest($"{OidcConstants.ResponseTypes.Code} {OidcConstants.ResponseTypes.IdToken}");

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .ReturnsAsync("code_id");

        var idToken = new Token();
        Mock.Get(tokenService)
            .Setup(x => x.CreateIdentityTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(idToken);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(idToken))
            .ReturnsAsync("id_token_jwt");

        var sut = CreateSut();
        await sut.CreateResponseAsync(request);

        Mock.Get(authorizationCodeStore)
            .Verify(x => x.StoreAuthorizationCodeAsync(It.Is<AuthorizationCode>(c =>
                c.ClientId == "client"
            )), Times.Once);
    }

    [Fact]
    public async Task CreateResponseAsync_HybridFlow_WithRequestResourceIndicators_StoresAuthorizationCode_ContainingResourceIndicators()
    {
        var request = CreateHybridRequest($"{OidcConstants.ResponseTypes.Code} {OidcConstants.ResponseTypes.IdToken}");
        request.RequestedResourceIndicators =
        [
            "urn:fake.resource.one", "https://other.resource"
        ];

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .ReturnsAsync("code_id");

        var idToken = new Token();
        Mock.Get(tokenService)
            .Setup(x => x.CreateIdentityTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(idToken);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(idToken))
            .ReturnsAsync("id_token_jwt");

        var sut = CreateSut();
        await sut.CreateResponseAsync(request);

        Mock.Get(authorizationCodeStore)
            .Verify(x => x.StoreAuthorizationCodeAsync(It.Is<AuthorizationCode>(c =>
                c.ClientId == "client" &&
                c.RequestedResourceIndicators == request.RequestedResourceIndicators
            )), Times.Once);
    }

    private static ValidatedAuthorizeRequest CreateHybridRequest(string responseType) => new()
    {
        GrantType = GrantType.Hybrid,
        ResponseType = responseType,
        Client = new Client
        {
            ClientId = "client",
            AuthorizationCodeLifetime = 300,
            AllowedIdentityTokenSigningAlgorithms = []
        },
        Subject = new ClaimsPrincipal(new ClaimsIdentity()),
        ValidatedResources = new ResourceValidationResult(),
        SessionId = "session",
        RedirectUri = "https://example.com/callback",
        CodeChallenge = "challenge",
        CodeChallengeMethod = "S256",
        Raw = new NameValueCollection()
    };
}