using System;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default.TokenRequestsValidator;

public class AuthCodeTokenValidationRequestsTests
{
    private IAuthorizationCodeStore authorizationCodeStore = Mock.Of<IAuthorizationCodeStore>();
    private ITokenRevocationService tokenRevocationService = Mock.Of<ITokenRevocationService>();
    private ITelemetryService telemetry = Mock.Of<ITelemetryService>();

    private static IClientStore Clients { get; } = Factory.CreateClientStore();
    
    private static NameValueCollection AuthCodeGrantParameters(
        string code = "fake-code", string clientId = "client-id", string redirectUri = "https://some.site.com/oidc-login") =>
        new()
        {
            [OidcConstants.TokenRequest.GrantType] = OidcConstants.GrantTypes.AuthorizationCode,
            [OidcConstants.TokenRequest.Code] = code,
            [OidcConstants.TokenRequest.ClientId] = clientId,
            [OidcConstants.TokenRequest.RedirectUri] = redirectUri,
        };

    private readonly FakeTimeProvider _fakeTimeProvider = new();
    private readonly DateTime _fakeNow = new(2026, 1, 1, 12, 30, 0, DateTimeKind.Utc);

    public AuthCodeTokenValidationRequestsTests()
    {
        _fakeTimeProvider.SetUtcNow(_fakeNow);
    }

    private TokenRequestValidator CreateSut()
    {
        return Factory.CreateTokenRequestValidator(
            telemetry: telemetry, 
            authorizationCodeStore: authorizationCodeStore, 
            tokenRevocationService: tokenRevocationService, 
            timeProvider: _fakeTimeProvider);
    }
    
    [Fact]
    public async Task AuthCodeGrant_WhenCodeUsed_ShouldMarkAsConsumed()
    {
        const string fakeAuthCode = "fakeAuthCode";
        const string codeClientId = "codeclient";
        const string redirectUri = "https://some.url.com/sign-in";

        AuthorizationCode fakeAuthCodeGrant = CreateFakeAuthCode(codeClientId, "bob", redirectUri, ["fakeScopeA"]);

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.GetAuthorizationCodeAsync(fakeAuthCode))
            .ReturnsAsync(fakeAuthCodeGrant);

        TokenRequestValidator subject = CreateSut();
        var client = await Clients.FindEnabledClientByIdAsync(codeClientId);

        var result = await subject.ValidateRequestAsync(
            AuthCodeGrantParameters(code: fakeAuthCode, clientId: codeClientId, redirectUri),
            client.ToValidationResult());

        result.IsError.Should().BeFalse();
        
        Mock.Get(authorizationCodeStore)
            .Verify(x => x.UpdateAuthorizationCodeAsync(fakeAuthCode, It.Is<AuthorizationCode>(ac => ac.ConsumedTime == _fakeNow)));
    }
    
    [Fact]  
    public async Task AuthCodeGrant_WhenCodeReuse_ShouldFailRevokingAnyIssuedTokensForCode()
    {
        const string fakeAuthCode = "fakeAuthCode";
        const string codeClientId = "codeclient";
        const string redirectUri = "https://some.url.com/sign-in";

        AuthorizationCode fakeAuthCodeGrant = CreateFakeAuthCode(codeClientId, "bob", redirectUri, ["fakeScopeA"]);

        //Mark AuthCode as Used
        fakeAuthCodeGrant.ConsumedTime = _fakeNow.AddDays(-1);

        Mock.Get(authorizationCodeStore)
            .Setup(x => x.GetAuthorizationCodeAsync(fakeAuthCode))
            .ReturnsAsync(fakeAuthCodeGrant);

        TokenRequestValidator subject = CreateSut();
        var client = await Clients.FindEnabledClientByIdAsync(codeClientId);

        var result = await subject.ValidateRequestAsync(
            AuthCodeGrantParameters(code: fakeAuthCode, clientId: codeClientId, redirectUri),
            client.ToValidationResult());
        
        result.IsError.Should().BeTrue();
        
        Mock.Get(tokenRevocationService)
            .Verify(x => x.RevokeGrantFamily(fakeAuthCodeGrant));
    }

    private AuthorizationCode CreateFakeAuthCode(string clientId, string subjectId, string redirectUri, string[] requestedScopes)
    {
        ClaimsPrincipal principal = new(new ClaimsIdentity([new Claim(JwtClaimTypes.Subject, subjectId)], "auth-type"));
        
        return new AuthorizationCode
        {
            CreationTime = _fakeNow.AddSeconds(-10),
            Lifetime = 60,
            ClientId = clientId,
            Subject = principal,
            IsOpenId = false,
            RequestedScopes = requestedScopes,
            RedirectUri = redirectUri,
            Nonce = null,
            StateHash = null,
            WasConsentShown = false,
            SessionId = null,
            CodeChallenge = null,
            CodeChallengeMethod = null,
            Description = null,
            Properties = null,
            RequestedResourceIndicators = null,
        };
    }
}