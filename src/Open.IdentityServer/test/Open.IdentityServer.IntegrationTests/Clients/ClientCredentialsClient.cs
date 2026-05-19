// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Clients.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using IdentityServer.IntegrationTests.Utility;
using Open.IdentityServer;
using Open.IdentityServer.Utility;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients;

public class ClientCredentialsClient : IDisposable
{
    private const string TokenEndpoint = "https://server/connect/token";

    private readonly HttpClient _client;
    private readonly IHost _host;

    public ClientCredentialsClient()
    {
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
    public async Task Invalid_endpoint_should_return_404()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint + "invalid",
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Http);
        response.Error.Should().Be("Not Found");
        response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Valid_request_single_audience_should_return_expected_payload()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        payload.Count.Should().Be(8);
        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo("client");
        ((JsonElement)payload["aud"]).GetString().Should().BeEquivalentTo("api");
        payload.Keys.Should().Contain("jti");
        payload.Keys.Should().Contain("iat");

        var scopes = ((JsonElement)payload["scope"]).EnumerateArray();
        scopes.First().ToString().Should().Be("api1");
    }

    [Fact]
    public async Task Valid_request_multiple_audiences_should_return_expected_payload()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1 other_api"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        payload.Count.Should().Be(8);
        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo("client");
        payload.Keys.Should().Contain("jti");
        payload.Keys.Should().Contain("iat");

        var audiences = ((JsonElement)payload["aud"]).EnumerateArray().Select(x => x.ToString()).ToList();
        audiences.Count.Should().Be(2);
        audiences.Should().Contain("api");
        audiences.Should().Contain("other_api");

        var scopes = (JsonElement)payload["scope"];
        scopes.EnumerateArray().First().ToString().Should().Be("api1");
    }

    [Fact]
    public async Task Valid_request_with_confirmation_should_return_expected_payload()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.cnf",
            ClientSecret = "foo",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        payload.Count.Should().Be(9);
        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo("client.cnf");
        ((JsonElement)payload["aud"]).GetString().Should().BeEquivalentTo("api");
        payload.Keys.Should().Contain("jti");
        payload.Keys.Should().Contain("iat");

        var scopes = ((JsonElement)payload["scope"]).EnumerateArray();
        scopes.First().ToString().Should().Be("api1");

        var cnf = ((JsonElement)payload["cnf"]).Deserialize<Dictionary<string, string>>();
        cnf["x5t#S256"].Should().Be("foo");
    }

    [Fact]
    public async Task Requesting_multiple_scopes_should_return_expected_payload()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1 api2"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        payload.Count.Should().Be(8);
        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo("client");
        ((JsonElement)payload["aud"]).GetString().Should().BeEquivalentTo("api");
        payload.Keys.Should().Contain("jti");
        payload.Keys.Should().Contain("iat");

        var scopes = ((JsonElement)payload["scope"]).EnumerateArray().Select(x => x.GetString()).ToList();
        scopes.Count.Should().Be(2);
        scopes.First().Should().Be("api1");
        scopes.Skip(1).First().Should().Be("api2");
    }

    [Fact]
    public async Task Request_with_no_explicit_scopes_should_return_expected_payload()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        payload.Count.Should().Be(8);
        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo("client");
        payload.Keys.Should().Contain("jti");
        payload.Keys.Should().Contain("iat");

        var audiences = ((JsonElement)payload["aud"]).EnumerateArray().Select(x => x.ToString()).ToList();
        audiences.Count.Should().Be(2);
        audiences.Should().Contain("api");
        audiences.Should().Contain("other_api");

        var scopes = ((JsonElement)payload["scope"]).EnumerateArray().Select(x => x.ToString()).ToList();
        scopes.Count.Should().Be(3);
        scopes.Should().Contain("api1");
        scopes.Should().Contain("api2");
        scopes.Should().Contain("other_api");
    }

    [Fact]
    public async Task Client_without_default_scopes_skipping_scope_parameter_should_return_error()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.no_default_scopes",
            ClientSecret = "secret"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ExpiresIn.Should().Be(0);
        response.TokenType.Should().BeNull();
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();
        response.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
    }

    [Fact]
    public async Task Request_posting_client_secret_in_body_should_succeed()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1",

            ClientCredentialStyle = ClientCredentialStyle.PostBody
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo("client");
        ((JsonElement)payload["aud"]).GetString().Should().BeEquivalentTo("api");

        var scopes = ((JsonElement)payload["scope"]).EnumerateArray();
        scopes.First().ToString().Should().Be("api1");
    }


    [Fact]
    public async Task Request_For_client_with_no_secret_and_basic_authentication_should_succeed()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.no_secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);

        ((JsonElement)payload["iss"]).GetString().Should().BeEquivalentTo("https://idsvr4");
        ((JsonElement)payload["client_id"]).GetString().Should().BeEquivalentTo("client.no_secret");
        ((JsonElement)payload["aud"]).GetString().Should().BeEquivalentTo("api");

        var scopes = ((JsonElement)payload["scope"]).EnumerateArray();
        scopes.First().ToString().Should().Be("api1");
    }

    [Fact]
    public async Task Request_with_invalid_client_secret_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "invalid",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.Error.Should().Be("invalid_client");
    }

    [Fact]
    public async Task Unknown_client_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "invalid",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("invalid_client");
    }

    [Fact]
    public async Task Implicit_client_should_not_use_client_credential_grant()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "implicit",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("unauthorized_client");
    }

    [Fact]
    public async Task Implicit_and_client_creds_client_should_not_use_client_credential_grant_without_secret()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "implicit_and_client_creds",
            ClientSecret = "invalid",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("invalid_client");
    }


    [Fact]
    public async Task Requesting_unknown_scope_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "unknown"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("invalid_scope");
    }

    [Fact]
    public async Task Client_explicitly_requesting_identity_scope_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.identityscopes",
            ClientSecret = "secret",
            Scope = "openid api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("invalid_scope");
    }

    [Fact]
    public async Task Client_explicitly_requesting_offline_access_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1 offline_access"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("invalid_scope");
    }

    [Fact]
    public async Task Requesting_unauthorized_scope_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api3"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("invalid_scope");
    }

    [Fact]
    public async Task Requesting_authorized_and_unauthorized_scopes_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1 api3"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error.Should().Be("invalid_scope");
    }

    private Dictionary<string, object> GetPayload(TokenResponse response)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(response.AccessToken);

        var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(
            Encoding.UTF8.GetString(Base64Url.Decode(token)));

        return dictionary;
    }
}