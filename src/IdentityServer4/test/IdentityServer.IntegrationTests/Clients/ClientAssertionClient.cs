// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using IdentityServer4.IntegrationTests.Common;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class ClientAssertionClient
    {
        private const string TokenEndpoint = "https://idsvr4/connect/token";
        private const string ClientId = "certificate_base64_valid";

        private readonly HttpClient _client;

        public ClientAssertionClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _client = server.CreateClient();
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

                Scope = "api1"
            });

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

                Scope = "api1"
            });

            AssertValidToken(response);
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

                Scope = "api1"
            });

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

                Scope = "api1"
            });

            response.IsError.Should().Be(true);
            response.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        }

        private async Task<TokenResponse> GetToken(FormUrlEncodedContent body)
        {
            var response = await _client.PostAsync(TokenEndpoint, body);
            return new TokenResponse(await response.Content.ReadAsStringAsync());
        }

        private void AssertValidToken(TokenResponse response)
        {
            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);
            
            payload.Count().Should().Be(6);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", ClientId);
            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");
        }

        private Dictionary<string, object> GetPayload(TokenResponse response)
        {
            var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
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
                        new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
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
}