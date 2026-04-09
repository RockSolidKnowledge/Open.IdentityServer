// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel;
using Open.IdentityModel.Client;
using IdentityServer.IntegrationTests.Clients.Setup;
using IdentityServer.IntegrationTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients;

public class ClientAssertionClient : IDisposable
{
    private const string TokenEndpoint = "https://idsvr4/connect/token";
    private const string ClientId = "certificate_base64_valid";

    private readonly HttpClient _client;
    private readonly IHost _host;

    public ClientAssertionClient()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder.UseTestServer();
                builder.UseStartup<Startup>();
            })
            .Build();

        _host.Start();
        _client = _host.GetTestClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _host?.Dispose();
    }

    [Fact]
    public async Task Valid_client_with_manual_payload_should_succeed()
    {
        var token = CreateToken(ClientId);
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", ClientId },
            { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
            { "client_assertion", token },
            { "grant_type", "client_credentials" },
            { "scope", "api1" }
        });

        var response = await GetToken(requestBody);

        AssertValidToken(response);
    }

    [Fact]
    public async Task Valid_client_should_succeed()
    {
        var token = CreateToken(ClientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },
            ClientCredentialStyle = ClientCredentialStyle.PostBody,

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        AssertValidToken(response);
    }

    [Fact]
    public async Task Valid_client_with_implicit_clientId_should_succeed()
    {
        var token = CreateToken(ClientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",

            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },
            ClientCredentialStyle = ClientCredentialStyle.PostBody,

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        AssertValidToken(response);
    }

    [Fact]
    public async Task Valid_client_with_token_replay_should_fail()
    {
        var token = CreateToken(ClientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },
            ClientCredentialStyle = ClientCredentialStyle.PostBody,

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        AssertValidToken(response);

        // replay
        response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },
            ClientCredentialStyle = ClientCredentialStyle.PostBody,

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().BeTrue();
        response.Error.Should().Be("invalid_client");
    }

    [Fact]
    public async Task Client_with_invalid_secret_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = "invalid"
            },
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
    }

    [Fact]
    public async Task Invalid_client_should_fail()
    {
        const string clientId = "certificate_base64_invalid";
        var token = CreateToken(clientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = clientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
    }

    private async Task<TokenResponse> GetToken(FormUrlEncodedContent body)
    {
        var response = await _client.PostAsync(TokenEndpoint, body);
        return await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(response);
    }

    private void AssertValidToken(TokenResponse response)
    {
        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        payload.Count.Should().Be(8);
        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo(ClientId);
        payload.Keys.Should().Contain("iat");

        var scopes = ((JsonElement)payload["scope"]).EnumerateArray();
        scopes.First().ToString().Should().Be("api1");

        ((JsonElement)payload["aud"]).GetString().Should().BeEquivalentTo("api");
    }

    private Dictionary<string, object> GetPayload(TokenResponse response)
    {
        var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(
            Encoding.UTF8.GetString(Base64Url.Decode(token)));

        return dictionary;
    }

    private string CreateToken(string clientId, DateTime? nowOverride = null)
    {
        var certificate = TestCert.Load();
        var now = nowOverride ?? DateTime.UtcNow;

        var token = new JwtSecurityToken(
            clientId,
            TokenEndpoint,
            new List<Claim>()
            {
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            },
            now,
            now.AddMinutes(1),
            new SigningCredentials(
                new X509SecurityKey(certificate),
                SecurityAlgorithms.RsaSha256
            )
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}