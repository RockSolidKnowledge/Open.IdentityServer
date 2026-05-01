// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.AuthorizeResponseGenerator;

public class AuthorizeResponseGeneratorTests_Code : AuthorizeResponseGeneratorTests
{
    [Fact]
    public async Task CreateResponseAsync_CodeFlow_ReturnsCodeResponse()
    {
        var request = CreateValidatedAuthorizeRequest(GrantType.AuthorizationCode);

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .ReturnsAsync("code_id");

        var sut = CreateSut();
        var response = await sut.CreateResponseAsync(request);

        response.Code.Should().Be("code_id");
        response.Request.Should().BeSameAs(request);
        response.AccessToken.Should().BeNull();
        response.IdentityToken.Should().BeNull();
    }

    [Fact]
    public async Task CreateResponseAsync_CodeFlow_StoresAuthorizationCode()
    {
        var request = CreateValidatedAuthorizeRequest(GrantType.AuthorizationCode);

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .ReturnsAsync("code_id");

        var sut = CreateSut();
        await sut.CreateResponseAsync(request);

        Mock.Get(authorizationCodeStore)
            .Verify(x => x.StoreAuthorizationCodeAsync(It.Is<AuthorizationCode>(c =>
                c.ClientId == "client" &&
                c.RedirectUri == "https://example.com/callback" &&
                c.SessionId == "session"
            )), Times.Once);
    }

    [Fact]
    public async Task CreateResponseAsync_CodeFlow_WithRequestResourceIndicators_StoresAuthorizationCode_ContainingResourceIndicators()
    {
        var request = CreateValidatedAuthorizeRequest(GrantType.AuthorizationCode);
        request.RequestedResourceIndicators =
        [
            "urn:fake.resource.one", "https://other.resource"
        ];

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.StoreAuthorizationCodeAsync(It.IsAny<AuthorizationCode>()))
            .ReturnsAsync("code_id");

        var sut = CreateSut();
        await sut.CreateResponseAsync(request);

        Mock.Get(authorizationCodeStore)
            .Verify(x => x.StoreAuthorizationCodeAsync(It.Is<AuthorizationCode>(c =>
                c.ClientId == "client" &&
                c.RedirectUri == "https://example.com/callback" &&
                c.SessionId == "session" &&
                c.RequestedResourceIndicators == request.RequestedResourceIndicators
            )), Times.Once);
    }

    [Fact]
    public async Task CreateResponseAsync_UnsupportedGrantType_Throws()
    {
        var request = CreateValidatedAuthorizeRequest("unsupported");

        var sut = CreateSut();

        var act = () => sut.CreateResponseAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static ValidatedAuthorizeRequest CreateValidatedAuthorizeRequest(string grantType) => new()
    {
        GrantType = grantType,
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