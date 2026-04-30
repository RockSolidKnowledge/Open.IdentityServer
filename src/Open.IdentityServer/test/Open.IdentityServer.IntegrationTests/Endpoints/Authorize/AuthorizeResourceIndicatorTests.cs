// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel;
using Open.IdentityModel.Client;
using Open.IdentityServer.IntegrationTests.Endpoints;
using Open.IdentityServer.Test;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Authorize;

public class AuthorizeResourceIndicatorTests: ResourceIndicatorTests
{
    private const string Category = "Authorize endpoint - resource indicators";

    public AuthorizeResourceIndicatorTests()
    {
        mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "bob",
            Username = "bob",
            Claims =
            [
                new Claim("name", "Bob Loblaw"),
                new Claim("email", "bob@loblaw.com"),
            ]
        });
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithResourceIndicator_ShouldRedirectToLogin()
    {
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithResourceIndicator_AuthenticatedUser_ShouldSucceed()
    {
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("https://server/cb");

        var authorization = new AuthorizeResponse(response.Headers.Location.ToString());
        authorization.IsError.Should().BeFalse();
        authorization.Code.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithHttpsResourceIndicator_ShouldRedirectToLogin()
    {
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid valid:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "https://valid.resource.com")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithMultipleResourceIndicators_ShouldRedirectToLogin()
    {
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid urn:valid.resource:Read valid:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource"),
                new KeyValuePair<string, string>("resource", "https://valid.resource.com")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithInvalidResourceIndicator_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:invalid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();
        mockPipeline.ErrorMessage.Should().NotBeNull();
        mockPipeline.ErrorMessage!.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithResourceIndicator_ScopeMismatch_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid valid:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithUnauthorizedResourceIndicator_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid unauth:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:unauth.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task HybridFlow_WithResourceIndicator_ShouldRedirectToLogin()
    {
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: HybridClient.ClientId,
            responseType: "code id_token",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task HybridFlow_WithResourceIndicator_AuthenticatedUser_ShouldSucceed()
    {
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: HybridClient.ClientId,
            responseType: "code id_token",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("https://server/cb");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task HybridFlow_WithHttpsResourceIndicator_ShouldRedirectToLogin()
    {
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: HybridClient.ClientId,
            responseType: "code id_token",
            scope: "openid valid:Read",
            redirectUri: "https://server/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "https://valid.resource.com")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task HybridFlow_WithInvalidResourceIndicator_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: HybridClient.ClientId,
            responseType: "code id_token",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:invalid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();mockPipeline.ErrorMessage.Should().NotBeNull();
        mockPipeline.ErrorMessage!.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ImplicitFlow_WithResourceIndicator_ShouldRedirectToLogin()
    {
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: ImplicitClient.ClientId,
            responseType: "id_token token",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "oob://implicit/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ImplicitFlow_WithResourceIndicator_AuthenticatedUser_ShouldSucceed()
    {
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: ImplicitClient.ClientId,
            responseType: "id_token token",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "oob://implicit/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("oob://implicit/cb");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ImplicitFlow_WithHttpsResourceIndicator_ShouldRedirectToLogin()
    {
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: ImplicitClient.ClientId,
            responseType: "id_token token",
            scope: "openid valid:Read",
            redirectUri: "oob://implicit/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "https://valid.resource.com")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ImplicitFlow_WithInvalidResourceIndicator_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: ImplicitClient.ClientId,
            responseType: "id_token token",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "oob://implicit/cb",
            nonce: "123_nonce",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:invalid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();mockPipeline.ErrorMessage.Should().NotBeNull();
        mockPipeline.ErrorMessage!.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithResourceIndicator_NoResourceScopes_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithNonAbsoluteUriResourceIndicator_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "not-a-uri")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithResourceIndicatorContainingFragment_ShouldShowError()
    {
        await mockPipeline.LoginAsync("bob");

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid urn:valid.resource:Read",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "https://valid.resource.com#fragment")
            ]));

        await mockPipeline.BrowserClient!.GetAsync(url, TestContext.Current.CancellationToken);
        mockPipeline.ErrorWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WithMultipleScopes_ForSingleResource_ShouldSucceed()
    {
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: CodeClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid urn:valid.resource:Read urn:valid.resource:Write urn:valid.resource:All",
            redirectUri: "https://server/cb",
            extra: new Parameters([
                new KeyValuePair<string, string>("resource", "urn:valid.resource")
            ]));

        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("https://server/cb");

        var authorization = new AuthorizeResponse(response.Headers.Location.ToString());
        authorization.IsError.Should().BeFalse();
        authorization.Code.Should().NotBeNull();
    }
}