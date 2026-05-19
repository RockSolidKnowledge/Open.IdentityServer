// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Utility;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public class TokenResponseGeneratorTests_RefreshToken : TokenResponseGeneratorTests
{
    [Fact]
    public async Task ProcessAsync_RefreshToken_UpdateClaimsOnRefreshTrue_CreatesNewAccessToken()
    {
        var capturedRefreshTokenRequest = SetupRefreshTokenService("new_refresh");
        var newAccessToken = new Token { Lifetime = 2500 };
        var capturedTokenRequest = SetupTokenService("new_access_token", newAccessToken);
        
        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 2500,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = true },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource"],
                AccessTokens = new Dictionary<string, Token>
                {
                    [string.Empty] = new()
                    {
                        Claims = [
                            new Claim("scope", "resource"),
                        ],
                    },
                }
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("new_access_token");
        response.RefreshToken.Should().Be("new_refresh");

        capturedTokenRequest.Value.Should().NotBeNull();
        capturedTokenRequest.Value!.Subject.Should().BeEquivalentTo(request.RefreshToken.Subject);
        capturedTokenRequest.Value.ValidatedResources.Resources.ApiResources
            .Should().ContainSingle(x => x.Name == "api")
            .And.HaveCount(1);
        capturedTokenRequest.Value.ValidatedResources.RawScopeValues.Should().BeEquivalentTo("resource");
        capturedTokenRequest.Value.ResourceIndicatorsUsed.Should().BeFalse();

        capturedRefreshTokenRequest.UpdatedRefreshToken.Should().NotBeNull();
        capturedRefreshTokenRequest.UpdatedRefreshToken!.AccessTokens.Should().ContainKey(string.Empty)
            .WhoseValue.Should().Be(newAccessToken);
    }

    [Fact]
    public async Task ProcessAsync_RefreshToken_UpdateClaimsOnRefreshFalse_ReusesOldToken()
    {
        var capturedRefreshTokenRequest = SetupRefreshTokenService("new_refresh");

        var originalAccessToken = new Token
        {
            CreationTime = FakeNow.AddDays(-10),
            Lifetime = 3000,
            ClientId = "client",
            AccessTokenType = AccessTokenType.Jwt,
            Claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, "original_jti")
            }
        };

        Token usedToken = null;
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
            .ReturnsAsync((Token token) =>
            {
                usedToken = token;
                return "reused_token";
            });

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 2500,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = false },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource"],
                AccessTokens = new Dictionary<string, Token>
                {
                    { string.Empty, originalAccessToken }
                }
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("reused_token");
        response.AccessTokenLifetime.Should().Be(2500);

        Mock.Get(tokenService)
            .Verify(x => x.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()), Times.Never);

        usedToken.Should().BeEquivalentTo(originalAccessToken, cnf =>
            cnf.Excluding(x => x.CreationTime)
                .Excluding(x => x.Lifetime)
                .Excluding(x => x.Claims));
    }
    
    [Fact]
    public async Task ProcessAsync_RefreshToken_WithResourcesIndicated_UpdateClaimsOnRefreshFalse_ReusesOldToken()
    {
        var capturedRefreshTokenRequest = SetupRefreshTokenService("new_refresh");

        var originalAccessToken = new Token
        {
            CreationTime = FakeNow.AddDays(-10),
            Lifetime = 3000,
            ClientId = "client",
            AccessTokenType = AccessTokenType.Jwt,
            Claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, "original_jti")
            }
        };

        Token usedToken = null;
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
            .ReturnsAsync((Token token) =>
            {
                usedToken = token;
                return "reused_token";
            });

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 2500,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = false },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["urn:valid.resource:Read", "valid:Read", "valid:Write"],
                AuthorizedResourceIndicators = ["urn:valid.resource", "https://valid.resource.com"],
                AccessTokens = new Dictionary<string, Token>
                {
                    { "urn:valid.resource", originalAccessToken }
                }
            },
            RequestedResourceIndicator = "urn:valid.resource",
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("reused_token");
        response.AccessTokenLifetime.Should().Be(2500);
        response.Scope.Should().BeEquivalentTo("urn:valid.resource:Read");

        Mock.Get(tokenService)
            .Verify(x => x.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()), Times.Never);

        usedToken.Should().BeEquivalentTo(originalAccessToken, cnf =>
            cnf.Excluding(x => x.CreationTime)
                .Excluding(x => x.Lifetime)
                .Excluding(x => x.Claims));

        capturedRefreshTokenRequest.UpdatedRefreshToken.Should().NotBeNull();
        capturedRefreshTokenRequest.UpdatedRefreshToken!.AccessTokens.Should().ContainKey("urn:valid.resource")
            .WhoseValue.Should().Be(request.RefreshToken.AccessTokens["urn:valid.resource"]);
    }
    
    [Fact]
    public async Task ProcessAsync_RefreshToken_WithResourcesIndicated_UpdateClaimsOnRefreshFalse_ButResourceIndicatorInTokenRequestDifferent_CreateNewAccessToken()
    {
        var capturedRefreshTokenRequest = SetupRefreshTokenService("new_refresh");

        var newAccessToken = new Token
        {
            CreationTime = FakeNow,
            Lifetime = 2500,
            ClientId = "client",
            AccessTokenType = AccessTokenType.Jwt,
            Claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, "original_jti")
            }
        };
        var capturedTokenRequest = SetupTokenService("new_access_token", newAccessToken);

        var originalAccessToken = new Token
        {
            CreationTime = FakeNow.AddDays(-10),
            Lifetime = 3000,
            ClientId = "client",
            AccessTokenType = AccessTokenType.Jwt,
            Claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, "original_jti")
            }
        };

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 2500,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = false },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["urn:valid.resource:Read", "valid:Read", "valid:Write"],
                AuthorizedResourceIndicators = ["urn:valid.resource", "https://valid.resource.com"],
                AccessTokens = new Dictionary<string, Token>
                {
                    { "urn:valid.resource", originalAccessToken }
                }
            },
            RequestedResourceIndicator = "https://valid.resource.com",
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("new_access_token");
        response.AccessTokenLifetime.Should().Be(2500);
        response.Scope.Should().BeEquivalentTo("valid:Read valid:Write");

        capturedTokenRequest.Value.Should().NotBeNull();
        capturedTokenRequest.Value!.Subject.Should().BeEquivalentTo(request.RefreshToken.Subject);
        capturedTokenRequest.Value.ValidatedResources.Resources.ApiResources
            .Should().ContainSingle(x => x.Name == "https://valid.resource.com")
            .And.HaveCount(1);
        capturedTokenRequest.Value.ValidatedResources.RawScopeValues.Should().BeEquivalentTo("valid:Read", "valid:Write");

        capturedTokenRequest.TokenValue.Should().BeEquivalentTo(newAccessToken);
        capturedTokenRequest.Value.ResourceIndicatorsUsed.Should().BeTrue();

        capturedRefreshTokenRequest.UpdatedRefreshToken.Should().NotBeNull();
        capturedRefreshTokenRequest.UpdatedRefreshToken!.AccessTokens.Should().ContainKey("urn:valid.resource")
            .WhoseValue.Should().Be(originalAccessToken);
        capturedRefreshTokenRequest.UpdatedRefreshToken!.AccessTokens.Should().ContainKey("https://valid.resource.com")
            .WhoseValue.Should().Be(newAccessToken);
    }
    
    [Fact]
    public async Task ProcessAsync_RefreshToken_WithResourcesIndicated_UpdateClaimsOnRefreshFalse_ButResourceIndicatorNotSet_CreateNewAccessToken_WithRefreshTokenScope()
    {
        var capturedRefreshTokenRequest = SetupRefreshTokenService("new_refresh");

        var newAccessToken = new Token
        {
            CreationTime = FakeNow,
            Lifetime = 2500,
            ClientId = "client",
            AccessTokenType = AccessTokenType.Jwt,
            Claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, "original_jti")
            }
        };
        var capturedTokenRequest = SetupTokenService("new_access_token", newAccessToken);

        var originalAccessToken = new Token
        {
            CreationTime = FakeNow.AddDays(-10),
            Lifetime = 3000,
            ClientId = "client",
            AccessTokenType = AccessTokenType.Jwt,
            Claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, "original_jti")
            }
        };

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 2500,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = false },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["urn:valid.resource:Read", "valid:Read", "valid:Write", "resource"],
                AuthorizedResourceIndicators = ["urn:valid.resource", "https://valid.resource.com"],
                AccessTokens = new Dictionary<string, Token>
                {
                    { "urn:valid.resource", originalAccessToken }
                }
            },
            RequestedResourceIndicator = null,
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("new_access_token");
        response.AccessTokenLifetime.Should().Be(2500);
        response.Scope.Should().BeEquivalentTo("urn:valid.resource:Read valid:Read valid:Write resource");

        capturedTokenRequest.Value.Should().NotBeNull();
        capturedTokenRequest.Value!.Subject.Should().BeEquivalentTo(request.RefreshToken.Subject);
        capturedTokenRequest.Value.ValidatedResources.Resources.ApiResources
            .Should().ContainSingle(x => x.Name == "urn:valid.resource")
            .And.ContainSingle(x => x.Name == "https://valid.resource.com")
            .And.ContainSingle(x => x.Name == "api")
            .And.HaveCount(3);
        capturedTokenRequest.Value.ValidatedResources.RawScopeValues.Should().BeEquivalentTo("urn:valid.resource:Read", "valid:Read", "valid:Write", "resource");
        capturedTokenRequest.TokenValue.Should().BeEquivalentTo(newAccessToken);
        capturedTokenRequest.Value.ResourceIndicatorsUsed.Should().BeFalse();

        capturedRefreshTokenRequest.UpdatedRefreshToken.Should().NotBeNull();
        capturedRefreshTokenRequest.UpdatedRefreshToken!.AccessTokens.Should().ContainKey("urn:valid.resource")
            .WhoseValue.Should().Be(originalAccessToken);
        capturedRefreshTokenRequest.UpdatedRefreshToken!.AccessTokens.Should().ContainKey(string.Empty)
            .WhoseValue.Should().Be(newAccessToken);
    }

    [Fact]
    public async Task ProcessAsync_RefreshToken_WithOpenId_ReturnsIdToken()
    {
        SetupRefreshTokenService("new_refresh");

        var accessToken = new Token
        {
            Lifetime = 3600,
            Claims =
            [
                new Claim("scope", "openid"),
                new Claim("scope", "resource"),
            ],
        };
        var idToken = new Token();

        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
            .ReturnsAsync("reused_access");
        Mock.Get(tokenService)
            .Setup(x => x.CreateIdentityTokenAsync(It.IsAny<TokenCreationRequest>()))
            .ReturnsAsync(idToken);
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(idToken))
            .ReturnsAsync("refresh_id_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 3600,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = false },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = [OidcConstants.StandardScopes.OpenId, "resource"],
                AccessTokens = new Dictionary<string, Token>
                {
                    { string.Empty, accessToken }
                }
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.IdentityToken.Should().Be("refresh_id_token");
    }

    [Fact]
    public async Task ProcessAsync_RefreshToken_WithoutOpenId_DoesNotReturnIdToken()
    {
        SetupRefreshTokenService("new_refresh");

        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
            .ReturnsAsync("reused_access");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 3600,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = false },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource"],
                AccessTokens = new Dictionary<string, Token>
                {
                    [string.Empty] = new()
                    {
                        Claims = [
                            new Claim("scope", "resource"),
                        ],
                    },
                }
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.IdentityToken.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_RefreshToken_ScopeFromAuthorizedScopes()
    {
        SetupRefreshTokenService("new_refresh");

        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
            .ReturnsAsync("token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 3600,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = false },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource", "resource2"],
                AccessTokens = new Dictionary<string, Token>
                {
                    [string.Empty] = new()
                    {
                        Claims = [
                            new Claim("scope", "resource"),
                        ],
                    },
                }
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.Scope.Should().Be("resource resource2");
    }

    [Fact]
    public async Task ProcessAsync_WhenClientUpdateClaimsOnRefreshIsFalse_ShouldStillUpdateJti()
    {
        var originalJti = CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex);
        var originalAccessToken = new Token
        {
            CreationTime = FakeNow.AddDays(-10),
            Lifetime = 3000,
            ClientId = "fake-cli",
            AccessTokenType = AccessTokenType.Jwt,
            Claims =
            [
                new Claim(JwtClaimTypes.JwtId, originalJti),
            ],
        };

        var fakeRefreshTokenAuthRequest = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 2500,
            RefreshToken = new RefreshToken
            {
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                ClientId = originalAccessToken.ClientId,
                SessionId = originalAccessToken.SessionId,
                AuthorizedScopes = originalAccessToken.Scopes,
                AccessTokens = new Dictionary<string, Token>
                {
                    { string.Empty, originalAccessToken }
                },
            },
            Client = new Client
            {
                UpdateAccessTokenClaimsOnRefresh = false,
            },
        };

        var fakeTokenString = "some.token.string";
        Token? updatedToken = null;
        Mock.Get(tokenService)
            .Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
            .ReturnsAsync((Token token) =>
            {
                updatedToken = token;
                return fakeTokenString;
            });

        var fakeNewHandle = "newHandle";
        Mock.Get(refreshTokenService)
            .Setup(x => x.UpdateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<RefreshToken>(), It.IsAny<Client>()))
            .ReturnsAsync(fakeNewHandle);

        var fakeTokenRequestValidationResult = new TokenRequestValidationResult(fakeRefreshTokenAuthRequest);
        var sut = CreateSut();

        var actual = await sut.ProcessAsync(fakeTokenRequestValidationResult);

        actual.AccessToken.Should().Be(fakeTokenString);
        actual.AccessTokenLifetime.Should().Be(fakeRefreshTokenAuthRequest.AccessTokenLifetime);
        actual.RefreshToken.Should().Be(fakeNewHandle);

        updatedToken.Should().NotBeNull();
        updatedToken!.CreationTime.Should().BeCloseTo(FakeNow, TimeSpan.FromSeconds(1));
        updatedToken.Lifetime.Should().Be(fakeRefreshTokenAuthRequest.AccessTokenLifetime);
        updatedToken.Claims.Should().Contain(x => x.Type == JwtClaimTypes.JwtId);
        updatedToken.Claims.Should().NotContain(x => x.Type == JwtClaimTypes.JwtId && x.Value == originalJti);
    }
}