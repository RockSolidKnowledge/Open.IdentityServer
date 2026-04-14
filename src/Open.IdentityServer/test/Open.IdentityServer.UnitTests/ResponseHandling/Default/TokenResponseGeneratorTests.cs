using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.Default;

public class TokenResponseGeneratorTests
{
    private FakeTimeProvider clock = new();
    private ITokenService tokenService = Mock.Of<ITokenService>();
    private IRefreshTokenService refreshTokenService = Mock.Of<IRefreshTokenService>();
    private IScopeParser scopeParser = Mock.Of<IScopeParser>();
    private IResourceStore resources = Mock.Of<IResourceStore>();
    private IClientStore clients = Mock.Of<IClientStore>();
    private ILogger<TokenResponseGenerator> logger = NullLogger<TokenResponseGenerator>.Instance;

    private DateTime FakeNow = new DateTime(2026, 02, 01, 12, 23, 00);
    
    public TokenResponseGeneratorTests()
    {
        clock.SetUtcNow(FakeNow);
    }
    
    private TokenResponseGenerator CreateSut() => 
        new(clock, tokenService, refreshTokenService, scopeParser, resources, clients, logger);

    [Fact]
    public async Task ProcessAsync_WhenClientUpdateClaimsOnRefreshIsFalse_ShouldStillUpdateJti()
    {
        var originalJti = CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex);
        var originalAccessToken = new Token
        {
            CreationTime  = FakeNow.AddDays(-10),
            Lifetime = 3000,
            ClientId = "fake-cli",
            AccessTokenType = AccessTokenType.Jwt,
            Claims = [
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
                    {string.Empty, originalAccessToken}
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

        updatedToken.CreationTime.Should().BeCloseTo(FakeNow, TimeSpan.FromSeconds(1));
        updatedToken.Lifetime.Should().Be(fakeRefreshTokenAuthRequest.AccessTokenLifetime);
        updatedToken.Claims.Should().Contain(x => x.Type == JwtClaimTypes.JwtId);
        updatedToken.Claims.Should().NotContain(x => x.Type == JwtClaimTypes.JwtId && x.Value == originalJti);
    }
}