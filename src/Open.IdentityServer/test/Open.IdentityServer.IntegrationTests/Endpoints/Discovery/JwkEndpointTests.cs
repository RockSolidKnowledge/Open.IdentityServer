// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Xunit;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace IdentityServer.IntegrationTests.Endpoints.Discovery;

public class JwkEndpointTests
{
    private const string Category = "Discovery JWK endpoint";

    [Fact]
    [Trait("Category", Category)]
    public async Task Jwks_entries_should_countain_crv()
    {
        var ecdsaKey = CryptoHelper.CreateECDsaSecurityKey(JsonWebKeyECTypes.P256);
        var parameters = ecdsaKey.ECDsa.ExportParameters(true);

        IdentityServerPipeline pipeline = new IdentityServerPipeline();

        var jsonWebKeyFromEcDsa = new JsonWebKey()
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
                .AddSigningCredential(jsonWebKeyFromEcDsa, SecurityAlgorithms.EcdsaSha256);
        };

        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient!.GetAsync(
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

        var result = await pipeline.BackChannelClient!.GetAsync(
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

        var result = await pipeline.BackChannelClient!.GetAsync(
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

        var result = await pipeline.BackChannelClient!.GetAsync(
            "https://server/root/.well-known/openid-configuration/jwks", 
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var jwks = new JsonWebKeySet(json);

        jwks.Keys.Should().Contain(x => x.KeyId == ecdsaKey.KeyId && x.Alg == "ES256");
        jwks.Keys.Should().Contain(x => x.KeyId == rsaKey.KeyId && x.Alg == "RS256");
    }

    [Fact]
    public async Task Jwks_WhenCompatibilityStoreConfigured_ShouldGetKeysFromStore()
    {
        IdentityServerPipeline pipeline = new IdentityServerPipeline();
        
        pipeline.OnPostConfigureServices += services =>
        {
            services.AddScoped<IIdentityServerKeyStore, FakeIdentityServerKeyStore>();
            services.AddIdentityServerBuilder()
                .AddCompatibilityKeyStores();
        };
        pipeline.Initialize("/ROOT");

        var result = await pipeline.BackChannelClient!.GetAsync(
            "https://server/root/.well-known/openid-configuration/jwks", 
            TestContext.Current.CancellationToken);

        var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var jwks = new JsonWebKeySet(json);

        //RSA Keys
        jwks.Keys.Should().Contain(x => x.KeyId == "FakeRSA_RS256" && x.Alg == "RS256");
        
        //EC Curve Keys
        jwks.Keys.Should().Contain(x => x.KeyId == "FakeEC_ES256" && x.Alg == "ES256");
        jwks.Keys.Should().Contain(x => x.KeyId == "FakeEC_ES384" && x.Alg == "ES384");
        jwks.Keys.Should().Contain(x => x.KeyId == "FakeEC_ES521" && x.Alg == "ES521");
        
        //Non Signing Keys
        jwks.Keys.Should().NotContain(x => x.KeyId == "FakeRSA_PS256_NONSIGN" && x.Alg == "RS256");
    }
}