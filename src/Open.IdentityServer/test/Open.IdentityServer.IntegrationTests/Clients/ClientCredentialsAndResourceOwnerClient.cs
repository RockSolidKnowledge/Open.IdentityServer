// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel.Client;
using IdentityServer.IntegrationTests.Clients.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients;

public class ClientCredentialsAndResourceOwnerClient : IDisposable
{
    private const string TokenEndpoint = "https://server/connect/token";

    private readonly HttpClient _client;
    private readonly IHost _host;

    public ClientCredentialsAndResourceOwnerClient()
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
        _client.Dispose();
        _host.Dispose();
    }

    [Fact]
    public async Task Resource_scope_should_be_requestable_via_client_credentials()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.and.ro",
            ClientSecret = "secret",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
    }

    [Fact]
    public async Task Openid_scope_should_not_be_requestable_via_client_credentials()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.and.ro",
            ClientSecret = "secret",
            Scope = "openid api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
    }

    [Fact]
    public async Task Openid_scope_should_be_requestable_via_password()
    {
        var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.and.ro",
            ClientSecret = "secret",
            Scope = "openid",

            UserName = "bob",
            Password = "bob"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
    }

    [Fact]
    public async Task Openid_and_resource_scope_should_be_requestable_via_password()
    {
        var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "client.and.ro",
            ClientSecret = "secret",
            Scope = "openid api1",

            UserName = "bob",
            Password = "bob"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(false);
    }
}