// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.Authorize;

public class AuthorizeDPoPTests: DPoPTests
{
    private const string Category = "Authorize Endpoint - DPoP";

    private const string ValidJkt = "0ZcOCORZNYy-DWpqq30jZyJGHTN0d2HglBV3uiguA4I";
    
    [Fact]
    [Trait("Category", Category)]
    public async Task CodeFlow_WhenDPoPJktProvided_ShouldBindAuthCodeToKey()
    {
        ConfigureClient();
        
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: DPoPClient.ClientId,
            responseType: "code",
            responseMode: "query",
            scope: "openid api",
            redirectUri: "https://server/cb",
            extra:
            [
                new KeyValuePair<string, string>("dpop_jkt", ValidJkt)
            ]);

        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("https://server/cb");

        var authorization = new AuthorizeResponse(response.Headers.Location.ToString());
        authorization.IsError.Should().BeFalse();
        authorization.Code.Should().NotBeNull();

        var codeModel = await authorizationCodeStore.GetAuthorizationCodeAsync(authorization.Code);
        codeModel.Should().NotBeNull();
        codeModel.DPoPKeyThumbprint.Should().Be(ValidJkt);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task HybridFlow_WhenDPoPJktProvided_ShouldBindAuthCodeToKey()
    {
        ConfigureClient(grantTypes: GrantTypes.Hybrid);
        
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: DPoPClient.ClientId,
            responseType: "code id_token",
            scope: "openid api",
            nonce: "123_nonce",
            redirectUri: "https://server/cb",
            extra:
            [
                new KeyValuePair<string, string>("dpop_jkt", ValidJkt)
            ]);

        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("https://server/cb");

        var authorization = new AuthorizeResponse(response.Headers.Location.ToString());
        authorization.IsError.Should().BeFalse();
        authorization.Code.Should().NotBeNull();

        var codeModel = await authorizationCodeStore.GetAuthorizationCodeAsync(authorization.Code);
        codeModel.Should().NotBeNull();
        codeModel.DPoPKeyThumbprint.Should().Be(ValidJkt);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ImplicitFlow_WhenDPoPJktProvided_ShouldBeIgnored()
    {
        ConfigureClient(grantTypes: GrantTypes.Implicit, allowTokensViaBrowser: true);
        
        await mockPipeline.LoginAsync("bob");
        mockPipeline.BrowserClient!.AllowAutoRedirect = false;

        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: DPoPClient.ClientId,
            responseType: "id_token token",
            scope: "openid api",
            redirectUri: "oob://implicit/cb",
            nonce: "123_nonce",
            extra:
            [
                new KeyValuePair<string, string>("dpop_jkt", ValidJkt)
            ]);

        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("oob://implicit/cb");

        var fragments = response.Headers.Location.Fragment.ParseFragment();

        fragments.Should().ContainKey("token_type").WhoseValue.Should().Be("Bearer");
        fragments.Should().ContainKey("access_token");
    }
}