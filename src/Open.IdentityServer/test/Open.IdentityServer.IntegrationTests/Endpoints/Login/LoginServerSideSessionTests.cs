// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.Login;

public class LoginServerSideSessionTests : ServerSideSessionTests
{
    private const string Category = nameof(LoginServerSideSessionTests);

    [Fact]
    [Trait("Category", Category)]
    public async Task Login_ShouldCreateSessionInServerStore()
    {
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        Cookie sessionCookie = _mockPipeline.GetSessionCookie();

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        var storedSession = await sessionStore.GetSession(authKey);
        storedSession.Should().NotBeNull();
        storedSession.SessionId.Should().Be(sessionCookie.Value);
        storedSession.SubjectId.Should().Be("bob");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Login_WhenUserChanges_ShouldRemoveExistingSession()
    {
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        Cookie originalSessionCookie = _mockPipeline.GetSessionCookie();

        var originalAuthKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        originalAuthKey.Should().NotBeNull();

        var originalSession = await sessionStore.GetSession(originalAuthKey);
        originalSession.Should().NotBeNull();
        originalSession.SessionId.Should().Be(originalSessionCookie.Value);
        originalSession.SubjectId.Should().Be("bob");

        await _mockPipeline.LoginAsync("alice");

        var newAuthKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        newAuthKey.Should().Be(originalAuthKey);

        var updatedSession = await sessionStore.GetSession(originalAuthKey);
        updatedSession.Should().BeNull();

        _mockPipeline.LoginWasCalled = false;

        var authorizeUrl = _mockPipeline.CreateAuthorizeUrl(
            clientId: "client1",
            responseType: "code",
            scope: "openid",
            redirectUri: "https://client1/callback");

        await _mockPipeline.BrowserClient!.GetAsync(
            authorizeUrl, TestContext.Current.CancellationToken);

        _mockPipeline.LoginWasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Login_ShouldStoreAuthTicketWithIssuerAttached()
    {
        //Setup
        ticketStore = _mockPipeline.GetTicketStore();
        sessionStore.Should().NotBeNull();

        // Initial login to create a session
        await _mockPipeline.LoginAsync("bob");

        // Verify that the session has been created in the store
        AuthenticationTicket? ticket = null;
        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Properties.Items[JwtClaimTypes.Issuer].Should().Be(IdentityServerPipeline.BaseUrl);
    }
}