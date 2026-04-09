// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AwesomeAssertions;
using Open.IdentityModel.Client;
using IdentityServer.IntegrationTests.Common;
using Open.IdentityServer;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

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
    public async Task Jwks_entries_should_countain_crv()
    {
        var ecdsaKey = CryptoHelper.CreateECDsaSecurityKey(JsonWebKeyECTypes.P256);
        var parameters = ecdsaKey.ECDsa.ExportParameters(true);

        IdentityServerPipeline pipeline = new IdentityServerPipeline();

        var jsonWebKeyFromECDsa = new JsonWebKey()
        {
            Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
            Use = "sig",
            Kid = ecdsaKey.KeyId,
            KeyId = ecdsaKey.KeyId,
            X = Base64UrlEncoder.Encode(parameters.Q.X),
            Y = Base64UrlEncoder.Encode(parameters.Q.Y),
            D = Base64UrlEncoder.Encode(parameters.D),
            Crv = JsonWebKeyECTypes.P256,
            Alg = SecurityAlgorithms.EcdsaSha256
        };
        pipeline.OnPostConfigureServices += services =>
        {
            // add ECDsa as JsonWebKey
            services.AddIdentityServerBuilder()
                .AddSigningCredential(jsonWebKeyFromECDsa, SecurityAlgorithms.EcdsaSha256);
        };

        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient.GetAsync(
            "https://server/root/.well-known/openid-configuration/jwks", 
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var data = JsonElement.Parse(json);

        var keys = data.GetProperty("keys").EnumerateArray().ToArray();
        keys.Should().NotBeNull();

        var key = keys[1];
        key.Should().NotBeNull();

        var crv = key.GetProperty("crv");
        crv.Should().NotBeNull();

        crv.GetString().Should().Be(JsonWebKeyECTypes.P256);

    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Jwks_entries_should_contain_alg()
    {
        IdentityServerPipeline pipeline = new IdentityServerPipeline();
        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient.GetAsync(
            "https://server/root/.well-known/openid-configuration/jwks",
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var data = JsonElement.Parse(json);

        var keys = data.GetProperty("keys").EnumerateArray().ToArray();
        keys.Should().NotBeNull();

        var key = keys[0];
        key.Should().NotBeNull();

        var alg = key.GetProperty("alg");
        alg.Should().NotBeNull();

        alg.GetString().Should().Be(Constants.SigningAlgorithms.RSA_SHA_256);
    }

    [Theory]
    [InlineData(JsonWebKeyECTypes.P256, SecurityAlgorithms.EcdsaSha256)]
    [InlineData(JsonWebKeyECTypes.P384, SecurityAlgorithms.EcdsaSha384)]
    [InlineData(JsonWebKeyECTypes.P521, SecurityAlgorithms.EcdsaSha512)]
    [Trait("Category", Category)]
    public async Task Jwks_with_ecdsa_should_have_parsable_key(string crv, string alg)
    {
        var key = CryptoHelper.CreateECDsaSecurityKey(crv);

        IdentityServerPipeline pipeline = new IdentityServerPipeline();
        pipeline.OnPostConfigureServices += services =>
        {
            services.AddIdentityServerBuilder()
                .AddSigningCredential(key, alg);
        };
        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient.GetAsync(
            "https://server/root/.well-known/openid-configuration/jwks", 
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var jwks = new JsonWebKeySet(json);
        var parsedKeys = jwks.GetSigningKeys();

        var matchingKey = parsedKeys.FirstOrDefault(x => x.KeyId == key.KeyId);
        matchingKey.Should().NotBeNull();
        matchingKey.Should().BeOfType<ECDsaSecurityKey>();
    }

    [Fact]
    public async Task Jwks_with_two_key_using_different_algs_expect_different_alg_values()
    {
        var ecdsaKey = CryptoHelper.CreateECDsaSecurityKey();
        var rsaKey = CryptoHelper.CreateRsaSecurityKey();

        IdentityServerPipeline pipeline = new IdentityServerPipeline();
        pipeline.OnPostConfigureServices += services =>
        {
            services.AddIdentityServerBuilder()
                .AddSigningCredential(ecdsaKey, "ES256")
                .AddValidationKey(new SecurityKeyInfo { Key = rsaKey, SigningAlgorithm = "RS256" });
        };
        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient.GetAsync(
            "https://server/root/.well-known/openid-configuration/jwks", 
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var jwks = new JsonWebKeySet(json);

        jwks.Keys.Should().Contain(x => x.KeyId == ecdsaKey.KeyId && x.Alg == "ES256");
        jwks.Keys.Should().Contain(x => x.KeyId == rsaKey.KeyId && x.Alg == "RS256");
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