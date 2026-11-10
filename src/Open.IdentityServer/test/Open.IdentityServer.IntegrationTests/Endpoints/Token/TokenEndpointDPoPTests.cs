// Copyright (c) 2026, Rock Solid Knowledge 
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Test.Utilities;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.Token;

public class TokenEndpointDPoPTests: DPoPTests
{
    private const string Category = "Token Endpoint - DPoP";
    
    private async Task<string> GetAuthorizationCodeAsync(string scope, string boundJkt = null)
    {
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        Parameters extra = [];

        if (!string.IsNullOrEmpty(boundJkt))
        {
            extra.Add("dpop_jkt", boundJkt);
        }
        
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: DPoPClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: scope,
            redirectUri: "https://server/cb",
            extra: extra);

        var response = await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        var authorization = new AuthorizeResponse(response.Headers.Location!.ToString());
        return authorization.Code!;
    }
    
    private async Task<HttpResponseMessage> PerformCodeFlowTokenRequest(string code, string dpopProof = null)
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", DPoPClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
        };

        return await PerformTokenRequest(data, dpopProof);
    }
    
    private async Task<HttpResponseMessage> PerformRefreshTokenExchangeRequest(string refreshToken, string dpopProof = null)
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", DPoPClient.ClientId },
            { "refresh_token", refreshToken },
            { "resource", "https://valid.resource.com" },
        };

        return await PerformTokenRequest(data, dpopProof);
    }
    
    private async Task<HttpResponseMessage> PerformTokenRequest(Dictionary<string, string> data, string dpopProof = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, IdentityServerPipeline.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(data)
        };

        // Custom headers (e.g. a DPoP proof JWT)
        if (!string.IsNullOrEmpty(dpopProof))
        {
            request.Headers.Add("DPoP", dpopProof);
        }

        return await mockPipeline.BackChannelClient!.SendAsync(
            request,
            TestContext.Current.CancellationToken);
    }

    // DPoP Client Configuration Tests
    
    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_WhenNoProof_AndClientRequiresDPoP_ReturnsInvalidDPoPProof()
    {
        ConfigureClient(requireDPoP: true);
        
        DPoPClient.AllowedGrantTypes = GrantTypes.Code;
        DPoPClient.AccessTokenType = AccessTokenType.Jwt;
        DPoPClient.RequireDPoP = true;
        
        var code = await GetAuthorizationCodeAsync("openid api");

        var response = await PerformCodeFlowTokenRequest(code);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);

        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_WhenNoProof_AndClientDoesntRequireProof_ReturnsUnboundAccessToken()
    {
        ConfigureClient();
        
        var code = await GetAuthorizationCodeAsync("openid api");
        
        var response = await PerformCodeFlowTokenRequest(code);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        
        result.TryGetString("token_type").Should().Be("Bearer");
        result.TryGetString("access_token").Should().NotBeNullOrEmpty()
            .And.BeAccessToken()
            .WithoutClaim("cnf");
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_WhenProofIatInvalid_ReturnsInvalidDPoPProof()
    {
        ConfigureClient(validationMode: DPoPTokenExpirationValidationMode.Iat);
        
        var code = await GetAuthorizationCodeAsync("openid api");

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method,
            iat: fakeNow.AddMinutes(20));
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Theory]
    [InlineData("GET", IdentityServerPipeline.TokenEndpoint)]
    [InlineData("POST", IdentityServerPipeline.AuthorizeEndpoint)]
    [Trait("Category", Category)]
    public async Task AuthCode_WhenDPoPProofHtuOrMethodsIncorrect_ShouldReturnInvalidResponse(string htm, string htu)
    {
        ConfigureClient();
        
        var code = await GetAuthorizationCodeAsync("openid api");

        var proof = dPoPProofGenerator.CreateDPoPProof(htu, htm, iat: fakeNow.AddSeconds(10));
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_ValidDPoPProofReplayed_ReturnsInvalidDPoPProof()
    {
        ConfigureClient();
        
        var code = await GetAuthorizationCodeAsync("openid api");

        var proof =  dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10));
        
        var originalRequest = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await originalRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        
        var secondCode = await GetAuthorizationCodeAsync("openid api");
        var replayedRequest = await PerformCodeFlowTokenRequest(secondCode, proof);
        
        var replayJson = await replayedRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var replayResult = JsonElement.Parse(replayJson);
        
        replayResult.TryGetProperty("error", out _).Should().BeTrue();
        replayResult.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_ValidDPoPProofPresent_AndCodeUnboundToDifferentKey_ShouldValidateAndReturnAccessTokenWithCnfJktClaim()
    {
        ConfigureClient();
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        var code = await GetAuthorizationCodeAsync("openid api");

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();

        result.TryGetString("token_type").Should().Be("DPoP");
        result.TryGetString("access_token").Should().NotBeNullOrEmpty()
            .And.BeAccessToken()
            .WithClaim("cnf", $"{{\"jkt\":\"{keyThumb}\"}}");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_ValidDPoPProofPresent_AndCodeBoundToDifferentKey_ShouldReturnInvalidDPoPProof()
    {
        ConfigureClient();
        
        //Auth code generate with different public key thumbprint
        var code = await GetAuthorizationCodeAsync("openid api", "0ZcOCORZNYy-DWpqq30jZyJGHTN0d2HglBV3uiguA4I");

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10));
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_ValidDPoPProofPresent_AndCodeBoundToPublicKey_ShouldValidateAndReturnAccessTokenWithCnfJktClaim()
    {
        ConfigureClient();
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();

        result.TryGetString("token_type").Should().Be("DPoP");
        result.TryGetString("access_token").Should().NotBeNullOrEmpty()
            .And.BeAccessToken()
            .WithClaim("cnf", $"{{\"jkt\":\"{keyThumb}\"}}");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_ValidDPoPProofPresent_AndReferenceTokenUsed_ShouldStoreThumbprintOnToken()
    {
        ConfigureClient(accessTokenType: AccessTokenType.Reference);
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetString("token_type").Should().Be("DPoP");
        result.TryGetProperty("access_token", out var accessToken).Should().BeTrue();
        
        var referenceTokenGrant = await referenceTokenStore.GetReferenceTokenAsync(accessToken.ToString());
        referenceTokenGrant.DPoPKeyThumbprint.Should().Be(keyThumb);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_WhenNonceRequired_ShouldReturnUseDPoPNonce_AndNonceHeader()
    {
        ConfigureClient(validationMode: DPoPTokenExpirationValidationMode.Nonce);
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.UseDPoPNonce);

        response.Headers.Should().ContainKey("DPoP-Nonce");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_WhenNonceRequired_AndRetryWithNonce_ShouldValidateAndReturnAccessTokenWithCnfJktClaim()
    {
        ConfigureClient(validationMode: DPoPTokenExpirationValidationMode.Nonce);
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var response = await PerformCodeFlowTokenRequest(code, proof);
        
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.UseDPoPNonce);

        response.Headers.Should().ContainKey("DPoP-Nonce");

        var serverNonce = response.Headers.GetValues("DPoP-Nonce").FirstOrDefault();

        serverNonce.Should().NotBeNull();
        
        var proofWithNonce = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), nonce: serverNonce, key: key);
        
        var nonceResponse = await PerformCodeFlowTokenRequest(code, proofWithNonce);
        
        json = await nonceResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetString("token_type").Should().Be("DPoP");
        result.TryGetString("access_token").Should().NotBeNullOrEmpty()
            .And.BeAccessToken()
            .WithClaim("cnf", $"{{\"jkt\":\"{keyThumb}\"}}");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_WhenGenerated_ShouldBeBoundToPublicKey()
    {
        ConfigureClient();
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("offline_access openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var tokenResponse = await PerformCodeFlowTokenRequest(code, proof);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tokenResult = JsonElement.Parse(tokenJson);
        tokenResult.TryGetProperty("refresh_token", out var refreshToken).Should().BeTrue();
        
        var refreshTokenGrant = await refreshTokenStore.GetRefreshTokenAsync(refreshToken.ToString());
        refreshTokenGrant.DPoPKeyThumbprint.Should().Be(keyThumb);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_WhenBound_AndProofMissing_ShouldReturnInvalidDPoPProof()
    {
        ConfigureClient();
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("offline_access openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var tokenResponse = await PerformCodeFlowTokenRequest(code, proof);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tokenResult = JsonElement.Parse(tokenJson);
        tokenResult.TryGetProperty("refresh_token", out var refreshToken).Should().BeTrue();

        var refreshResponse = await PerformRefreshTokenExchangeRequest(refreshToken.ToString(), null);
        
        var json = await refreshResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_WhenBound_AndProofDifferentPublicKey_ShouldReturnInvalidDPoPProof()
    {
        ConfigureClient();
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("offline_access openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var tokenResponse = await PerformCodeFlowTokenRequest(code, proof);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tokenResult = JsonElement.Parse(tokenJson);
        tokenResult.TryGetProperty("refresh_token", out var refreshToken).Should().BeTrue();

        var otherKey = DPoPProofGenerator.CreateDPoPKey();
        var refreshProof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: otherKey);
        
        var refreshResponse = await PerformRefreshTokenExchangeRequest(refreshToken.ToString(), refreshProof);
        
        var json = await refreshResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_WhenBound_AndProofSamePublicKey_ShouldValidateAndReturnAccessTokenWithCnfJktClaim()
    {
        ConfigureClient();
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var code = await GetAuthorizationCodeAsync("offline_access openid api", keyThumb);

        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var tokenResponse = await PerformCodeFlowTokenRequest(code, proof);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tokenResult = JsonElement.Parse(tokenJson);
        tokenResult.TryGetProperty("refresh_token", out var refreshToken).Should().BeTrue();

        var refreshProof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var refreshResponse = await PerformRefreshTokenExchangeRequest(refreshToken.ToString(), refreshProof);
        
        var json = await refreshResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetString("token_type").Should().Be("DPoP");
        result.TryGetString("access_token").Should().NotBeNullOrEmpty()
            .And.BeAccessToken()
            .WithClaim("cnf", $"{{\"jkt\":\"{keyThumb}\"}}");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WhenProofRequired_AndProofMissing_ShouldReturnInvalidDPoPProof()
    {
        ConfigureClient(grantTypes: GrantTypes.ClientCredentials, requireDPoP: true);
        
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", DPoPClient.ClientId },
            { "client_secret", "secret" },
        };

        var response = await PerformTokenRequest(data);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeTrue();
        result.TryGetString("error").Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_AndProofValid_ShouldValidateAndReturnAccessTokenWithCnfJktClaim()
    {
        ConfigureClient(grantTypes: GrantTypes.ClientCredentials, requireDPoP: true);
        
        var key = DPoPProofGenerator.CreateDPoPKey();
        var keyThumb = DPoPProofGenerator.Thumprint(key);
        
        var proof = dPoPProofGenerator.CreateDPoPProof(IdentityServerPipeline.TokenEndpoint, HttpMethod.Post.Method, iat: fakeNow.AddSeconds(10), key: key);
        
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", DPoPClient.ClientId },
            { "client_secret", "secret" },
        };

        var response = await PerformTokenRequest(data, proof);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetString("token_type").Should().Be("DPoP");
        result.TryGetString("access_token").Should().NotBeNullOrEmpty()
            .And.BeAccessToken()
            .WithClaim("cnf", $"{{\"jkt\":\"{keyThumb}\"}}");
    }
}