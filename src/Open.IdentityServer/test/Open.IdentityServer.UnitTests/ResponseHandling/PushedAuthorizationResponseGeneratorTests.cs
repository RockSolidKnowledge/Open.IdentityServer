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
    private Mock<TimeProvider> clock = new Mock<TimeProvider>();
    private IdentityServerOptions options = new IdentityServerOptions();

    private ValidatedAuthorizeRequest _request = new ValidatedAuthorizeRequest() { Raw = new NameValueCollection() };

    public PushedAuthorizationResponseGeneratorTests()
    {
      
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldMapRequestCorrectlyAndSendToStore()
    {
        var sut = CreateSut();
        
        PushedAuthorizationMemento? storedInfo = null;
        
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync( It.IsAny<PushedAuthorizationMemento>()))
            .Callback<PushedAuthorizationMemento>(info => storedInfo = info);
        
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
        
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync( It.IsAny<PushedAuthorizationMemento>()))
            .Callback<PushedAuthorizationMemento>(memento => passedId = memento.Key);
        
        await sut.CreateResponseAsync(_request);
        
        passedId.Should().NotBeNull();
        passedId.Should().Be(IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + generatedUniquePart);
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldSetExpirationBasedOnOptions()
    {
        string generatedUniquePart = "sdufbsibdvibv";

        _request.Client = new Client();
        
        DateTime expectedExpiration;
        DateTime now = new DateTime(2027, 3, 2, 13, 10, 20);
        DateTimeOffset spiedExpiration = now;
        
        options.PushedAuthorization.Expiration = TimeSpan.FromSeconds(90);

        clock.Setup(c => c.GetUtcNow()).Returns(now);
        expectedExpiration = now.Add(options.PushedAuthorization.Expiration);
        
        _handleGenerationService.Setup(g => g.GenerateAsync()).ReturnsAsync(generatedUniquePart);
        
        var sut = CreateSut();
       
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync( It.IsAny<PushedAuthorizationMemento>()))
            .Callback<PushedAuthorizationMemento>(memento => spiedExpiration = memento.ValidUntil);
        
        await sut.CreateResponseAsync(_request);

        spiedExpiration.Should().Be(expectedExpiration);
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldSetExpirationBasedOnClientProperties()
    {
        string generatedUniquePart = "sdufbsibdvibv";

        _request.Client = new Client()
        {
            PushedAuthorizationLifetime = TimeSpan.FromSeconds(30).Seconds
        };
        
        DateTime expectedExpiration;
        DateTime now = new DateTime(2027, 3, 2, 13, 10, 20);
        DateTimeOffset spiedExpiration = now;

        clock.Setup(c => c.GetUtcNow()).Returns(now);
        int expectedDuration = (int)_request.Client.PushedAuthorizationLifetime;
        expectedExpiration = now.AddSeconds(expectedDuration);
        
        _handleGenerationService.Setup(g => g.GenerateAsync()).ReturnsAsync(generatedUniquePart);
        
        var sut = CreateSut();
       
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync( It.IsAny<PushedAuthorizationMemento>()))
            .Callback<PushedAuthorizationMemento>( memento => spiedExpiration = memento.ValidUntil);
        
        var response = await sut.CreateResponseAsync(_request);

        response.Should().NotBeNull();
        spiedExpiration.Should().Be(expectedExpiration);
        response.Lifetime.Should().Be(expectedDuration);
    }

    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldGenerateResponseCorrectly()
    {
        string generatedUniquePart = "sdufbsibdvibv";
        string expectedUri =
            $"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{generatedUniquePart}";

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
        _store.Setup(s => s.StorePushedAuthorizationRequestAsync(It.IsAny<PushedAuthorizationMemento>()))
            .ThrowsAsync(new Exception());

        var sut = CreateSut();

        PushedAuthorizationResponse? response = await sut.CreateResponseAsync(_request);

        response.Should().BeNull();
    }
    
    private void VerifyPushedAuthorizeRequestMapping(PushedAuthorizationMemento? storedInfo, ValidatedAuthorizeRequest request)
    {
        storedInfo.Should().NotBeNull();
        storedInfo.Parameters.Should().BeEquivalentTo(request.Raw);
        storedInfo.Key.Should().NotBeEmpty();
    }
    
    private PushedAuthorizationResponseGenerator CreateSut()
    {
        return new PushedAuthorizationResponseGenerator(_store.Object, 
            _handleGenerationService.Object, 
            clock.Object,
            options,
            _logger.Object);
    }
}