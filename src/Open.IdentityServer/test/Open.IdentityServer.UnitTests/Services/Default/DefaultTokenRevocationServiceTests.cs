using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services.Default;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultTokenRevocationServiceTests
{
    private IAuthorizationCodeStore authorizationCodeStore = Mock.Of<IAuthorizationCodeStore>();
    private IReferenceTokenStore referenceTokenStore = Mock.Of<IReferenceTokenStore>();
    private IRefreshTokenStore refreshTokenStore = Mock.Of<IRefreshTokenStore>();
    private IPersistedGrantStore persistedGrantStore = Mock.Of<IPersistedGrantStore>();
    
    private DefaultTokenRevocationService CreateSut() => new(authorizationCodeStore, referenceTokenStore, refreshTokenStore, persistedGrantStore);

    [Fact]
    public async Task RevokeGrantFamily_WhenChildGrantNotSet_ShouldOnlyRemoveAuthCode()
    {
        const string code = "auth-code";
        AuthorizationCode authorizationCode = new AuthorizationCode
        {
            ChildGrant = null,
        };

        IEnumerable<string> keysRemoved = null;
        Mock.Get(persistedGrantStore)
            .Setup(x => x.RemoveAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(keys => keysRemoved = keys);
        
        var sut = CreateSut();

        await sut.RevokeGrantFamily(code, authorizationCode);

        keysRemoved.Should().BeEquivalentTo(code);
    }
    
    [Fact]
    public async Task RevokeGrantFamily_WhenChildGrantSetToReferenceToken_ShouldRevokeReferenceToken()
    {
        const string code = "auth-code";
        const string referenceToken = "ref-token";
        AuthorizationCode authorizationCode = new AuthorizationCode
        {
            ChildGrant = referenceToken,
        };

        IEnumerable<string> keysRemoved = null;
        Mock.Get(persistedGrantStore)
            .Setup(x => x.RemoveAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(keys => keysRemoved = keys);
        
        var sut = CreateSut();

        await sut.RevokeGrantFamily(code, authorizationCode);

        keysRemoved.Should().BeEquivalentTo(referenceToken, code);
    }
    
    [Fact]
    public async Task RevokeGrantFamily_WhenChildGrantSetToRefreshToken_ShouldRevokeRefreshToken()
    {
        const string code = "auth-code";
        const string refreshToken = "refresh-token";
        AuthorizationCode authorizationCode = new AuthorizationCode
        {
            ChildGrant = refreshToken,
        };
        RefreshToken refreshTokenGrant = new RefreshToken
        {
            ChildGrant = null,
            AccessTokens = new Dictionary<string, Token>(),
        };

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken))
            .ReturnsAsync(refreshTokenGrant);

        IEnumerable<string> keysRemoved = null;
        Mock.Get(persistedGrantStore)
            .Setup(x => x.RemoveAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(keys => keysRemoved = keys);
        
        var sut = CreateSut();

        await sut.RevokeGrantFamily(code, authorizationCode);

        keysRemoved.Should().BeEquivalentTo(refreshToken, code);
    }
    
    [Fact]
    public async Task RevokeGrantFamily_WhenChildGrantSetToRefreshToken_AndReferenceTokensIssued_ShouldRevokeRefreshToken_AndReferenceTokens()
    {
        const string code = "auth-code";
        const string refreshToken = "refresh-token";
        const string refToken = "toke-b";
        AuthorizationCode authorizationCode = new AuthorizationCode
        {
            ChildGrant = refreshToken,
        };
        RefreshToken refreshTokenGrant = new RefreshToken
        {
            ChildGrant = null,
            AccessTokens = new Dictionary<string, Token>
            {
                ["toke-a"] = new() { AccessTokenType = AccessTokenType.Jwt },
                [refToken] = new() { AccessTokenType = AccessTokenType.Reference },
            },
        };

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken))
            .ReturnsAsync(refreshTokenGrant);

        IEnumerable<string> keysRemoved = null;
        Mock.Get(persistedGrantStore)
            .Setup(x => x.RemoveAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(keys => keysRemoved = keys);
        
        var sut = CreateSut();

        await sut.RevokeGrantFamily(code, authorizationCode);

        keysRemoved.Should().BeEquivalentTo(refreshToken, refToken, code);
    }
    
    [Fact]
    public async Task RevokeGrantFamily_WhenChildGrantSetToRefreshToken_AndHasMultipleChildRefreshTokens_ShouldRevokeAllRefreshToken()
    {
        const string code = "auth-code";
        const string refreshToken1 = "refresh-token-1";
        const string refreshToken2 = "refresh-token-2";
        const string refreshToken3 = "refresh-token-3";
        AuthorizationCode authorizationCode = new AuthorizationCode
        {
            ChildGrant = refreshToken1,
        };
        RefreshToken refreshTokenGrant1 = new RefreshToken
        {
            ChildGrant = refreshToken2,
            AccessTokens = new Dictionary<string, Token>
            {
                ["toke-a"] = new() { AccessTokenType = AccessTokenType.Jwt },
            },
        };
        RefreshToken refreshTokenGrant2 = new RefreshToken
        {
            ChildGrant = refreshToken3,
            AccessTokens = new Dictionary<string, Token>
            {
                ["toke-b"] = new() { AccessTokenType = AccessTokenType.Jwt },
            },
        };
        RefreshToken refreshTokenGrant3 = new RefreshToken
        {
            ChildGrant = null,
            AccessTokens = new Dictionary<string, Token>
            {
                ["toke-c"] = new() { AccessTokenType = AccessTokenType.Jwt },
            },
        };

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken1))
            .ReturnsAsync(refreshTokenGrant1);

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken2))
            .ReturnsAsync(refreshTokenGrant2);

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken3))
            .ReturnsAsync(refreshTokenGrant3);

        IEnumerable<string> keysRemoved = null;
        Mock.Get(persistedGrantStore)
            .Setup(x => x.RemoveAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(keys => keysRemoved = keys);
        
        var sut = CreateSut();

        await sut.RevokeGrantFamily(code, authorizationCode);

        keysRemoved.Should().BeEquivalentTo(refreshToken1, refreshToken2, refreshToken3, code);
    }
    
    [Fact]
    public async Task RevokeGrantFamily_WhenChildGrantSetToRefreshToken_AndHasMultipleChildRefreshTokensAndReferenceTokens_ShouldRevokeAllRefreshToken()
    {
        const string code = "auth-code";
        
        const string refreshToken1 = "refresh-token-1";
        const string refreshToken2 = "refresh-token-2";
        const string refreshToken3 = "refresh-token-3";
        
        const string refTokeA = "ref-token-a";
        const string refTokeB = "ref-token-b";
        const string refTokeC = "ref-token-c";
        const string refTokeD = "ref-token-d";
        const string refTokeE = "ref-token-e";
        const string refTokeF = "ref-token-f";
        
        AuthorizationCode authorizationCode = new AuthorizationCode
        {
            ChildGrant = refreshToken1,
        };
        RefreshToken refreshTokenGrant1 = new RefreshToken
        {
            ChildGrant = refreshToken2,
            AccessTokens = new Dictionary<string, Token>
            {
                ["toke-a"] = new() { AccessTokenType = AccessTokenType.Jwt },
                [refTokeA] = new() { AccessTokenType = AccessTokenType.Reference },
                [refTokeB] = new() { AccessTokenType = AccessTokenType.Reference },
            },
        };
        RefreshToken refreshTokenGrant2 = new RefreshToken
        {
            ChildGrant = refreshToken3,
            AccessTokens = new Dictionary<string, Token>
            {
                [refTokeC] = new() { AccessTokenType = AccessTokenType.Reference },
                [refTokeE] = new() { AccessTokenType = AccessTokenType.Reference },
                ["toke-b"] = new() { AccessTokenType = AccessTokenType.Jwt },
                [refTokeD] = new() { AccessTokenType = AccessTokenType.Reference },
            },
        };
        RefreshToken refreshTokenGrant3 = new RefreshToken
        {
            ChildGrant = null,
            AccessTokens = new Dictionary<string, Token>
            {
                [refTokeF] = new() { AccessTokenType = AccessTokenType.Reference },
                ["toke-c"] = new() { AccessTokenType = AccessTokenType.Jwt },
            },
        };

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken1))
            .ReturnsAsync(refreshTokenGrant1);

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken2))
            .ReturnsAsync(refreshTokenGrant2);

        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(refreshToken3))
            .ReturnsAsync(refreshTokenGrant3);

        IEnumerable<string> keysRemoved = null;
        Mock.Get(persistedGrantStore)
            .Setup(x => x.RemoveAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(keys => keysRemoved = keys);
        
        var sut = CreateSut();

        await sut.RevokeGrantFamily(code, authorizationCode);
        
        keysRemoved.Should().BeEquivalentTo(
            refreshToken1, refreshToken2, refreshToken3,
            refTokeA, refTokeB, refTokeC, refTokeD, refTokeE, refTokeF,
            code);
    }
}