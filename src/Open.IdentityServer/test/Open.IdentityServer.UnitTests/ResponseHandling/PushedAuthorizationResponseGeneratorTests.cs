using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.ResponseHandling.Default;
using Open.IdentityServer.Services;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling;
#nullable enable

public class PushedAuthorizationResponseGeneratorTests
{
    private readonly Mock<IPushedAuthorizationRequestStore> _store = new();
    private readonly Mock<IHandleGenerationService> _handleGenerationService = new();
    private Mock<ILogger<PushedAuthorizationResponseGenerator>> _logger = new();
    private ValidatedAuthorizeRequest _request;

    public PushedAuthorizationResponseGeneratorTests()
    {
        _request = new ValidatedAuthorizeRequest
        {
            ClientId = "dfui",
            GrantType = "code",
            Description = "skdvibjsd",
            LoginHint = "jdsfbivufe",
            IsApiResourceRequest = true,
            IsOpenIdRequest = true,
            MaxAge = 100,
            Nonce = "jdsfbivufe",
            RedirectUri = "https://foo.bar",
            CodeChallenge = "kdsjfbvj",
            CodeChallengeMethod = "skjdehfoub",
            DisplayMode = "dfjbvjk",
            ResponseMode = "shjdbvhb",
            ResponseType = "skjdfhvb",
            State = "wrger",
            UiLocales = "skjdfhvb",
            RequestObject = "kjsbdvkbjj",
            WasConsentShown = true,
            AccessTokenLifetime = 200,
            Client = new(),
            Confirmation = "dsfifvbuo",
            SessionId = "djcbiud",
            Subject = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("sub", "123") })),
            AccessTokenType = AccessTokenType.Jwt,
            Secret = new ParsedSecret(),
            AuthenticationContextReferenceClasses = new List<string> { "hvsdvc", "bsdfu" },
            RequestedResourceIndicators = new List<string> { "efe", "ergberb" },
            RequestedScopes = new List<string> { "yhrehq", "asdgfrh" },
            PromptModes = new List<string> { "aerh", "gfrea" },
            ClientClaims = new List<Claim>{ new Claim("foo", "bar"), new Claim("baz", "qux") },
            Options = new IdentityServerOptions(),
            Raw = new NameValueCollection(),
            RequestObjectValues = new Dictionary<string, string> { ["jdbsv"] = "sdbvbudv" },
            ValidatedResources = new ResourceValidationResult
            {
                Resources = new Resources
                {
                    OfflineAccess = true,
                    ApiScopes = [new ApiScope("api")],
                }
            },
        };
    }
    
    
    private PushedAuthorizationResponseGenerator CreateSut()
    {
        return new PushedAuthorizationResponseGenerator(_store.Object, _handleGenerationService.Object, _logger.Object);
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldMapRequestCorrectlyAndSendToStore()
    {
        var sut = CreateSut();
        
        PushedAuthorizationStoredInformation? storedInfo = null;
        
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync(It.IsAny<string>(), It.IsAny<PushedAuthorizationStoredInformation>()))
            .Callback<string, PushedAuthorizationStoredInformation>((_, info) => storedInfo = info);
        
        await sut.CreateResponseAsync(_request);
        
        VerifyPushedAuthorizeRequestMapping(storedInfo, _request);
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldConstructUriCorrectlyAndPassToStore()
    {
        string generatedUniquePart = "sdufbsibdvibv";
        
        _handleGenerationService.Setup(g => g.GenerateAsync()).ReturnsAsync(generatedUniquePart);
        
        var sut = CreateSut();

        string? passedId = null;
        
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync(It.IsAny<string>(), It.IsAny<PushedAuthorizationStoredInformation>()))
            .Callback<string, PushedAuthorizationStoredInformation>((id,  _) => passedId = id);
        
        await sut.CreateResponseAsync(_request);
        
        passedId.Should().NotBeNull();
        passedId.Should().Be(PushedAuthorizationResponseGenerator.PushedAuthorizationRequestPrefix + generatedUniquePart);
    }

    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldGenerateResponseCorrectly()
    {
        string generatedUniquePart = "sdufbsibdvibv";
        string expectedUri =
            $"{PushedAuthorizationResponseGenerator.PushedAuthorizationRequestPrefix}{generatedUniquePart}";

        _handleGenerationService.Setup(g => g.GenerateAsync()).ReturnsAsync(generatedUniquePart);

        var sut = CreateSut();

        PushedAuthorizationResponse? response = await sut.CreateResponseAsync(_request);

        response.Should().NotBeNull();
        response.Uri.Should().Be(expectedUri);
        response.Lifetime.Should().Be(PushedAuthorizationResponseGenerator.DefaultRequestLifetimeInSeconds);
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalledAndStoreThrowsException_ShouldReturnNull()
    {
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync(It.IsAny<string>(), It.IsAny<PushedAuthorizationStoredInformation>()))
            .ThrowsAsync(new Exception());

        var sut = CreateSut();

        PushedAuthorizationResponse? response = await sut.CreateResponseAsync(_request);

        response.Should().BeNull();
    }

    private void VerifyPushedAuthorizeRequestMapping(PushedAuthorizationStoredInformation? storedInfo, ValidatedAuthorizeRequest request)
    {
        storedInfo.Should().NotBeNull();
        storedInfo.AccessTokenLifetime.Should().Be(request.AccessTokenLifetime);
        storedInfo.RequestObjectValues.Should().BeEquivalentTo(request.RequestObjectValues);
        storedInfo.ValidatedResources.ApiScopes.Should().BeEquivalentTo(request.ValidatedResources.Resources.ApiScopes);
        storedInfo.ValidatedResources.OfflineAccess.Should().Be(request.ValidatedResources.Resources.OfflineAccess);
        storedInfo.CodeChallenge.Should().Be(request.CodeChallenge);
        storedInfo.CodeChallengeMethod.Should().Be(request.CodeChallengeMethod);
        storedInfo.Nonce.Should().Be(request.Nonce);
        storedInfo.RedirectUri.Should().Be(request.RedirectUri);
        storedInfo.ClientId.Should().Be(request.ClientId);
        storedInfo.Description.Should().Be(request.Description);
        storedInfo.DisplayMode.Should().Be(request.DisplayMode);
        storedInfo.GrantType.Should().Be(request.GrantType);
        storedInfo.IsApiResourceRequest.Should().Be(request.IsApiResourceRequest);
        storedInfo.LoginHint.Should().Be(request.LoginHint);
        storedInfo.ResponseMode.Should().Be(request.ResponseMode);
        storedInfo.Subject.Should().Be(request.Subject);
        storedInfo.SessionId.Should().Be(request.SessionId);
        storedInfo.WasConsentShown.Should().Be(request.WasConsentShown);
        storedInfo.IsOpenIdRequest.Should().Be(request.IsOpenIdRequest);
        storedInfo.ClientSecretVerified.Should().Be(request.Secret != null);
        storedInfo.AuthenticationContextReferenceClasses.Should().BeEquivalentTo(request.AuthenticationContextReferenceClasses);
        storedInfo.PromptModes.Should().BeEquivalentTo(request.PromptModes);
        storedInfo.RequestedResourceIndicators.Should().BeEquivalentTo(request.RequestedResourceIndicators);
        storedInfo.RequestedScopes.Should().BeEquivalentTo(request.RequestedScopes);
        storedInfo.ResponseType.Should().Be(request.ResponseType);
        storedInfo.State.Should().Be(request.State);
        storedInfo.MaxAge.Should().Be(request.MaxAge);
        storedInfo.UiLocales.Should().BeEquivalentTo(request.UiLocales);
    }
}