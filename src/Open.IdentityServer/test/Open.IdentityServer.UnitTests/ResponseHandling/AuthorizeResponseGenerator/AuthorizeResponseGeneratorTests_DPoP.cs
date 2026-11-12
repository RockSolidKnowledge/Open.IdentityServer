// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.AuthorizeResponseGenerator;

public class AuthorizeResponseGeneratorTests_DPoP : AuthorizeResponseGeneratorTests
{
    private const string Category = "AuthorizeResponse Generator - DPoP";
    
    [Fact]
    [Trait("Category", Category)]
    public async Task CreateResponseAsync_CodeFlow_AndDPoPThumbprint_ReturnsCodeResponse_StoringThumbprint()
    {
        var request = CreateValidatedAuthorizeRequest(GrantType.AuthorizationCode);
        request.DPoPThumbprint = Guid.NewGuid().ToString();

        AuthorizationCode storedCode = null;
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .Callback<AuthorizationCode>(code => { storedCode = code; })
            .ReturnsAsync("code_id");

        var sut = CreateSut();
        var response = await sut.CreateResponseAsync(request);

        response.Code.Should().Be("code_id");
        response.Request.Should().BeSameAs(request);
        response.AccessToken.Should().BeNull();
        response.IdentityToken.Should().BeNull();

        storedCode.Should().NotBeNull();
        storedCode.DPoPKeyThumbprint.Should().Be(request.DPoPThumbprint);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task CreateResponseAsync_HybridFlow_AndDPoPThumbprint_WithCodeAndIdToken_StoringThumbprint()
    {
        var request = CreateHybridRequest($"{OidcConstants.ResponseTypes.Code} {OidcConstants.ResponseTypes.IdToken}");
        request.DPoPThumbprint = Guid.NewGuid().ToString();

        AuthorizationCode storedCode = null;
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .Callback<AuthorizationCode>(code => { storedCode = code; })
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

        storedCode.Should().NotBeNull();
        storedCode.DPoPKeyThumbprint.Should().Be(request.DPoPThumbprint);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task CreateResponseAsync_ImplicitFlow_AndDPoPThumbprint_ShouldBeIgnored()
    {
        var request = CreateImplicitRequest(OidcConstants.ResponseTypes.Token);
        request.DPoPThumbprint = Guid.NewGuid().ToString();

        var token = new Token { Lifetime = 3600 };
        Mock.Get(tokenService)
            .Setup(x => x.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(token);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(token))
            .ReturnsAsync("access_token_value");

        var sut = CreateSut();
        var response = await sut.CreateResponseAsync(request);
        
        response.AccessToken.Should().Be("access_token_value");
        response.AccessTokenLifetime.Should().Be(3600);
        response.Code.Should().BeNull();
        response.IdentityToken.Should().BeNull();
    }
}