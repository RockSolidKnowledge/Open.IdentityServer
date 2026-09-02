using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Services.Default;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultPushedAuthorizationRequestServiceTests
{
    private Mock<TimeProvider> clock = new();
    private IdentityServerOptions options = new();
    private Mock<IHandleGenerationService> handleGeneration = new();
    private Mock<IPushedAuthorizationRequestStore> store = new();
    private Mock<ILogger<DefaultPushedAuthorizationRequestService>> logger = new();

    public DefaultPushedAuthorizationRequestServiceTests()
    {
    }

    [Fact]
    public async Task CreateResponse_when_called_should_return_response()
    {
        NameValueCollection parameters = new();
        string expectedHandle = "someHandle";
        handleGeneration.Setup(hg => hg.GenerateAsync()).ReturnsAsync(expectedHandle);
        
        var sut = CreateSut();

        var result = await sut.CreateAsync(new Client(),parameters);

        result.Key.Should()
            .Be($"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{expectedHandle}");
        result.ExpiresIn.Should().Be(options.PushedAuthorization.Expiration);
    }
    
    [Fact]
    public async Task CreateResponse_when_called_should_store_parameters_and_global_expiration()
    {
        Client client = new Client();
        DateTimeOffset now = new DateTimeOffset(2026, 6, 2, 15, 0, 0, TimeSpan.FromSeconds(0));
        DateTimeOffset expectedExpiration = now.Add(options.PushedAuthorization.Expiration);
        NameValueCollection parameters = new();
        string expectedHandle = "someHandle";
        string expectedKey = $"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{expectedHandle}";
        
        clock.Setup(c => c.GetUtcNow()).Returns(now);
        
        handleGeneration.Setup(hg => hg.GenerateAsync()).ReturnsAsync(expectedHandle);
        
        var sut = CreateSut();

        var result = await sut.CreateAsync(client,parameters);

        store.Verify(s => s.StorePushedAuthorizationRequestAsync(
            new PushedAuthorizationMemento(
                expectedKey,expectedExpiration,parameters
                )),Times.Once);
        
        result.ExpiresIn.Should().Be(options.PushedAuthorization.Expiration);
    }
    
    [Fact]
    public async Task CreateResponse_when_called_should_store_parameters_and_per_client_expiration()
    {
        Client client = new Client() { PushedAuthorizationLifetime = 70 };
        
        DateTimeOffset now = new DateTimeOffset(2026, 6, 2, 15, 0, 0, TimeSpan.FromSeconds(0));
        DateTimeOffset expectedExpiration = now.Add( TimeSpan.FromSeconds(client.PushedAuthorizationLifetime.Value));
        NameValueCollection parameters = new();
        string expectedHandle = "someHandle";
        string expectedKey = $"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{expectedHandle}";

        
        clock.Setup(c => c.GetUtcNow()).Returns(now);
        
        handleGeneration.Setup(hg => hg.GenerateAsync()).ReturnsAsync(expectedHandle);
        
        var sut = CreateSut();

        var result = await sut.CreateAsync(client,parameters);

        store.Verify(s => s.StorePushedAuthorizationRequestAsync(
            new PushedAuthorizationMemento(
                expectedKey,expectedExpiration,parameters
            )),Times.Once);

        result.ExpiresIn.Should().Be(TimeSpan.FromSeconds(client.PushedAuthorizationLifetime.Value));
    }
    
    [Fact]
    public async Task ConsumeResponse_when_called_with_non_expired_key_should_return_parameters()
    {
        DateTimeOffset now = new DateTimeOffset(2026, 6, 2, 15, 0, 0, TimeSpan.FromSeconds(0));
        DateTimeOffset expectedExpiration = now.Add(options.PushedAuthorization.Expiration);

        clock.Setup(c => c.GetUtcNow()).Returns(now);
        
        NameValueCollection parameters = new();
        string expectedHandle = "someHandle";
        string expectedKey = $"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{expectedHandle}";
        
        store.Setup(s => s.ConsumePushedAuthorizationRequestAsync(expectedKey))
            .ReturnsAsync(new PushedAuthorizationMemento(expectedKey,expectedExpiration,parameters));
        
        var sut = CreateSut();

        var result = await sut.ConsumeAsync(expectedKey);

        result.Should().Be(parameters);
    }
    
    [Fact]
    public async Task ConsumeResponse_when_called_with_an_expired_key_should_return_null()
    {
        DateTimeOffset issuedAt = new DateTimeOffset(2026, 6, 2, 15, 0, 0, TimeSpan.FromSeconds(0));
        DateTimeOffset expectedExpiration = issuedAt.Add(options.PushedAuthorization.Expiration);

        clock.Setup(c => c.GetUtcNow())
            .Returns(issuedAt.Add(options.PushedAuthorization.Expiration).AddSeconds(1));
        
        NameValueCollection parameters = new();
        string expectedHandle = "someHandle";
        string expectedKey = $"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{expectedHandle}";
        
        store.Setup(s => s.ConsumePushedAuthorizationRequestAsync(expectedKey))
            .ReturnsAsync(new PushedAuthorizationMemento(expectedKey,expectedExpiration,parameters));
        
        var sut = CreateSut();

        var result = await sut.ConsumeAsync(expectedKey);

        result.Should().BeNull();
    }

    private DefaultPushedAuthorizationRequestService CreateSut()
    {
        return new DefaultPushedAuthorizationRequestService(
            clock.Object,
            handleGeneration.Object,
            options,
            store.Object,
            logger.Object);
    }
}