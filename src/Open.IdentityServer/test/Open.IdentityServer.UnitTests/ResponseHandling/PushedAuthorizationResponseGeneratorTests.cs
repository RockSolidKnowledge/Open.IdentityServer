using System;
using System.Collections.Specialized;
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
    private readonly Mock<IPushedAuthorizationRequestService> service = new();
    private Mock<ILogger<PushedAuthorizationResponseGenerator>> _logger = new();
    
    private ValidatedAuthorizeRequest _request = new ValidatedAuthorizeRequest() { Raw = new NameValueCollection() };

    public PushedAuthorizationResponseGeneratorTests()
    {
      
    }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalled_ShouldMapRequestCorrectlyAndSendToStore()
    {
        Uri expectedKey = new Uri($"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}232444234");
        
        _request.Client = new Client();
        
        var sut = CreateSut();
        
        PushedAuthorizationMemento? storedInfo = null;

        service.Setup(s => s.CreateAsync(_request.Client, _request.Raw))
            .ReturnsAsync(new PushedAuthorization(expectedKey, TimeSpan.FromSeconds(20)));
        
        var response = await sut.CreateResponseAsync(_request);

        response!.Lifetime.Should().Be(20);
        response.Uri.Should().Be(expectedKey.ToString());
    }
    
    
    // [Fact]
    // public async Task CreateResponseAsync_WhenCalled_ShouldSetExpirationBasedOnOptions()
    // {
    //     string generatedUniquePart = "sdufbsibdvibv";
    //
    //     _request.Client = new Client();
    //     
    //     DateTime expectedExpiration;
    //     DateTime now = new DateTime(2027, 3, 2, 13, 10, 20);
    //     DateTimeOffset spiedExpiration = now;
    //     
    //     options.PushedAuthorization.Expiration = TimeSpan.FromSeconds(90);
    //
    //     clock.Setup(c => c.GetUtcNow()).Returns(now);
    //     expectedExpiration = now.Add(options.PushedAuthorization.Expiration);
    //     
    //     _handleGenerationService.Setup(g => g.GenerateAsync()).ReturnsAsync(generatedUniquePart);
    //     
    //     var sut = CreateSut();
    //    
    //     _store.Setup(s => s.StorePushedAuthorizationRequestAsync( It.IsAny<PushedAuthorizationMemento>()))
    //         .Callback<PushedAuthorizationMemento>(memento => spiedExpiration = memento.ValidUntil);
    //     
    //     await sut.CreateResponseAsync(_request);
    //
    //     spiedExpiration.Should().Be(expectedExpiration);
    // }
    
    // [Fact]
    // public async Task CreateResponseAsync_WhenCalled_ShouldSetExpirationBasedOnClientProperties()
    // {
    //     string generatedUniquePart = "sdufbsibdvibv";
    //
    //     _request.Client = new Client()
    //     {
    //         PushedAuthorizationLifetime = TimeSpan.FromSeconds(30).Seconds
    //     };
    //     
    //     DateTime expectedExpiration;
    //     DateTime now = new DateTime(2027, 3, 2, 13, 10, 20);
    //     DateTimeOffset spiedExpiration = now;
    //
    //     clock.Setup(c => c.GetUtcNow()).Returns(now);
    //     int expectedDuration = (int)_request.Client.PushedAuthorizationLifetime;
    //     expectedExpiration = now.AddSeconds(expectedDuration);
    //     
    //     _handleGenerationService.Setup(g => g.GenerateAsync()).ReturnsAsync(generatedUniquePart);
    //     
    //     var sut = CreateSut();
    //    
    //     _store.Setup(s => s.StorePushedAuthorizationRequestAsync( It.IsAny<PushedAuthorizationMemento>()))
    //         .Callback<PushedAuthorizationMemento>( memento => spiedExpiration = memento.ValidUntil);
    //     
    //     var response = await sut.CreateResponseAsync(_request);
    //
    //     response.Should().NotBeNull();
    //     spiedExpiration.Should().Be(expectedExpiration);
    //     response.Lifetime.Should().Be(expectedDuration);
    // }

    // [Fact]
    // public async Task CreateResponseAsync_WhenCalled_ShouldGenerateResponseCorrectly()
    // {
    //     string generatedUniquePart = "sdufbsibdvibv";
    //     string expectedUri =
    //         $"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{generatedUniquePart}";
    //
    //     _handleGenerationService.Setup(g => g.GenerateAsync()).ReturnsAsync(generatedUniquePart);
    //
    //     var sut = CreateSut();
    //
    //     PushedAuthorizationResponse? response = await sut.CreateResponseAsync(_request);
    //
    //     response.Should().NotBeNull();
    //     response.Uri.Should().Be(expectedUri);
    //     response.Lifetime.Should().Be(PushedAuthorizationResponseGenerator.DefaultRequestLifetimeInSeconds);
    // }
    
    [Fact]
    public async Task CreateResponseAsync_WhenCalledAndServiceThrowsException_ShouldReturnNull()
    {
        service.Setup(s => s.CreateAsync(It.IsAny<Client>(),It.IsAny<NameValueCollection>()))
            .ThrowsAsync(new Exception());

        var sut = CreateSut();

        PushedAuthorizationResponse? response = await sut.CreateResponseAsync(_request);

        response.Should().BeNull();
    }
    
    private PushedAuthorizationResponseGenerator CreateSut()
    {
        return new PushedAuthorizationResponseGenerator(service.Object, _logger.Object);
    }
}