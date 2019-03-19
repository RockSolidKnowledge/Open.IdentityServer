// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class ExtensionGrantClient
    {
        private const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;

        public ExtensionGrantClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _client = server.CreateClient();
        }

        [Fact]
        public async Task Valid_client_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "custom",

                ClientId = "client.custom",
                ClientSecret = "secret",

                Parameters =
                {
                    { "custom_credential", "custom credential"},
                    { "scope", "api1" }
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            var unixNow = DateTime.UtcNow.ToEpochTime();
            var exp = Int64.Parse(payload["exp"].ToString());
            exp.Should().BeLessThan(unixNow + 3605);
            exp.Should().BeGreaterThan(unixNow + 3595);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.custom");
            payload.Should().Contain("sub", "818727");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("custom");
        }

        [Fact]
        public async Task Valid_client_no_subject_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "custom.nosubject",

                ClientId = "client.custom",
                ClientSecret = "secret",

                Parameters =
                {
                    { "custom_credential", "custom credential"},
                    { "scope", "api1" }
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(6);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.custom");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");
        }

        [Fact]
        public async Task Valid_client_with_default_scopes_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "custom",

                ClientId = "client.custom",
                ClientSecret = "secret",

                Parameters =
                {
                    { "custom_credential", "custom credential"}
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.custom");
            payload.Should().Contain("sub", "818727");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("custom");

            var scopes = payload["scope"] as JArray;
            scopes.Count().Should().Be(2);
            scopes.First().ToString().Should().Be("api1");
            scopes.Skip(1).First().ToString().Should().Be("api2");
        }

        [Fact]
        public async Task Valid_client_missing_grant_specific_data_should_fail()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "custom",

                ClientId = "client.custom",
                ClientSecret = "secret",

                Parameters =
                {
                    { "scope", "api1" }
                }
            });

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
            response.ErrorDescription.Should().Be("invalid_custom_credential");
        }

        [Fact]
        public async Task Valid_client_using_unsupported_grant_type_should_fail()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "invalid",

                ClientId = "client.custom",
                ClientSecret = "secret",

                Parameters =
                {
                    { "custom_credential", "custom credential"},
                    { "scope", "api1" }
                }
            });

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("unsupported_grant_type");
        }

        [Fact]
        public async Task Valid_client_using_unauthorized_grant_type_should_fail()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "custom2",

                ClientId = "client.custom",
                ClientSecret = "secret",

                Parameters =
                {
                    { "custom_credential", "custom credential"},
                    { "scope", "api1" }
                }
            });

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("unsupported_grant_type");
        }

        [Fact]
        public async Task Dynamic_lifetime_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "dynamic",

                ClientId = "client.dynamic",
                ClientSecret = "secret",

                Parameters =
                {
                    { "scope", "api1" },

                    { "lifetime", "5000"},
                    { "sub",  "818727"}
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(5000);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            var unixNow = DateTime.UtcNow.ToEpochTime();
            var exp = Int64.Parse(payload["exp"].ToString());
            exp.Should().BeLessThan(unixNow + 5005);
            exp.Should().BeGreaterThan(unixNow + 4995);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.dynamic");
            payload.Should().Contain("sub", "818727");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("delegation");
        }

        [Fact]
        public async Task Dynamic_token_type_jwt_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "dynamic",

                ClientId = "client.dynamic",
                ClientSecret = "secret",

                Parameters =
                {
                    { "scope", "api1" },

                    { "type", "jwt"},
                    { "sub",  "818727"}
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            response.AccessToken.Should().Contain(".");
        }

        [Fact]
        public async Task Dynamic_token_type_reference_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "dynamic",

                ClientId = "client.dynamic",
                ClientSecret = "secret",

                Parameters =
                {
                    { "scope", "api1" },

                    { "type", "reference"},
                    { "sub",  "818727"}
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            response.AccessToken.Should().NotContain(".");
        }

        [Fact]
        public async Task Dynamic_client_claims_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "dynamic",

                ClientId = "client.dynamic",
                ClientSecret = "secret",

                Parameters =
                {
                    { "scope", "api1" },

                    { "claim", "extra_claim"},
                    { "sub",  "818727"}
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(11);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.dynamic");
            payload.Should().Contain("sub", "818727");
            payload.Should().Contain("idp", "local");

            payload.Should().Contain("client_extra", "extra_claim");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("delegation");
        }

        [Fact]
        public async Task Dynamic_client_claims_no_sub_should_succeed()
        {
            var response = await _client.RequestTokenAsync(new TokenRequest
            {
                Address = TokenEndpoint,
                GrantType = "dynamic",

                ClientId = "client.dynamic",
                ClientSecret = "secret",

                Parameters =
                {
                    { "scope", "api1" },

                    { "claim", "extra_claim"},
                }
            });

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(7);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.dynamic");
            payload.Should().Contain("client_extra", "extra_claim");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");
        }

        private Dictionary<string, object> GetPayload(TokenResponse response)
        {
            var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Encoding.UTF8.GetString(Base64Url.Decode(token)));

            return dictionary;
        }
    }
}