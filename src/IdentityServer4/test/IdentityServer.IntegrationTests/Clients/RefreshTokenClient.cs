// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class RefreshTokenClient
    {
        private const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;

        public RefreshTokenClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _client = server.CreateClient();
        }

        [Fact]
        public async Task Requesting_a_refresh_token_without_identity_scopes_should_return_expected_results()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                Scope = "api1 offline_access",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().NotBeNull();

            response = await _client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                RefreshToken = response.RefreshToken
            });

            response.IsError.Should().BeFalse();
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().NotBeNull();
        }

        [Fact]
        public async Task Requesting_a_refresh_token_with_identity_scopes_should_return_expected_results()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                Scope = "openid api1 offline_access",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().NotBeNull();

            response = await _client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                RefreshToken = response.RefreshToken
            });

            response.IsError.Should().BeFalse();
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().NotBeNull();
            response.RefreshToken.Should().NotBeNull();
        }

        [Fact]
        public async Task Refreshing_a_refresh_token_with_reuse_should_return_same_refresh_token()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient.reuse",
                ClientSecret = "secret",

                Scope = "openid api1 offline_access",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().NotBeNull();

            var rt1 = response.RefreshToken;

            response = await _client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient.reuse",
                ClientSecret = "secret",

                RefreshToken = response.RefreshToken
            });

            response.IsError.Should().BeFalse();
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().NotBeNull();
            response.RefreshToken.Should().NotBeNull();

            var rt2 = response.RefreshToken;

            rt1.Should().BeEquivalentTo(rt2);
        }
    }
}