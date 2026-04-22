// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel.Client;
using IdentityServer.IntegrationTests.Clients.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients;

public class CustomTokenRequestValidatorClient : IDisposable
{
    private const string TokenEndpoint = "https://server/connect/token";

    private readonly HttpClient _client;
    private readonly IHost _host;

    public CustomTokenRequestValidatorClient()
    {
        var val = new TestCustomTokenRequestValidator();
        Startup.CustomTokenRequestValidator = val;

        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseStartup<Startup>();
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
    public async Task Client_credentials_request_should_contain_custom_response()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        var fields = GetFields(response);
        (fields["custom"] as JsonElement?)?.GetString().Should().BeEquivalentTo("custom");
    }

    [Fact]
    public async Task Resource_owner_credentials_request_should_contain_custom_response()
    {
        var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = "roclient",
            ClientSecret = "secret",
            Scope = "api1",

            UserName = "bob",
            Password = "bob"
        }, TestContext.Current.CancellationToken);

        var fields = GetFields(response);
        (fields["custom"] as JsonElement?)?.GetString().Should().BeEquivalentTo("custom");
    }

    [Fact]
    public async Task Refreshing_a_token_should_contain_custom_response()
    {
        var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = "roclient",
            ClientSecret = "secret",
            Scope = "api1 offline_access",

            UserName = "bob",
            Password = "bob"
        }, TestContext.Current.CancellationToken);

        response = await _client.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "roclient",
            ClientSecret = "secret",

            RefreshToken = response.RefreshToken
        }, TestContext.Current.CancellationToken);

        var fields = GetFields(response);
        (fields["custom"] as JsonElement?)?.GetString().Should().BeEquivalentTo("custom");
    }

    [Fact]
    public async Task Extension_grant_request_should_contain_custom_response()
    {
        var response = await _client.RequestTokenAsync(new TokenRequest
        {
            Address = TokenEndpoint,
            GrantType = "custom",

            ClientId = "client.custom",
            ClientSecret = "secret",

            Parameters =
            {
                { "scope", "api1" },
                { "custom_credential", "custom credential"}
            }
        }, TestContext.Current.CancellationToken);

        var fields = GetFields(response);
        (fields["custom"] as JsonElement?)?.GetString().Should().BeEquivalentTo("custom");
    }

    private Dictionary<string, object> GetFields(TokenResponse response)
    {
        var dictionary = new Dictionary<string, object>();

        if (response.Json.HasValue)
        {
            dictionary = response.Json.Value.Deserialize<Dictionary<string, object>>();
        }

        return dictionary;
    }
}