// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using Open.IdentityServer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.IntegrationTests.Utility;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Discovery;

public class DiscoveryEndpointTests
{
    private const string Category = "Discovery endpoint";

    [Fact]
    [Trait("Category", Category)]
    public async Task Issuer_uri_should_be_lowercase()
    {
        IdentityServerPipeline pipeline = new IdentityServerPipeline();
        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient.GetAsync(
            "HTTPS://SERVER/ROOT/.WELL-KNOWN/OPENID-CONFIGURATION",
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var data = JsonElement.Parse(json);
        var issuer = data.GetProperty("issuer").GetString();

        issuer.Should().Be("https://server/root");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task when_lower_case_issuer_option_disabled_issuer_uri_should_be_preserved()
    {
        IdentityServerPipeline pipeline = new IdentityServerPipeline();
        pipeline.Initialize("/ROOT");

        pipeline.Options.LowerCaseIssuerUri = false;

        var result = await pipeline.BackChannelClient.GetAsync(
            "HTTPS://SERVER/ROOT/.WELL-KNOWN/OPENID-CONFIGURATION", 
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var data = JsonElement.Parse(json);
        var issuer = data.GetProperty("issuer").GetString();

        issuer.Should().Be("https://server/ROOT");
    }

    private void Pipeline_OnPostConfigureServices(IServiceCollection obj)
    {
        throw new System.NotImplementedException();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Algorithms_supported_should_match_signing_key()
    {
        var key = CryptoHelper.CreateECDsaSecurityKey(JsonWebKeyECTypes.P256);
        var expectedAlgorithm = SecurityAlgorithms.EcdsaSha256;

        IdentityServerPipeline pipeline = new IdentityServerPipeline();
        pipeline.OnPostConfigureServices += services =>
        {
            // add key to standard RSA key
            services.AddIdentityServerBuilder()
                .AddSigningCredential(key, expectedAlgorithm);
        };
        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient.GetAsync(
            "https://server/root/.well-known/openid-configuration", 
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var data = JsonElement.Parse(json);
        var algorithmsSupported = data.GetProperty("id_token_signing_alg_values_supported")
            .EnumerateArray()
            .Select(x => x.GetString())
            .ToList();

        algorithmsSupported.Count().Should().Be(2);
        algorithmsSupported.Should().Contain(SecurityAlgorithms.RsaSha256);
        algorithmsSupported.Should().Contain(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Unicode_values_in_url_should_be_processed_correctly()
    {
        var pipeline = new IdentityServerPipeline();
        pipeline.Initialize();

        var result = await pipeline.BackChannelClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
        {
            Address = "https://грант.рф",
            Policy =
            {
                ValidateIssuerName = false,
                ValidateEndpoints = false,
                RequireHttps = false,
                RequireKeySet = false
            }
        }, TestContext.Current.CancellationToken);

        result.Issuer.Should().Be("https://грант.рф");
    }
}