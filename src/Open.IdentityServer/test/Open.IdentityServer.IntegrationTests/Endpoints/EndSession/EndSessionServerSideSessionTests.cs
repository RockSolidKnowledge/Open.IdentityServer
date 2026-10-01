// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.EndSession;

public class EndSessionServerSideSessionTests: ServerSideSessionTests
{
    private const string Category = nameof(EndSessionServerSideSessionTests);

    [Fact]
    [Trait("Category", Category)]
    public async Task EndSession_ShouldRemoveSession()
    {
        sessionStore.Should().NotBeNull();
        
        await _mockPipeline.LoginAsync("bob");

        Cookie sessionCookie = _mockPipeline.GetSessionCookie();
        
        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();
        
        var storedSessionPreEndSession = await sessionStore.GetSession(authKey);
        storedSessionPreEndSession.Should().NotBeNull();
        storedSessionPreEndSession.SessionId.Should().Be(sessionCookie.Value);
        storedSessionPreEndSession.SubjectId.Should().Be("bob");
        
        await _mockPipeline.BrowserClient!.GetAsync(IdentityServerPipeline.EndSessionEndpoint, 
            TestContext.Current.CancellationToken);

        _mockPipeline.LogoutWasCalled.Should().BeTrue();
        _mockPipeline.LogoutRequest.Should().NotBeNull();
        
        var storedSessionPostEndSession = await sessionStore.GetSession(authKey);
        storedSessionPostEndSession.Should().BeNull();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task EndSession_WhenMultipleClients_ShouldRenderFrontChannelSignoutIframes()
    {
        ticketStore = _mockPipeline.GetTicketStore();

        await _mockPipeline.LoginAsync("bob");
        var sid = _mockPipeline.GetSessionCookie().Value;

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();
        
        // Perform Client Authorizations
        var (_, client1TokenResponse) = await AuthCodeAndTokenRequest(
            "client1",
            "openid profile api1 offline_access",
            "https://client1/callback");
        
        await AuthCodeAndTokenRequest(
            "client2", 
            "openid profile", 
            "https://client2/callback");

        var endSessionUrl = IdentityServerPipeline.EndSessionEndpoint +
                            "?id_token_hint=" + Uri.EscapeDataString(client1TokenResponse.IdentityToken!);
        
        // Validate End Session Endpoint Behaviour
        await _mockPipeline.BrowserClient.GetAsync(endSessionUrl, TestContext.Current.CancellationToken);

        _mockPipeline.LogoutWasCalled.Should().BeTrue();
        _mockPipeline.LogoutRequest.Should().NotBeNull();
        _mockPipeline.LogoutRequest.SignOutIFrameUrl.Should().NotBeNull();

        var signoutFrameResponse = await _mockPipeline.BrowserClient.GetAsync(
            _mockPipeline.LogoutRequest.SignOutIFrameUrl,
            TestContext.Current.CancellationToken);

        signoutFrameResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await signoutFrameResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        html.Should().Contain(HtmlEncoder.Default.Encode(
            "https://client1/signout?sid=" + sid + "&iss=" +
            UrlEncoder.Default.Encode(IdentityServerPipeline.BaseUrl)));
        html.Should().Contain(HtmlEncoder.Default.Encode(
            "https://client2/signout?sid=" + sid + "&iss=" +
            UrlEncoder.Default.Encode(IdentityServerPipeline.BaseUrl)));
    }
}