// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using Open.IdentityModel;
using Open.IdentityModel.Client;
using Open.IdentityServer.IntegrationTests.Endpoints;
using Open.IdentityServer.Test;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Token;

public class TokenResourceIndicatorTests : ResourceIndicatorTests
{
    private const string Category = "Token endpoint - resource indicators";

    public TokenResourceIndicatorTests()
    {
        mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "bob",
            Username = "bob",
            Password = "password",
            Claims =
            [
                new Claim("name", "Bob Loblaw"),
                new Claim("email", "bob@loblaw.com"),
            ]
        });
    }

    private async Task<string> GetAuthorizationCodeAsync(string scope, string resource)
    {
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: scope,
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", resource)
            ]));

        var response = await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        var authorization = new AuthorizeResponse(response.Headers.Location!.ToString());
        return authorization.Code!;
    }

    private async Task<string> GetAuthorizationCodeWithMultipleResourcesAsync(string scope, params string[] resources)
    {
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var parameters = new List<KeyValuePair<string, string>>();
        foreach (var resource in resources)
        {
            parameters.Add(new KeyValuePair<string, string>("resource", resource));
        }

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: scope,
            redirectUri: "https://server/cb",
            extra: new Parameters(parameters));

        var response = await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        var authorization = new AuthorizeResponse(response.Headers.Location!.ToString());
        return authorization.Code!;
    }

    // Client Credentials with Resource Indicator

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithResourceIndicator_ShouldSucceed()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "urn:valid.resource:Read" },
            { "resource", "urn:valid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithHttpsResourceIndicator_ShouldSucceed()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "valid:Read" },
            { "resource", "https://valid.resource.com" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithMultipleScopes_ForSingleResource_ShouldSucceed()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "urn:valid.resource:Read urn:valid.resource:Write urn:valid.resource:All" },
            { "resource", "urn:valid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithInvalidResourceIndicator_ShouldFail()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "urn:valid.resource:Read" },
            { "resource", "urn:invalid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out var error).Should().BeTrue();
        error.GetString().Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithResourceIndicator_ScopeMismatch_ShouldFail()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "valid:Read" },
            { "resource", "urn:valid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithNonAbsoluteUriResourceIndicator_ShouldFail()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "urn:valid.resource:Read" },
            { "resource", "not-a-uri" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithResourceIndicatorContainingFragment_ShouldFail()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "valid:Read" },
            { "resource", "https://valid.resource.com#fragment" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeTrue();
    }

    // Authorization Code exchange with Resource Indicator

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeExchange_WithResourceIndicator_ShouldSucceed()
    {
        var code = await GetAuthorizationCodeAsync("openid urn:valid.resource:Read", "urn:valid.resource");

        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "urn:valid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeExchange_WithHttpsResourceIndicator_ShouldSucceed()
    {
        var code = await GetAuthorizationCodeAsync("openid valid:Read", "https://valid.resource.com");

        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "https://valid.resource.com" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeExchange_WithResourceIndicator_NotInOriginalRequest_ShouldFail()
    {
        var code = await GetAuthorizationCodeAsync("openid urn:valid.resource:Read", "urn:valid.resource");

        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "https://valid.resource.com" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeExchange_WithMultipleResources_SingleResourceAtToken_ShouldSucceed()
    {
        var code = await GetAuthorizationCodeWithMultipleResourcesAsync(
            "openid urn:valid.resource:Read valid:Read",
            "urn:valid.resource", "https://valid.resource.com");

        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "urn:valid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeExchange_WithInvalidResourceIndicator_ShouldFail()
    {
        var code = await GetAuthorizationCodeAsync("openid urn:valid.resource:Read", "urn:valid.resource");

        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "urn:invalid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeTrue();
    }

    // Refresh Token with Resource Indicator

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_WithResourceIndicator_ShouldSucceed()
    {
        var code = await GetAuthorizationCodeAsync("openid offline_access urn:valid.resource:Read", "urn:valid.resource");

        var tokenData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "urn:valid.resource" },
        };

        var tokenResponse = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(tokenData),
            TestContext.Current.CancellationToken);

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tokenResult = JsonElement.Parse(tokenJson);
        tokenResult.TryGetProperty("refresh_token", out var refreshToken).Should().BeTrue();

        var refreshData = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", CodeClient.ClientId },
            { "refresh_token", refreshToken.GetString()! },
            { "resource", "urn:valid.resource" },
        };

        var refreshResponse = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(refreshData),
            TestContext.Current.CancellationToken);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshJson = await refreshResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var refreshResult = JsonElement.Parse(refreshJson);
        refreshResult.TryGetProperty("error", out _).Should().BeFalse();
        refreshResult.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_WithDifferentResourceIndicator_ShouldFail()
    {
        var code = await GetAuthorizationCodeAsync("openid offline_access urn:valid.resource:Read", "urn:valid.resource");

        var tokenData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "urn:valid.resource" },
        };

        var tokenResponse = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(tokenData),
            TestContext.Current.CancellationToken);

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tokenResult = JsonElement.Parse(tokenJson);
        tokenResult.TryGetProperty("refresh_token", out var refreshToken).Should().BeTrue();

        var refreshData = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", CodeClient.ClientId },
            { "refresh_token", refreshToken.GetString()! },
            { "resource", "https://valid.resource.com" },
        };

        var refreshResponse = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(refreshData),
            TestContext.Current.CancellationToken);

        var refreshJson = await refreshResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var refreshResult = JsonElement.Parse(refreshJson);
        refreshResult.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_WithInvalidResourceIndicator_ShouldFail()
    {
        var code = await GetAuthorizationCodeAsync("openid offline_access urn:valid.resource:Read", "urn:valid.resource");

        var tokenData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", CodeClient.ClientId },
            { "code", code },
            { "redirect_uri", "https://server/cb" },
            { "resource", "urn:valid.resource" },
        };

        var tokenResponse = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(tokenData),
            TestContext.Current.CancellationToken);

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tokenResult = JsonElement.Parse(tokenJson);
        tokenResult.TryGetProperty("refresh_token", out var refreshToken).Should().BeTrue();

        var refreshData = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", CodeClient.ClientId },
            { "refresh_token", refreshToken.GetString()! },
            { "resource", "urn:invalid.resource" },
        };

        var refreshResponse = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(refreshData),
            TestContext.Current.CancellationToken);

        var refreshJson = await refreshResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var refreshResult = JsonElement.Parse(refreshJson);
        refreshResult.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithNoScopes_AndResourceIndicator_ShouldSucceedWithAllResourceScopes()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "resource", "urn:valid.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeFalse();
        result.TryGetProperty("access_token", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithUnauthorizedResourceIndicator_ShouldFail()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientCredentialsClient.ClientId },
            { "client_secret", "secret" },
            { "scope", "unauth:Read" },
            { "resource", "urn:unauth.resource" },
        };

        var response = await mockPipeline.BackChannelClient!.PostAsync(
            IdentityServerPipeline.TokenEndpoint,
            new FormUrlEncodedContent(data),
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonElement.Parse(json);
        result.TryGetProperty("error", out _).Should().BeTrue();
    }
}