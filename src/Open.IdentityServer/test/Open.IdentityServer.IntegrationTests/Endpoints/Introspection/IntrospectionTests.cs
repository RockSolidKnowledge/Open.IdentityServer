// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel.Client;
using IdentityServer.IntegrationTests.Endpoints.Introspection.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Introspection;

public class IntrospectionTests : IDisposable
{
    private const string Category = "Introspection endpoint";
    private const string IntrospectionEndpoint = "https://server/connect/introspect";
    private const string TokenEndpoint = "https://server/connect/token";

    private readonly HttpClient _client;
    private readonly IHost _host;
    private readonly HttpMessageHandler _handler;

    public IntrospectionTests()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder => {
                webBuilder.UseTestServer();
                webBuilder.UseStartup<Startup>();
            })
            .Build();

        _host.Start();

        var server = _host.GetTestServer();
        _handler = server.CreateHandler();
        _client = _host.GetTestClient();
    }

    public void Dispose()
    {
        _host?.Dispose();
        _client?.Dispose();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Empty_request_should_fail()
    {
        var form = new Dictionary<string, string>();

        var response = await _client.PostAsync(
            IntrospectionEndpoint, 
            new FormUrlEncodedContent(form), 
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Unknown_scope_should_fail()
    {
        var form = new Dictionary<string, string>();

        _client.SetBasicAuthentication("unknown", "invalid");
        var response = await _client.PostAsync(
            IntrospectionEndpoint, 
            new FormUrlEncodedContent(form), 
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_scope_secret_should_fail()
    {
        var form = new Dictionary<string, string>();

        _client.SetBasicAuthentication("api1", "invalid");
        var response = await _client.PostAsync(
            IntrospectionEndpoint, 
            new FormUrlEncodedContent(form), 
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Missing_token_should_fail()
    {
        var form = new Dictionary<string, string>();

        _client.SetBasicAuthentication("api1", "secret");
        var response = await _client.PostAsync(
            IntrospectionEndpoint, 
            new FormUrlEncodedContent(form), 
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_token_should_fail()
    {
        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api1",
            ClientSecret = "secret",

            Token = "invalid"
        }, TestContext.Current.CancellationToken);

        introspectionResponse.IsActive.Should().Be(false);
        introspectionResponse.IsError.Should().Be(false);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_Content_type_should_fail()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client1",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        var data = new
        {
            client_id = "api1",
            client_secret = "secret",
            token = tokenResponse.AccessToken
        };
        var json = JsonSerializer.Serialize(data);

        var client = new HttpClient(_handler);
        var response = await client.PostAsync(
            IntrospectionEndpoint, 
            new StringContent(json, Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_token_and_valid_scope_should_succeed()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client1",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api1",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        introspectionResponse.IsActive.Should().Be(true);
        introspectionResponse.IsError.Should().Be(false);

        var scopes = from c in introspectionResponse.Claims
            where c.Type == "scope"
            select c;

        scopes.Count().Should().Be(1);
        scopes.First().Value.Should().Be("api1");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Response_data_should_be_valid_using_single_scope()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client1",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api1",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        var values = introspectionResponse.Json?.Deserialize<Dictionary<string, object>>();
            
        ((JsonElement)values["aud"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["iss"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["nbf"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["exp"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["client_id"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["active"]).ValueKind.Should().Be(JsonValueKind.True);
        ((JsonElement)values["scope"]).ValueKind.Should().Be(JsonValueKind.String);

        values["scope"].ToString().Should().Be("api1");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Response_data_with_user_authentication_should_be_valid_using_single_scope()
    {
        var tokenResponse = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "ro.client",
            ClientSecret = "secret",
            UserName = "bob",
            Password = "bob",

            Scope = "api1",
        }, TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api1",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        var values = introspectionResponse.Json?.Deserialize<Dictionary<string, object>>();
            
        ((JsonElement)values["aud"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["iss"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["nbf"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["exp"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["auth_time"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["client_id"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["sub"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["active"]).ValueKind.Should().Be(JsonValueKind.True);
        ((JsonElement)values["scope"]).ValueKind.Should().Be(JsonValueKind.String);

        values["scope"].ToString().Should().Be("api1");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Response_data_should_be_valid_using_multiple_scopes_multiple_audiences()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client1",
            ClientSecret = "secret",

            Scope = "api2 api3-a api3-b",
        }, TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api3",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        var values = introspectionResponse.Json?.Deserialize<Dictionary<string, object>>();

        values["aud"].GetType().Name.Should().Be("JsonElement");

        var audiences = ((JsonElement)values["aud"]).EnumerateArray();
        foreach (var aud in audiences)
        {
            aud.ValueKind.Should().Be(JsonValueKind.String);
        }

        ((JsonElement)values["iss"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["nbf"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["exp"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["client_id"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["active"]).ValueKind.Should().Be(JsonValueKind.True);
        ((JsonElement)values["scope"]).ValueKind.Should().Be(JsonValueKind.String);

        var scopes = values["scope"].ToString();
        scopes.Should().Be("api3-a api3-b");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Response_data_should_be_valid_using_multiple_scopes_single_audience()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client1",
            ClientSecret = "secret",

            Scope = "api3-a api3-b",
        }, TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api3",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        var values = introspectionResponse.Json?.Deserialize<Dictionary<string, object>>();
            
        ((JsonElement)values["aud"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["iss"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["nbf"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["exp"]).ValueKind.Should().Be(JsonValueKind.Number);
        ((JsonElement)values["client_id"]).ValueKind.Should().Be(JsonValueKind.String);
        ((JsonElement)values["active"]).ValueKind.Should().Be(JsonValueKind.True);
        ((JsonElement)values["scope"]).ValueKind.Should().Be(JsonValueKind.String);

        var scopes = values["scope"].ToString();
        scopes.Should().Be("api3-a api3-b");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Token_with_many_scopes_but_api_should_only_see_its_own_scopes()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client3",
            ClientSecret = "secret",

            Scope = "api1 api2 api3-a",
        }, TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api3",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        introspectionResponse.IsActive.Should().BeTrue();
        introspectionResponse.IsError.Should().BeFalse();

        var scopes = from c in introspectionResponse.Claims
            where c.Type == "scope"
            select c.Value;

        scopes.Count().Should().Be(1);
        scopes.First().Should().Be("api3-a");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_token_with_valid_multiple_scopes()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client1",
            ClientSecret = "secret",

            Scope = "api1 api2",
        }, TestContext.Current.CancellationToken);

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api1",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        introspectionResponse.IsActive.Should().Be(true);
        introspectionResponse.IsError.Should().Be(false);

        var scopes = from c in introspectionResponse.Claims
            where c.Type == "scope"
            select c;

        scopes.Count().Should().Be(1);
        scopes.First().Value.Should().Be("api1");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_token_with_invalid_scopes_should_fail()
    {
        var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client1",
            ClientSecret = "secret",

            Scope = "api1",
        }, TestContext.Current.CancellationToken);

        var introspectionResponse = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = IntrospectionEndpoint,
            ClientId = "api2",
            ClientSecret = "secret",

            Token = tokenResponse.AccessToken
        }, TestContext.Current.CancellationToken);

        introspectionResponse.IsActive.Should().Be(false);
        introspectionResponse.IsError.Should().Be(false);
    }
}