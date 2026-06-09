// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Linq;
using System.Security.Claims;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public abstract class TokenResponseGeneratorTests
{
    protected readonly FakeTimeProvider clock = new();
    protected readonly ITokenService tokenService = Mock.Of<ITokenService>();
    protected readonly IRefreshTokenService refreshTokenService = Mock.Of<IRefreshTokenService>();
    protected readonly IScopeParser scopeParser = new DefaultScopeParser(NullLogger<DefaultScopeParser>.Instance);
    protected readonly IResourceStore resources = new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis(), TestScopes.GetScopes());
    protected readonly IClientStore clients = new InMemoryClientStore(TestClients.Get());
    protected readonly ILogger<Open.IdentityServer.ResponseHandling.TokenResponseGenerator> logger = NullLogger<Open.IdentityServer.ResponseHandling.TokenResponseGenerator>.Instance;

    protected readonly DateTime FakeNow = new DateTime(2026, 02, 01, 12, 23, 00);

    protected TokenResponseGeneratorTests()
    {
        clock.SetUtcNow(FakeNow);
    }

    protected Open.IdentityServer.ResponseHandling.TokenResponseGenerator CreateSut() =>
        new(clock, tokenService, refreshTokenService, scopeParser, resources, clients, logger);
    
    protected class CapturedTokenCreationRequest
    {
        public TokenCreationRequest? Value { get; set; }
        public Token? TokenValue { get; set; }
    }

    protected CapturedTokenCreationRequest SetupTokenService(string tokenString = "access_token", Token? token = null)
    {
        token ??= new Token { Lifetime = 3600 };
        CapturedTokenCreationRequest capturedRequest = new CapturedTokenCreationRequest();
        Mock.Get(tokenService)
            .Setup(x => x.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync((TokenCreationRequest request) =>
            {
                capturedRequest.Value = request;
                return token;
            });
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
            .ReturnsAsync((Token tokenUsed) =>
            {
                capturedRequest.TokenValue = tokenUsed;
                return tokenString;
            });

        return capturedRequest;
    }

    protected void SetupTokenServiceWithIdToken(string accessTokenString = "access_token",
        string idTokenString = "id_token_jwt")
    {
        var accessToken = new Token { Lifetime = 3600 };
        var idToken = new Token();

        Mock.Get(tokenService)
            .Setup(x => x.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(accessToken);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(accessToken))
            .ReturnsAsync(accessTokenString);
        Mock.Get(tokenService)
            .Setup(x => x.CreateIdentityTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(idToken);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(idToken))
            .ReturnsAsync(idTokenString);
    }
    
    protected class CapturedRefreshTokenCreationRequest
    {
        public RefreshTokenCreationRequest? Value { get; set; }
        
        // Update Request
        public string? UpdatedHandle { get; set; }
        public RefreshToken? UpdatedRefreshToken { get; set; }
        public Client? UpdatedClient { get; set; }
    }

    protected CapturedRefreshTokenCreationRequest SetupRefreshTokenService(string handle = "refresh_token_handle")
    {
        CapturedRefreshTokenCreationRequest capturedRequest = new CapturedRefreshTokenCreationRequest();
        Mock.Get(refreshTokenService)
            .Setup(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenCreationRequest>()))
            .ReturnsAsync((RefreshTokenCreationRequest request) =>
            {
                capturedRequest.Value = request;
                return handle;
            });
        Mock.Get(refreshTokenService)
            .Setup(x => x.UpdateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<RefreshToken>(), It.IsAny<Client>()))
            .ReturnsAsync((string _, RefreshToken refreshToken, Client client) =>
            {
                capturedRequest.UpdatedHandle = handle;
                capturedRequest.UpdatedRefreshToken = refreshToken;
                capturedRequest.UpdatedClient = client;
                return handle;
            });

        return capturedRequest;
    }

    protected static ClaimsPrincipal CreateSubject(string subjectId = "123")
    {
        return new IdentityServerUser(subjectId).CreatePrincipal();
    }

    protected static ResourceValidationResult GenerateResourceValidationResult(string[] identResources,
        string[] apiResources, string[] apiScopes, bool offlineAccess = false)
    {
        return new ResourceValidationResult(new Resources
        {
            ApiResources = TestScopes.GetApis().Where(x => apiResources.Contains(x.Name)).ToArray(),
            ApiScopes = TestScopes.GetScopes().Where(x => apiScopes.Contains(x.Name)).ToArray(),
            IdentityResources = TestScopes.GetIdentity().Where(x => identResources.Contains(x.Name)).ToArray(),
            OfflineAccess = offlineAccess,
        });
    }
}