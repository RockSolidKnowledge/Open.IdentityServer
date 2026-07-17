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
    private readonly IEnumerable<IExtensionGrantValidator> _extensionGrantValidators = [];
    private readonly ILogger<ExtensionGrantValidator> _extensionGrantValidatorLogger = Mock.Of<ILogger<ExtensionGrantValidator>>();

    private readonly IdentityServerOptions _options = new();
    private ExtensionGrantValidator _extensionGrants;
    private readonly IKeyMaterialService _keys = Mock.Of<IKeyMaterialService>();
    private readonly IResourceOwnerPasswordValidator _resourceOwnerValidator = Mock.Of<IResourceOwnerPasswordValidator>();
    private readonly IResourceStore _resourceStore = Mock.Of<IResourceStore>();
    private readonly ISecretsListParser _secretParsers = Mock.Of<ISecretsListParser>();
    private readonly ITelemetryService _telemetry = Mock.Of<ITelemetryService>();
    private readonly ITrace _trace = Mock.Of<ITrace>();
    private readonly ILogger<DiscoveryResponseGenerator> _logger = NullLogger<DiscoveryResponseGenerator>.Instance;

    private DiscoveryResponseGenerator CreateSut()
    {
        _extensionGrants = new ExtensionGrantValidator(_extensionGrantValidators, _extensionGrantValidatorLogger);

        Mock.Get(_resourceStore)
            .Setup(x => x.GetAllResourcesAsync())
            .ReturnsAsync(new Resources());

        return new DiscoveryResponseGenerator(
            _options, 
            _resourceStore, 
            _keys, 
            _extensionGrants, 
            _secretParsers,
            _resourceOwnerValidator, 
            _telemetry,
            _logger);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
        var sut = CreateSut();

        await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/somepath", "https://open.ids.url");

        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Basic,
                sut,
                "CreateDiscoveryDocumentAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CreateJwkDocumentASync_WhenCalled_ShouldInitiateTelemetryTrace()
    { 
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        Mock.Get(_keys)
            .Setup(k => k.GetValidationKeysAsync())
            .ReturnsAsync([]);
        
        var sut = CreateSut();

        await sut.CreateJwkDocumentAsync();
        
        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Basic,
                sut,
                "CreateJwkDocumentAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthoriseEndpointDisabled_ShouldContainAuthorizationResponseIssParameterSupportedAsTrue()
    {
        _options.Endpoints.EnableAuthorizeEndpoint = false;
        
        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/somepath", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.AuthorizationResponseIssParameterSupported);
    }

    [Theory]
    [InlineData(true), InlineData(false)]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthoriseEndpointEnabled_ShouldContainAuthorizationResponseIssParameterSupported(bool value)
    {
        _options.EnableAuthorizeResponseIssuerParam = value;
        
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
        _options.Endpoints.EnableAuthorizeEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.AuthorizationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthorizeEndpointDisabled_ShouldNotContainAuthorizationEndpoint()
    {
        _options.Endpoints.EnableAuthorizeEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.AuthorizationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenTokenEndpointEnabled_ShouldContainTokenEndpoint()
    {
        _options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TokenEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenTokenEndpointDisabled_ShouldNotContainTokenEndpoint()
    {
        _options.Endpoints.EnableTokenEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.TokenEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenUserInfoEndpointEnabled_ShouldContainUserInfoEndpoint()
    {
        _options.Endpoints.EnableUserInfoEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.UserInfoEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenUserInfoEndpointDisabled_ShouldNotContainUserInfoEndpoint()
    {
        _options.Endpoints.EnableUserInfoEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.UserInfoEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenEndSessionEndpointEnabled_ShouldContainEndSessionEndpoint()
    {
        _options.Endpoints.EnableEndSessionEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.EndSessionEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenEndSessionEndpointDisabled_ShouldNotContainEndSessionEndpoint()
    {
        _options.Endpoints.EnableEndSessionEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.EndSessionEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenEndSessionEndpointEnabled_ShouldContainLogoutSupport()
    {
        _options.Endpoints.EnableEndSessionEndpoint = true;

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
        _options.Endpoints.EnableEndSessionEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.FrontChannelLogoutSupported);
        actual.Should().NotContainKey(OidcConstants.Discovery.BackChannelLogoutSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCheckSessionEndpointEnabled_ShouldContainCheckSessionIframe()
    {
        _options.Endpoints.EnableCheckSessionEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.CheckSessionIframe);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCheckSessionEndpointDisabled_ShouldNotContainCheckSessionIframe()
    {
        _options.Endpoints.EnableCheckSessionEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.CheckSessionIframe);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenRevocationEndpointEnabled_ShouldContainRevocationEndpoint()
    {
        _options.Endpoints.EnableTokenRevocationEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.RevocationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenIntrospectionEndpointEnabled_ShouldContainIntrospectionEndpoint()
    {
        _options.Endpoints.EnableIntrospectionEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.IntrospectionEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenDeviceAuthorizationEndpointEnabled_ShouldContainDeviceAuthorizationEndpoint()
    {
        _options.Endpoints.EnableDeviceAuthorizationEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.DeviceAuthorizationEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowEndpointsDisabled_ShouldNotContainAnyEndpoints()
    {
        _options.Discovery.ShowEndpoints = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.AuthorizationEndpoint);
        actual.Should().NotContainKey(OidcConstants.Discovery.TokenEndpoint);
        actual.Should().NotContainKey(OidcConstants.Discovery.UserInfoEndpoint);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowGrantTypesEnabled_ShouldContainStandardGrantTypes()
    {
        _options.Discovery.ShowGrantTypes = true;

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
        _options.Discovery.ShowGrantTypes = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.GrantTypesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenDeviceEndpointEnabled_ShouldContainDeviceCodeGrantType()
    {
        _options.Discovery.ShowGrantTypes = true;
        _options.Endpoints.EnableDeviceAuthorizationEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.GrantTypesSupported)
            .WhoseValue.Should().BeOfType<string[]>()
            .Which.Should().Contain(OidcConstants.GrantTypes.DeviceCode);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseTypesEnabled_ShouldContainResponseTypes()
    {
        _options.Discovery.ShowResponseTypes = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ResponseTypesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseTypesDisabled_ShouldNotContainResponseTypes()
    {
        _options.Discovery.ShowResponseTypes = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.ResponseTypesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseModesEnabled_ShouldContainResponseModes()
    {
        _options.Discovery.ShowResponseModes = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ResponseModesSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowResponseModesDisabled_ShouldNotContainResponseModes()
    {
        _options.Discovery.ShowResponseModes = false;

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
        _options.Endpoints.EnableAuthorizeEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.RequestParameterSupported)
            .WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthorizeEndpointDisabled_ShouldNotContainRequestParameterSupported()
    {
        _options.Endpoints.EnableAuthorizeEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.RequestParameterSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenJwtRequestUriEnabled_ShouldContainRequestUriParameterSupported()
    {
        _options.Endpoints.EnableAuthorizeEndpoint = true;
        _options.Endpoints.EnableJwtRequestUri = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.RequestUriParameterSupported)
            .WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenJwtRequestUriDisabled_ShouldNotContainRequestUriParameterSupported()
    {
        _options.Endpoints.EnableAuthorizeEndpoint = true;
        _options.Endpoints.EnableJwtRequestUri = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.RequestUriParameterSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabled_ShouldContainTlsClientCertificateBoundAccessTokens()
    {
        _options.MutualTls.Enabled = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TlsClientCertificateBoundAccessTokens)
            .WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsDisabled_ShouldNotContainTlsClientCertificateBoundAccessTokens()
    {
        _options.MutualTls.Enabled = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.TlsClientCertificateBoundAccessTokens);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledWithTokenEndpoint_ShouldContainMtlsEndpointAliases()
    {
        _options.MutualTls.Enabled = true;
        _options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.MtlsEndpointAliases);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledWithDomainName_ShouldUseDomainBasedEndpoints()
    {
        _options.MutualTls.Enabled = true;
        _options.MutualTls.DomainName = "mtls.example.com";
        _options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.MtlsEndpointAliases)
            .WhoseValue.Should().BeOfType<Dictionary<string, string>>()
            .Which[OidcConstants.Discovery.TokenEndpoint].Should().StartWith("https://mtls.example.com/");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledWithSubDomainName_ShouldUseSubDomainBasedEndpoints()
    {
        _options.MutualTls.Enabled = true;
        _options.MutualTls.DomainName = "mtls";
        _options.Endpoints.EnableTokenEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.MtlsEndpointAliases)
            .WhoseValue.Should().BeOfType<Dictionary<string, string>>()
            .Which[OidcConstants.Discovery.TokenEndpoint].Should().StartWith("https://mtls.open.ids.url/");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowKeySetEnabledAndKeysExist_ShouldContainJwksUri()
    {
        _options.Discovery.ShowKeySet = true;
        Mock.Get(_keys)
            .Setup(x => x.GetValidationKeysAsync())
            .ReturnsAsync([new SecurityKeyInfo { Key = new RsaSecurityKey(System.Security.Cryptography.RSA.Create()) }]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.JwksUri);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowKeySetEnabledAndNoKeys_ShouldNotContainJwksUri()
    {
        _options.Discovery.ShowKeySet = true;
        Mock.Get(_keys)
            .Setup(x => x.GetValidationKeysAsync())
            .ReturnsAsync([]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.JwksUri);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowKeySetDisabled_ShouldNotContainJwksUri()
    {
        _options.Discovery.ShowKeySet = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.JwksUri);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCustomEntriesConfigured_ShouldContainCustomEntries()
    {
        _options.Discovery.CustomEntries.Add("custom_key", "custom_value");

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey("custom_key")
            .WhoseValue.Should().Be("custom_value");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCustomEntryWithRelativePath_ShouldExpandPath()
    {
        _options.Discovery.CustomEntries.Add("custom_endpoint", "~/custom");
        _options.Discovery.ExpandRelativePathsInCustomEntries = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey("custom_endpoint")
            .WhoseValue.Should().Be("https://open.ids.url/custom");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenCustomEntryConflictsWithExistingKey_ShouldNotOverwrite()
    {
        _options.Discovery.CustomEntries.Add(OidcConstants.Discovery.Issuer, "bad_issuer");

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.Issuer)
            .WhoseValue.Should().Be("https://open.ids.url");
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenShowIdentityScopesEnabled_ShouldContainScopesSupported()
    {
        _options.Discovery.ShowIdentityScopes = true;

        var sut = CreateSut();
        
        Mock.Get(_resourceStore)
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
        _options.Discovery.ShowApiScopes = true;

        var sut = CreateSut();

        Mock.Get(_resourceStore)
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
        _options.Discovery.ShowClaims = true;

        var sut = CreateSut();
        
        Mock.Get(_resourceStore)
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
        _options.Discovery.ShowTokenEndpointAuthenticationMethods = true;
        Mock.Get(_secretParsers)
            .Setup(x => x.GetAvailableAuthenticationMethods())
            .Returns(["client_secret_basic"]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TokenEndpointAuthenticationMethodsSupported);
    }

    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenMtlsEnabledAndShowAuthMethods_ShouldIncludeTlsAuthMethods()
    {
        _options.Discovery.ShowTokenEndpointAuthenticationMethods = true;
        _options.MutualTls.Enabled = true;
        Mock.Get(_secretParsers)
            .Setup(x => x.GetAvailableAuthenticationMethods())
            .Returns(["client_secret_basic"]);

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.TokenEndpointAuthenticationMethodsSupported)
            .WhoseValue.Should().BeOfType<List<string>>()
            .Which.Should().Contain(OidcConstants.EndpointAuthenticationMethods.TlsClientAuth)
            .And.Contain(OidcConstants.EndpointAuthenticationMethods.SelfSignedTlsClientAuth);
    }
    
    
    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenAuthorizeEndpointEnabled_ShouldContainClaimsParameterSupported()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = true;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().ContainKey(OidcConstants.Discovery.ClaimsParameterSupported)
            .WhoseValue.Should().Be(true);
    }
    
    [Fact]
    public async Task CreateDiscoveryDocumentAsync_WhenNAuthorizeEndpointNotEnabled_ShouldNotContainClaimsParameterSupported()
    {
        Options.Endpoints.EnableAuthorizeEndpoint = false;

        var sut = CreateSut();

        var actual = await sut.CreateDiscoveryDocumentAsync("https://open.ids.url/", "https://open.ids.url");

        actual.Should().NotContainKey(OidcConstants.Discovery.ClaimsParameterSupported);
    }
}