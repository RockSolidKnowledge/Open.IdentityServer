// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.Default;

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

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCalled_ShouldContainIssuer()
    {
        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/somepath", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.Issuer)
            .WhoseValue.Should().Be("https://open.ids.url");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthorizeEndpointEnabled_ShouldContainAuthorizationEndpoint()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.AuthorizationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthorizeEndpointDisabled_ShouldNotContainAuthorizationEndpoint()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.AuthorizationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenTokenEndpointEnabled_ShouldContainTokenEndpoint()
    {
        Options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TokenEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenTokenEndpointDisabled_ShouldNotContainTokenEndpoint()
    {
        Options.Endpoints.EnableTokenEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.TokenEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenUserInfoEndpointEnabled_ShouldContainUserInfoEndpoint()
    {
        Options.Endpoints.EnableUserInfoEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.UserInfoEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenUserInfoEndpointDisabled_ShouldNotContainUserInfoEndpoint()
    {
        Options.Endpoints.EnableUserInfoEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.UserInfoEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenEndSessionEndpointEnabled_ShouldContainEndSessionEndpoint()
    {
        Options.Endpoints.EnableEndSessionEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.EndSessionEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenEndSessionEndpointDisabled_ShouldNotContainEndSessionEndpoint()
    {
        Options.Endpoints.EnableEndSessionEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.EndSessionEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenEndSessionEndpointEnabled_ShouldContainLogoutSupport()
    {
        Options.Endpoints.EnableEndSessionEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.FrontChannelLogoutSupported)
            .WhoseValue.Should().Be(true);
        actual.Should().ContainKey(OidcConstants.Discovery.FrontChannelLogoutSessionSupported)
            .WhoseValue.Should().Be(true);
        actual.Should().ContainKey(OidcConstants.Discovery.BackChannelLogoutSupported)
            .WhoseValue.Should().Be(true);
        actual.Should().ContainKey(OidcConstants.Discovery.BackChannelLogoutSessionSupported)
            .WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenEndSessionEndpointDisabled_ShouldNotContainLogoutSupport()
    {
        Options.Endpoints.EnableEndSessionEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.FrontChannelLogoutSupported);
        actual.Should().NotContainKey(OidcConstants.Discovery.BackChannelLogoutSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCheckSessionEndpointEnabled_ShouldContainCheckSessionIframe()
    {
        Options.Endpoints.EnableCheckSessionEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.CheckSessionIframe);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCheckSessionEndpointDisabled_ShouldNotContainCheckSessionIframe()
    {
        Options.Endpoints.EnableCheckSessionEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.CheckSessionIframe);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenRevocationEndpointEnabled_ShouldContainRevocationEndpoint()
    {
        Options.Endpoints.EnableTokenRevocationEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.RevocationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenIntrospectionEndpointEnabled_ShouldContainIntrospectionEndpoint()
    {
        Options.Endpoints.EnableIntrospectionEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.IntrospectionEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenDeviceAuthorizationEndpointEnabled_ShouldContainDeviceAuthorizationEndpoint()
    {
        Options.Endpoints.EnableDeviceAuthorizationEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.DeviceAuthorizationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowEndpointsDisabled_ShouldNotContainAnyEndpoints()
    {
        Options.Discovery.ShowEndpoints = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.AuthorizationEndpoint);
        actual.Should().NotContainKey(OidcConstants.Discovery.TokenEndpoint);
        actual.Should().NotContainKey(OidcConstants.Discovery.UserInfoEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowGrantTypesEnabled_ShouldContainStandardGrantTypes()
    {
        Options.Discovery.ShowGrantTypes = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.GrantTypesSupported)
            .WhoseValue.Should().BeOfType<string[]>()
            .Which.Should().Contain(OidcConstants.GrantTypes.AuthorizationCode)
            .And.Contain(OidcConstants.GrantTypes.ClientCredentials)
            .And.Contain(OidcConstants.GrantTypes.RefreshToken)
            .And.Contain(OidcConstants.GrantTypes.Implicit);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowGrantTypesDisabled_ShouldNotContainGrantTypes()
    {
        Options.Discovery.ShowGrantTypes = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.GrantTypesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenDeviceEndpointEnabled_ShouldContainDeviceCodeGrantType()
    {
        Options.Discovery.ShowGrantTypes = true;
        Options.Endpoints.EnableDeviceAuthorizationEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.GrantTypesSupported)
            .WhoseValue.Should().BeOfType<string[]>()
            .Which.Should().Contain(OidcConstants.GrantTypes.DeviceCode);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseTypesEnabled_ShouldContainResponseTypes()
    {
        Options.Discovery.ShowResponseTypes = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ResponseTypesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseTypesDisabled_ShouldNotContainResponseTypes()
    {
        Options.Discovery.ShowResponseTypes = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.ResponseTypesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseModesEnabled_ShouldContainResponseModes()
    {
        Options.Discovery.ShowResponseModes = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ResponseModesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseModesDisabled_ShouldNotContainResponseModes()
    {
        Options.Discovery.ShowResponseModes = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.ResponseModesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCalled_ShouldContainSubjectTypesSupported()
    {
        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.SubjectTypesSupported)
            .WhoseValue.Should().BeOfType<string[]>()
            .Which.Should().Contain("public");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCalled_ShouldContainCodeChallengeMethodsSupported()
    {
        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.CodeChallengeMethodsSupported)
            .WhoseValue.Should().BeOfType<string[]>()
            .Which.Should().Contain(OidcConstants.CodeChallengeMethods.Plain)
            .And.Contain(OidcConstants.CodeChallengeMethods.Sha256);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthorizeEndpointEnabled_ShouldContainRequestParameterSupported()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.RequestParameterSupported)
            .WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthorizeEndpointDisabled_ShouldNotContainRequestParameterSupported()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.RequestParameterSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenJwtRequestUriEnabled_ShouldContainRequestUriParameterSupported()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = true;
        Options.Endpoints.EnableJwtRequestUri = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.RequestUriParameterSupported)
            .WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenJwtRequestUriDisabled_ShouldNotContainRequestUriParameterSupported()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = true;
        Options.Endpoints.EnableJwtRequestUri = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.RequestUriParameterSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabled_ShouldContainTlsClientCertificateBoundAccessTokens()
    {
        Options.MutualTls.Enabled = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TlsClientCertificateBoundAccessTokens)
            .WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsDisabled_ShouldNotContainTlsClientCertificateBoundAccessTokens()
    {
        Options.MutualTls.Enabled = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.TlsClientCertificateBoundAccessTokens);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledWithTokenEndpoint_ShouldContainMtlsEndpointAliases()
    {
        Options.MutualTls.Enabled = true;
        Options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.MtlsEndpointAliases);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledWithDomainName_ShouldUseDomainBasedEndpoints()
    {
        Options.MutualTls.Enabled = true;
        Options.MutualTls.DomainName = "mtls.example.com";
        Options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.MtlsEndpointAliases)
            .WhoseValue.Should().BeOfType<Dictionary<string, string>>()
            .Which[OidcConstants.Discovery.TokenEndpoint].Should().StartWith("https://mtls.example.com/");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledWithSubDomainName_ShouldUseSubDomainBasedEndpoints()
    {
        Options.MutualTls.Enabled = true;
        Options.MutualTls.DomainName = "mtls";
        Options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.MtlsEndpointAliases)
            .WhoseValue.Should().BeOfType<Dictionary<string, string>>()
            .Which[OidcConstants.Discovery.TokenEndpoint].Should().StartWith("https://mtls.open.ids.url/");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowKeySetEnabledAndKeysExist_ShouldContainJwksUri()
    {
        Options.Discovery.ShowKeySet = true;
        Mock.Get(Keys)
            .Setup(x => x.GetValidationKeysAsync())
            .ReturnsAsync([new SecurityKeyInfo { Key = new RsaSecurityKey(System.Security.Cryptography.RSA.Create()) }]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.JwksUri);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowKeySetEnabledAndNoKeys_ShouldNotContainJwksUri()
    {
        Options.Discovery.ShowKeySet = true;
        Mock.Get(Keys)
            .Setup(x => x.GetValidationKeysAsync())
            .ReturnsAsync([]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.JwksUri);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowKeySetDisabled_ShouldNotContainJwksUri()
    {
        Options.Discovery.ShowKeySet = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.JwksUri);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCustomEntriesConfigured_ShouldContainCustomEntries()
    {
        Options.Discovery.CustomEntries.Add("custom_key", "custom_value");

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey("custom_key")
            .WhoseValue.Should().Be("custom_value");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCustomEntryWithRelativePath_ShouldExpandPath()
    {
        Options.Discovery.CustomEntries.Add("custom_endpoint", "~/custom");
        Options.Discovery.ExpandRelativePathsInCustomEntries = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey("custom_endpoint")
            .WhoseValue.Should().Be("https://open.ids.url/custom");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCustomEntryConflictsWithExistingKey_ShouldNotOverwrite()
    {
        Options.Discovery.CustomEntries.Add(OidcConstants.Discovery.Issuer, "bad_issuer");

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.Issuer)
            .WhoseValue.Should().Be("https://open.ids.url");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowIdentityScopesEnabled_ShouldContainScopesSupported()
    {
        Options.Discovery.ShowIdentityScopes = true;

        var sut = CreateSut();
        
        Mock.Get(ResourceStore)
            .Setup(x => x.GetAllResourcesAsync())
            .ReturnsAsync(new Resources(
                [
                    new IdentityResource("openid", ["sub"]) { Enabled = true, ShowInDiscoveryDocument = true },
                    new IdentityResource("profile", ["sub"]) { Enabled = false, ShowInDiscoveryDocument = true },
                ],
                [],
                []));

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ScopesSupported)
            .WhoseValue.Should().BeEquivalentTo(new[] { "openid", "offline_access" });
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowApiScopesEnabled_ShouldContainOfflineAccess()
    {
        Options.Discovery.ShowApiScopes = true;

        var sut = CreateSut();

        Mock.Get(ResourceStore)
            .Setup(x => x.GetAllResourcesAsync())
            .ReturnsAsync(new Resources(
                [],
                [],
                [
                    new ApiScope("api1") { Enabled = true, ShowInDiscoveryDocument = true },
                    new ApiScope("api2") { Enabled = false, ShowInDiscoveryDocument = true },
                ]));
        
        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ScopesSupported)
            .WhoseValue.Should().BeEquivalentTo(new[] { IdentityServerConstants.StandardScopes.OfflineAccess, "api1" });
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowClaimsEnabled_ShouldContainClaimsSupported()
    {
        Options.Discovery.ShowClaims = true;

        var sut = CreateSut();
        
        Mock.Get(ResourceStore)
            .Setup(x => x.GetAllResourcesAsync())
            .ReturnsAsync(new Resources(
                [
                    new IdentityResource("openid", ["sub"]) { Enabled = true, ShowInDiscoveryDocument = true },
                    new IdentityResource("other", ["otherClaim"]) { Enabled = false, ShowInDiscoveryDocument = true },
                ],
                [],
                []));

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ClaimsSupported)
            .WhoseValue.Should().BeEquivalentTo(new[] { "sub" });
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowTokenEndpointAuthMethodsEnabled_ShouldContainAuthMethods()
    {
        Options.Discovery.ShowTokenEndpointAuthenticationMethods = true;
        Mock.Get(SecretParsers)
            .Setup(x => x.GetAvailableAuthenticationMethods())
            .Returns(["client_secret_basic"]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TokenEndpointAuthenticationMethodsSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledAndShowAuthMethods_ShouldIncludeTlsAuthMethods()
    {
        Options.Discovery.ShowTokenEndpointAuthenticationMethods = true;
        Options.MutualTls.Enabled = true;
        Mock.Get(SecretParsers)
            .Setup(x => x.GetAvailableAuthenticationMethods())
            .Returns(["client_secret_basic"]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TokenEndpointAuthenticationMethodsSupported)
            .WhoseValue.Should().BeOfType<List<string>>()
            .Which.Should().Contain(OidcConstants.EndpointAuthenticationMethods.TlsClientAuth)
            .And.Contain(OidcConstants.EndpointAuthenticationMethods.SelfSignedTlsClientAuth);
    }
}