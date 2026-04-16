using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Open.IdentityModel;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.Default;

public class DiscoveryResponseGeneratorTests
{
    //ExtensionGrantValidator Mocks
    private readonly IEnumerable<IExtensionGrantValidator> ExtensionGrantValidators = [];
    private readonly ILogger<ExtensionGrantValidator> ExtensionGrantValidatorLogger = Mock.Of<ILogger<ExtensionGrantValidator>>();

    private readonly IdentityServerOptions Options = new();
    private ExtensionGrantValidator ExtensionGrants;
    private readonly IKeyMaterialService Keys = Mock.Of<IKeyMaterialService>();
    private readonly IResourceOwnerPasswordValidator ResourceOwnerValidator = Mock.Of<IResourceOwnerPasswordValidator>();
    private readonly IResourceStore ResourceStore = Mock.Of<IResourceStore>();
    private readonly ISecretsListParser SecretParsers = Mock.Of<ISecretsListParser>();
    private readonly ILogger<DiscoveryResponseGenerator> Logger = NullLogger<DiscoveryResponseGenerator>.Instance;

    private DiscoveryResponseGenerator CreateSut()
    {
        ExtensionGrants = new ExtensionGrantValidator(ExtensionGrantValidators, ExtensionGrantValidatorLogger);

        Mock.Get(ResourceStore)
            .Setup(x => x.GetAllResourcesAsync())
            .ReturnsAsync(new Resources());

        return new DiscoveryResponseGenerator(Options, ResourceStore, Keys, ExtensionGrants, SecretParsers,
            ResourceOwnerValidator, Logger);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthoriseEndpointDisabled_ShouldContainAuthorizationResponseIssParameterSupportedAsTrue()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = false;
        
        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/somepath", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.AuthorizationResponseIssParameterSupported);
    }

    [Theory]
    [InlineData(true), InlineData(false)]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthoriseEndpointEnabled_ShouldContainAuthorizationResponseIssParameterSupported(bool value)
    {
        Options.EnableAuthorizeResponseIssuerParam = value;
        
        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/somepath", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.AuthorizationResponseIssParameterSupported)
            .WhoseValue.Should().BeOfType<bool>()
            .Which.Should().Be(value);
    }
}