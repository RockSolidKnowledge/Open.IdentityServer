using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Services.Default;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Test.Utilities;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultDPoPProofValidatorTests
{
    private IHttpContextAccessor contextAccessor = Mock.Of<IHttpContextAccessor>();
    private IReplayCache replayCache = Mock.Of<IReplayCache>();
    private IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private MockDataProtector dataProtector = new();
    private FakeTimeProvider timeProvider = new();
    private MockLogger<DefaultDPoPProofValidator> logger = new();

    private IdentityServerOptions fakeOptions = new();

    private HttpContext fakeContext = new DefaultHttpContext();
    
    private DPoPProofGenerator proofGenerator;

    private DateTimeOffset fakeNow = new(2026, 01, 01, 12, 30, 0, TimeSpan.Zero);
    private Client fakeClient = new();
    
    private DefaultDPoPProofValidator CreateSut() => new(contextAccessor, replayCache, dataProtectionProvider, fakeOptions, logger);

    public DefaultDPoPProofValidatorTests()
    {
        proofGenerator = new DPoPProofGenerator(timeProvider);
        timeProvider.SetUtcNow(fakeNow);
        
        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector(DataProtectionConstants.DPoPNoncePurpose))
            .Returns(dataProtector);
        
        Mock.Get(contextAccessor)
            .SetupGet(x => x.HttpContext)
            .Returns(fakeContext);
    }

    [Fact]
    public async Task ValidateProof_WhenHttpHeaderMissing_ShouldReturnTrue_WithNullThumbprint()
    {
        SetupClient();
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeTrue();
        actual.Thumbprint.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ValidateProof_WhenHttpHeaderMissing_AndRequired_ShouldReturnFalse_WithInvalidDPoPProofError()
    {
        SetupClient(requireDpop: true);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }

    [Fact]
    public async Task ValidateProof_WhenMultipleHttpHeaders_ShouldReturnFalse_WithInvalidDPoPProofError()
    {
        SetupClient(requireDpop: true);

        fakeContext.Request.Headers[OidcConstants.HttpHeaders.DPoP] = new StringValues([
            "fake_header_a", "fake_header_b",
        ]);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndNonceMissingWhenRequired_ShouldFail_WithUseNonce_AndSetDPoPNonceHeader()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: key);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Nonce);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.UseDPoPNonce);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndInvalidNoncePresent_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        DateTimeOffset iat = fakeNow.AddSeconds(10);
        string testNonce = dataProtector.GenerateFakeProtectedData(fakeNow.AddMinutes(30).ToUnixTimeSeconds().ToString());
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: key, iat: iat, nonce: testNonce);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Nonce);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndNonceUnprotectFails_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        DateTimeOffset iat = fakeNow.AddSeconds(10);
        string testNonce = "invalid_nonce";
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: key, iat: iat, nonce: testNonce);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Nonce);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndNonceUnprotectThrows_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        DateTimeOffset iat = fakeNow.AddSeconds(10);
        string testNonce = "invalid_nonce";

        Mock.Get(dataProtector.dataProtector)
            .Setup(x => x.Unprotect(It.IsAny<byte[]>()))
            .Throws(new Exception("FAKE UNPROTECT EXCEPTION"));
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: key, iat: iat, nonce: testNonce);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Nonce);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndValidationModeIat_WhenIatTooOld_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        DateTimeOffset iat = fakeNow.AddHours(1);
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: key, iat: iat);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndJtiReplayDetected_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);

        //TODO: Update IReplay Cache to Support Atomic Operations
        Mock.Get(replayCache)
            .Setup(x => x.ExistsAsync(IdentityServerConstants.ReplayCachePurposes.DPoPJti, testJti))
            .ReturnsAsync(true);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: testKey, iat: testIat, jti: testJti);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndSignatureInvalid_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var otherTestKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: testKey, headerKey: otherTestKey, iat: testIat, jti: testJti);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Theory]
    [InlineData(nameof(HttpMethod.Get))]
    [InlineData(nameof(HttpMethod.Put))]
    [InlineData("invalid_method")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndHtmInvalid_ShouldFail_WithInvalidDPoPProof(string htm)
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, htm, signingKey: testKey, iat: testIat, jti: testJti);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Theory]
    [InlineData("https://open.identityserver.com/TOKEN")]
    [InlineData("https://open.identityserver.com/authorise")]
    [InlineData("https://samle.site.com/token")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndHtuInvalid_ShouldFail_WithInvalidDPoPProof(string htu)
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(htu, method, signingKey: testKey, iat: testIat, jti: testJti);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndKeyInHeaderContainsPrivateKey_ShouldFail()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: testKey, iat: testIat, jti: testJti, includeWholeKey: true);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndValidatedAccessTokenPresent_WithMissingAth_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";
        const string fakeAccessTokenJwt = "hjfahfdalkhfkfjhskldjfsdf.fsksjdfhjksdhfhdsfkhsdfkjhsjkfhsdkjf.sdfghksdjfhjsdhkfjhdsfkjhsd";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: testKey, iat: testIat, jti: testJti);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);

        TokenValidationResult tokenValidationResult = new TokenValidationResult
        {
            Jwt = fakeAccessTokenJwt,
            Claims = [ new Claim("cnf", $"{{\"jtk\":\"{DPoPProofGenerator.Thumprint(testKey)}\"}}") ],
        };
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient, tokenValidationResult);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndValidatedAccessTokenPresent_WithAthMistmatch_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";
        const string fakeAccessTokenJwt = "hjfahfdalkhfkfjhskldjfsdf.fsksjdfhjksdhfhdsfkhsdfkjhsjkfhsdkjf.sdfghksdjfhjsdhkfjhdsfkjhsd";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: testKey, iat: testIat, jti: testJti, accessToken: "incorrect.access.token");
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);

        TokenValidationResult tokenValidationResult = new TokenValidationResult
        {
            Jwt = fakeAccessTokenJwt,
            Claims = [ new Claim("cnf", $"{{\"jtk\":\"{DPoPProofGenerator.Thumprint(testKey)}\"}}") ],
        };
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient, tokenValidationResult);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndValidatedAccessTokenPresent_WithThumbprintMismatch_ShouldFail_WithInvalidDPoPProof()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";
        const string fakeAccessTokenJwt = "hjfahfdalkhfkfjhskldjfsdf.fsksjdfhjksdhfhdsfkhsdfkjhsjkfhsdkjf.sdfghksdjfhjsdhkfjhdsfkjhsd";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: testKey, iat: testIat, jti: testJti, accessToken: fakeAccessTokenJwt);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);

        TokenValidationResult tokenValidationResult = new TokenValidationResult
        {
            Jwt = fakeAccessTokenJwt,
            Claims = [ new Claim("cnf", $"{{\"jtk\":\"FAKETHUMBPRINT\"}}") ],
        };
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient, tokenValidationResult);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeFalse();
        actual.ValidationError.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndValidatedAccessTokenPresent_WhenAthValidAndThumbprintsMatch_ShouldPass_ReturningKeyThumbprint()
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";
        const string fakeAccessTokenJwt = "hjfahfdalkhfkfjhskldjfsdf.fsksjdfhjksdhfhdsfkhsdfkjhsjkfhsdkjf.sdfghksdjfhjsdhkfjhdsfkjhsd";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(requestUrl, method, signingKey: testKey, iat: testIat, jti: testJti, accessToken: fakeAccessTokenJwt);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);

        TokenValidationResult tokenValidationResult = new TokenValidationResult
        {
            Jwt = fakeAccessTokenJwt,
            Claims = [ new Claim("cnf", $"{{\"jtk\":\"{DPoPProofGenerator.Thumprint(testKey)}\"}}") ],
        };
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient, tokenValidationResult);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeTrue();
        actual.Thumbprint.Should().Be(DPoPProofGenerator.Thumprint(testKey));
    }
    
    [Theory]
    [InlineData(nameof(HttpMethod.Post), "https://open.identityserver.com/token")]
    [InlineData("post", "https://open.identityserver.com/token")]
    [InlineData("Post", "https://open.identityserver.com/token?param=val")]
    [InlineData(nameof(HttpMethod.Post), "https://open.identityserver.com/token#frag")]
    public async Task ValidateProof_WhenHttpHeaderPresent_AndIsValid_ShouldPass_WithThumbprintReturned(string htm, string htu)
    {
        const string method = nameof(HttpMethod.Post);
        const string requestUrl = "https://open.identityserver.com/token";

        string testJti = Guid.NewGuid().ToString();
        DateTimeOffset testIat = fakeNow.AddSeconds(10);
        
        var testKey = DPoPProofGenerator.CreateDPoPKey();
        var dpopProof = proofGenerator.CreateDPoPProof(htu, htm, signingKey: testKey, iat: testIat, jti: testJti);
        
        SetupClient(requireDpop: true, validationMode: DPoPTokenExpirationValidationMode.Iat);
        SetupHttpRequest(method, new Uri(requestUrl), dpopProof);
        
        var sut = CreateSut();

        var actual = await sut.ValidateProof(fakeClient);
        
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeTrue();
        actual.Thumbprint.Should().Be(DPoPProofGenerator.Thumprint(testKey));
    }

    private void SetupClient(bool requireDpop = false, DPoPTokenExpirationValidationMode validationMode = DPoPTokenExpirationValidationMode.Iat)
    {
        fakeClient.RequireDPoP = requireDpop;
        fakeClient.DPoPValidationMode = validationMode;
    }

    private void SetupHttpRequest(string method = "GET", Uri url = null, string dpopProof = null)
    {
        var uri = url ?? new Uri("https://open.identityserver.com/authorise");

        fakeContext.Request.Method = method;
        fakeContext.Request.Scheme = uri.Scheme;
        fakeContext.Request.Host = new HostString(uri.Host);
        fakeContext.Request.QueryString = new QueryString(uri.Query);

        fakeContext.Request.Headers[OidcConstants.HttpHeaders.DPoP] = dpopProof;
    }
}