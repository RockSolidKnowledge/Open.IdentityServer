// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Test;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.Login;

public class LoginServerSideSessionTests
{
    private const string Category = "LoginServerSideSessionTests";

    private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();
    private ITicketStore ticketStore = null;
    private IIdentityServerServerSideSessionStore? sessionStore = null;

    public LoginServerSideSessionTests()
    {
        _mockPipeline.EnableServerSideSessions = true;
        
        _mockPipeline.Clients.AddRange([
            new Client
            {
                ClientId = "client1",
                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid", "profile", "api1" },
                RedirectUris = new List<string> { "https://client1/callback" },
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = true,
                RequirePkce = false,
                RequireClientSecret = false
            },
            new Client
            {
                ClientId = "client2",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                RedirectUris = new List<string> { "https://client2/callback" },
                AllowAccessTokensViaBrowser = true
            },
            new Client()
            {
                ClientId = "client3",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string> { "api1", "api2" },
                RedirectUris = new List<string> { "https://client3/callback" },
                AllowOfflineAccess = true
            }
        ]);

        _mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "bob",
            Username = "bob",
            Claims =
            [
                new Claim("name", "Bob Loblaw"),
                new Claim("email", "bob@loblaw.com"),
                new Claim("role", "Attorney")
            ]
        });

        _mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "alice",
            Username = "alice",
            Claims =
            [
                new Claim("name", "Alice Smith"),
                new Claim("alice", "alice@smith.com"),
                new Claim("role", "Attorney")
            ]
        });

        _mockPipeline.IdentityScopes.AddRange([
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        ]);
        _mockPipeline.ApiResources.AddRange([
            new ApiResource
            {
                Name = "api",
            }
        ]);
        _mockPipeline.ApiScopes.AddRange([
            new ApiScope
            {
                Name = "api1"
            },
            new ApiScope
            {
                Name = "api2"
            }
        ]);
        
        _mockPipeline.OnPreConfigure += app =>
        {
            ticketStore = app.ApplicationServices.GetRequiredService<ITicketStore>();
            sessionStore = app.ApplicationServices.GetRequiredService<IIdentityServerServerSideSessionStore>();
        };

        _mockPipeline.Initialize();
    }

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
    public async Task Login_WhenUserChanges_ShouldUpdateSessionInServerStore()
    {
        sessionStore.Should().NotBeNull();
        
        await _mockPipeline.LoginAsync("bob");

        Cookie originalSessionCookie = _mockPipeline.GetSessionCookie();

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();
        
        var originalSession = await sessionStore.GetSession(authKey);
        originalSession.Should().NotBeNull();
        originalSession.SessionId.Should().Be(originalSessionCookie.Value);
        originalSession.SubjectId.Should().Be("bob");
        
        await _mockPipeline.LoginAsync("alice");

        Cookie newSessionCookie = _mockPipeline.GetSessionCookie();
        
        var updatedSession = await sessionStore.GetSession(authKey);
        updatedSession.Should().NotBeNull();
        updatedSession.SessionId.Should().Be(newSessionCookie.Value);
        updatedSession.SubjectId.Should().Be("alice");
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task login_when_multiple_clients_should_update_session_in_server_store()
    {
        sessionStore.Should().NotBeNull();
        
        await _mockPipeline.LoginAsync("bob");

        AuthenticationTicket? ticket = null;
        
        Cookie originalSessionCookie = _mockPipeline.GetSessionCookie();

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();
        
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        
        var client1Authorization =
            await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client1",
                responseType: "code",
                scope: "openid profile api1 offline_access",
                redirectUri: "https://client1/callback",
                state: "state",
                nonce: "nonce");

        client1Authorization.IsError.Should().BeFalse();
        client1Authorization.IdentityToken.Should().BeNull();
        client1Authorization.State.Should().Be("state");
        
        var tokenClient1 = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client1"
            });

        var tokenResponse = await tokenClient1.RequestAuthorizationCodeTokenAsync(
            code: client1Authorization.Code!,
            redirectUri: "https://client1/callback",
            cancellationToken: TestContext.Current.CancellationToken);
    
        tokenResponse.IsError.Should().BeFalse();
        tokenResponse.AccessToken.Should().NotBeNull();
        tokenResponse.IdentityToken.Should().NotBeNull();
        tokenResponse.RefreshToken.Should().NotBeNull();
        
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        
        var initialIssuedUtc = ticket.Properties.IssuedUtc;
        var initialExpiresUtc = ticket.Properties.ExpiresUtc;
        
        var clientList = ticket.Properties.GetClientList();
        clientList.Should().Contain("client1");
        
        var client2Authorization =
            await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client2",
                responseType: "id_token",
                scope: "openid profile",
                redirectUri: "https://client2/callback",
                state: "state2",
                nonce: "nonce2");
        
        client2Authorization.IsError.Should().BeFalse();
        client2Authorization.IdentityToken.Should().NotBeNull();
        client2Authorization.State.Should().Be("state2");
        
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        
        clientList = ticket.Properties.GetClientList();
        clientList.Should().Contain("client1");
        clientList.Should().Contain("client2");
        
        ticket.Properties.IssuedUtc.Should().Be(initialIssuedUtc!.Value);
        ticket.Properties.ExpiresUtc.Should().BeAfter(initialExpiresUtc!.Value);
        
        var tokenClient3 = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client3",
                ClientSecret = "secret"
            });

        var accessTokenResponse =
            await tokenClient3.RequestClientCredentialsTokenAsync(
                scope: "api1 api2",
                cancellationToken: TestContext.Current.CancellationToken);

        accessTokenResponse.IsError.Should().BeFalse();
        accessTokenResponse.AccessToken.Should().NotBeNull();
        accessTokenResponse.RefreshToken.Should().BeNull();
        
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        
        clientList = ticket.Properties.GetClientList();
        clientList.Should().Contain("client1");
        clientList.Should().Contain("client2");
        clientList.Should().NotContain("client3");

        ticket.Properties.IssuedUtc.Should().BeAfter(initialIssuedUtc!.Value);


    }
}