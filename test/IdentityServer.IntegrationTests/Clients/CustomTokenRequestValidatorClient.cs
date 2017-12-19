// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class CustomTokenRequestValidatorClient
    {
        private const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public CustomTokenRequestValidatorClient()
        {
            var val = new TestCustomTokenRequestValidator();
            Startup.CustomTokenRequestValidator = val;

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        public async Task client_credentials_request_should_contain_custom_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            var fields = GetFields(response);
            fields.Should().Contain("custom", "custom");
        }

        [Fact]
        public async Task resource_owner_credentials_request_should_contain_custom_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");

            var fields = GetFields(response);
            fields.Should().Contain("custom", "custom");
        }

        [Fact]
        public async Task refreshing_a_token_should_contain_custom_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "api1 offline_access");
            response = await client.RequestRefreshTokenAsync(response.RefreshToken);

            var fields = GetFields(response);
            fields.Should().Contain("custom", "custom");
        }

        [Fact]
        public async Task extension_grant_request_should_contain_custom_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("custom", "api1", customParameters);

            var fields = GetFields(response);
            fields.Should().Contain("custom", "custom");
        }

        private Dictionary<string, object> GetFields(TokenResponse response)
        {
            return response.Json.ToObject<Dictionary<string, object>>();
        }
    }
}