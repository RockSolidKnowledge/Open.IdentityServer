// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.IntegrationTests.Common;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IdentityServer4.IntegrationTests.Endpoints.Discovery
{
    public class DiscoveryEndpointTests
    {
        private const string Category = "Discovery endpoint";

        [Fact]
        [Trait("Category", Category)]
        public async Task Issuer_uri_should_be_lowercase()
        {
            IdentityServerPipeline pipeline = new IdentityServerPipeline();
            pipeline.Initialize("/ROOT");

            var result = await pipeline.BackChannelClient.GetAsync("HTTPS://SERVER/ROOT/.WELL-KNOWN/OPENID-CONFIGURATION");

            var json = await result.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var issuer = data["issuer"].ToString();

            issuer.Should().Be("https://server/root");
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
                services.AddIdentityServerBuilder()
                    .AddSigningCredential(key, expectedAlgorithm);
            };
            pipeline.Initialize("/ROOT");

            var result = await pipeline.BackChannelClient.GetAsync("https://server/root/.well-known/openid-configuration");

            var json = await result.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var algorithmsSupported = data["id_token_signing_alg_values_supported"].Values<string>();

            algorithmsSupported.Should().Contain(expectedAlgorithm);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Jwks_entries_should_contain_alg()
        {
            IdentityServerPipeline pipeline = new IdentityServerPipeline();
            pipeline.Initialize("/ROOT");

            var result = await pipeline.BackChannelClient.GetAsync("https://server/root/.well-known/openid-configuration/jwks");

            var json = await result.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var keys = data["keys"];
            keys.Should().NotBeNull();

            var key = keys[0];
            key.Should().NotBeNull();

            var alg = key["alg"];
            alg.Should().NotBeNull();

            alg.Value<string>().Should().Be(Constants.SigningAlgorithms.RSA_SHA_256);
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

            var result = await pipeline.BackChannelClient.GetAsync("https://server/root/.well-known/openid-configuration/jwks");

            var json = await result.Content.ReadAsStringAsync();
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
                    .AddValidationKey(new SecurityKeyInfo {Key = rsaKey, SigningAlgorithm = "RS256"});
            };
            pipeline.Initialize("/ROOT");

            var result = await pipeline.BackChannelClient.GetAsync("https://server/root/.well-known/openid-configuration/jwks");

            var json = await result.Content.ReadAsStringAsync();
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
            });

            result.Issuer.Should().Be("https://грант.рф");
        }
    }
}