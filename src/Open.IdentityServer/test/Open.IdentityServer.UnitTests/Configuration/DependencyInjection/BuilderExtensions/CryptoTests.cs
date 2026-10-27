using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Open.IdentityServer.UnitTests.Configuration.DependencyInjection;

public class CryptoTests
{
    private IServiceCollection serviceCollection = new ServiceCollection();

    [Fact]
    public void AddCompatibilityKeyStores_When_Should()
    {
        //TODO Implement tests
        false.Should().BeTrue();
    }
}