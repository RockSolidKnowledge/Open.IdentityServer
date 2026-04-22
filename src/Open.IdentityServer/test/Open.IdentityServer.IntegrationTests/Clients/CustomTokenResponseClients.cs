// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel;
using Open.IdentityModel.Client;
using IdentityServer.IntegrationTests.Clients.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients;

public class CustomTokenResponseClients : IDisposable
{
    private const string TokenEndpoint = "https://server/connect/token";

    private readonly HttpClient _client;
    private readonly IHost _host;

    public CustomTokenResponseClients()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseStartup<StartupWithCustomTokenResponses>();
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
    public async Task Resource_owner_success_should_return_custom_response()
    {
        var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "roclient",
            ClientSecret = "secret",

            UserName = "bob",
            Password = "bob",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        // raw fields
        var fields = GetFields(response);
        (fields["string_value"] as JsonElement?)?.GetString().Should().BeEquivalentTo("some_string");
        (fields["int_value"] as JsonElement?)?.GetInt64().Should().Be(42);

        object temp;
        fields.TryGetValue("identity_token", out temp).Should().BeFalse();
        fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
        fields.TryGetValue("error", out temp).Should().BeFalse();
        fields.TryGetValue("error_description", out temp).Should().BeFalse();
        fields.TryGetValue("token_type", out temp).Should().BeTrue();
        fields.TryGetValue("expires_in", out temp).Should().BeTrue();

        var responseObject = fields["dto"] as JsonElement?;
        responseObject.Should().NotBeNull();

        var responseDto = GetDto(responseObject);
        var dto = CustomResponseDto.Create;

        responseDto.string_value.Should().Be(dto.string_value);
        responseDto.int_value.Should().Be(dto.int_value);
        responseDto.nested.string_value.Should().Be(dto.nested.string_value);
        responseDto.nested.int_value.Should().Be(dto.nested.int_value);


        // token client response
        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();
            

        // token content
        var payload = GetPayload(response);
        payload.Count.Should().Be(12);
        (payload["iss"] as JsonElement?)?.GetString().Should().BeEquivalentTo("https://idsvr4");
        (payload["client_id"] as JsonElement?)?.GetString().Should().BeEquivalentTo("roclient");
        (payload["sub"] as JsonElement?)?.GetString().Should().BeEquivalentTo("bob");
        (payload["idp"] as JsonElement?)?.GetString().Should().BeEquivalentTo("local");
        (payload["aud"] as JsonElement?)?.GetString().Should().BeEquivalentTo("api");
            
        var scopes = (payload["scope"] as JsonElement?)?.EnumerateArray();
        scopes.Should().NotBeNull();
        scopes?.First().ToString().Should().Be("api1");

        var amr = (payload["amr"] as JsonElement?)?.EnumerateArray();
        amr.Should().NotBeNull();
        amr?.Count().Should().Be(1);
        amr?.First().ToString().Should().Be("password");
    }

    [Fact]
    public async Task Resource_owner_failure_should_return_custom_error_response()
    {
        var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = TokenEndpoint,
            ClientId = "roclient",
            ClientSecret = "secret",

            UserName = "bob",
            Password = "invalid",
            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        // raw fields
        var fields = GetFields(response);
        (fields["string_value"] as JsonElement?)?.GetString().Should().BeEquivalentTo("some_string");
        (fields["int_value"] as JsonElement?)?.GetInt64().Should().Be(42);

        object temp;
        fields.TryGetValue("identity_token", out temp).Should().BeFalse();
        fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
        fields.TryGetValue("error", out temp).Should().BeTrue();
        fields.TryGetValue("error_description", out temp).Should().BeTrue();
        fields.TryGetValue("token_type", out temp).Should().BeFalse();
        fields.TryGetValue("expires_in", out temp).Should().BeFalse();

        var responseObject = fields["dto"] as JsonElement?;
        responseObject.Should().NotBeNull();

        var responseDto = GetDto(responseObject);
        var dto = CustomResponseDto.Create;

        responseDto.string_value.Should().Be(dto.string_value);
        responseDto.int_value.Should().Be(dto.int_value);
        responseDto.nested.string_value.Should().Be(dto.nested.string_value);
        responseDto.nested.int_value.Should().Be(dto.nested.int_value);


        // token client response
        response.IsError.Should().Be(true);
        response.Error.Should().Be("invalid_grant");
        response.ErrorDescription.Should().Be("invalid_credential");
        response.ExpiresIn.Should().Be(0);
        response.TokenType.Should().BeNull();
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task Extension_grant_success_should_return_custom_response()
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
                { "outcome", "succeed"}
            }
        }, TestContext.Current.CancellationToken);


        // raw fields
        var fields = GetFields(response);
        (fields["string_value"] as JsonElement?)?.GetString().Should().BeEquivalentTo("some_string");
        (fields["int_value"] as JsonElement?)?.GetInt64().Should().Be(42);

        object temp;
        fields.TryGetValue("identity_token", out temp).Should().BeFalse();
        fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
        fields.TryGetValue("error", out temp).Should().BeFalse();
        fields.TryGetValue("error_description", out temp).Should().BeFalse();
        fields.TryGetValue("token_type", out temp).Should().BeTrue();
        fields.TryGetValue("expires_in", out temp).Should().BeTrue();

        var responseObject = fields["dto"] as JsonElement?;
        responseObject.Should().NotBeNull();

        var responseDto = GetDto(responseObject);
        var dto = CustomResponseDto.Create;

        responseDto.string_value.Should().Be(dto.string_value);
        responseDto.int_value.Should().Be(dto.int_value);
        responseDto.nested.string_value.Should().Be(dto.nested.string_value);
        responseDto.nested.int_value.Should().Be(dto.nested.int_value);


        // token client response
        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();


        // token content
        var payload = GetPayload(response);
        payload.Count.Should().Be(12);
        (payload["iss"] as JsonElement?)?.GetString().Should().BeEquivalentTo("https://idsvr4");
        (payload["client_id"] as JsonElement?)?.GetString().Should().BeEquivalentTo("client.custom");
        (payload["sub"] as JsonElement?)?.GetString().Should().BeEquivalentTo("bob");
        (payload["idp"] as JsonElement?)?.GetString().Should().BeEquivalentTo("local");
        (payload["aud"] as JsonElement?)?.GetString().Should().BeEquivalentTo("api");
            
        var scopes = (payload["scope"] as JsonElement?)?.EnumerateArray();
        scopes.Should().NotBeNull();
        scopes?.First().ToString().Should().Be("api1");

        var amr = (payload["amr"] as JsonElement?)?.EnumerateArray();
        amr.Should().NotBeNull();
        amr?.Count().Should().Be(1);
        amr?.First().ToString().Should().Be("custom");
    }

    [Fact]
    public async Task Extension_grant_failure_should_return_custom_error_response()
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
                { "outcome", "fail"}
            }
        }, TestContext.Current.CancellationToken);
            
        // raw fields
        var fields = GetFields(response);
            
        (fields["string_value"] as JsonElement?)?.GetString().Should().BeEquivalentTo("some_string");
        (fields["int_value"] as JsonElement?)?.GetInt64().Should().Be(42);

        object temp;
        fields.TryGetValue("identity_token", out temp).Should().BeFalse();
        fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
        fields.TryGetValue("error", out temp).Should().BeTrue();
        fields.TryGetValue("error_description", out temp).Should().BeTrue();
        fields.TryGetValue("token_type", out temp).Should().BeFalse();
        fields.TryGetValue("expires_in", out temp).Should().BeFalse();

        var responseObject = fields["dto"] as JsonElement?;
        responseObject.Should().NotBeNull();

        var responseDto = GetDto(responseObject);
        var dto = CustomResponseDto.Create;

        responseDto.string_value.Should().Be(dto.string_value);
        responseDto.int_value.Should().Be(dto.int_value);
        responseDto.nested.string_value.Should().Be(dto.nested.string_value);
        responseDto.nested.int_value.Should().Be(dto.nested.int_value);

        // token client response
        response.IsError.Should().Be(true);
        response.Error.Should().Be("invalid_grant");
        response.ErrorDescription.Should().Be("invalid_credential");
        response.ExpiresIn.Should().Be(0);
        response.TokenType.Should().BeNull();
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();
    }

    private CustomResponseDto GetDto(JsonElement? responseObject)
    {
        return responseObject?.Deserialize<CustomResponseDto>();;
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

    private Dictionary<string, object> GetPayload(TokenResponse response)
    {
        var token = response.AccessToken!.Split('.').Skip(1).Take(1).First();
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(
            Encoding.UTF8.GetString(Base64Url.Decode(token)));

        return dictionary;
    }
}