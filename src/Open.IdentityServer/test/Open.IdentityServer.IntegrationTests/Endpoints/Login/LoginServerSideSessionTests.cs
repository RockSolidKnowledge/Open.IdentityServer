// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.Configuration;
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
    private FakeTimeProvider fakeTimeProvider = new FakeTimeProvider();
    private ITicketStore ticketStore = null;
    private IIdentityServerServerSideSessionStore? sessionStore = null;

    public LoginServerSideSessionTests()
    {
        fakeTimeProvider.SetUtcNow(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));


        _mockPipeline.Clients.AddRange([
            new Client
            {
                //TODO: Turn this into a ref code client
                ClientId = "client1",
                AllowedGrantTypes = GrantTypes.Code,
                AccessTokenType = AccessTokenType.Reference,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid", "profile", "api1" },
                RedirectUris = new List<string> { "https://client1/callback" },
                FrontChannelLogoutUri = "https://client1/signout",
                AllowOfflineAccess = true,
                RequirePkce = false,
                RequireClientSecret = false,
                CoordinateLifetimeWithUserSession = true
            },
            new Client
            {
                ClientId = "client2",
                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                RedirectUris = new List<string> { "https://client2/callback" },
                FrontChannelLogoutUri = "https://client2/signout",
                RequirePkce = false,
                RequireClientSecret = false,
            },
            new Client()
            {
                //TODO:// turn this in to a code client with non-ref tokens
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
            sessionStore = app.ApplicationServices.GetRequiredService<IIdentityServerServerSideSessionStore>();
        };

        _mockPipeline.OnPostConfigureServices += services =>
        {
            services.Configure<IdentityServerOptions>(options =>
            {
                //Session Expirey is only update if more than half of the cookie lifetime has passed,
                //so we set the cookie lifetime to 6 minutes, then update the TimeProvider by 5 minutes each step.
                options.Authentication.CookieLifetime = TimeSpan.FromMinutes(6);
                options.Authentication.CookieSlidingExpiration = true;
            });

            services.AddSingleton<TimeProvider>(fakeTimeProvider);

            services.PostConfigure<CookieAuthenticationOptions>(
                IdentityServerConstants.DefaultCookieAuthenticationScheme,
                options => { options.TimeProvider = fakeTimeProvider; });
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
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Auth code grant
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
        client1Authorization.Code.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        var clientList = ticket.Properties.GetClientList();
        clientList.Should().Contain("client1");

        // Exchange code for tokens
        var tokenClient1 = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client1",

            });

        var tokenResponse = await tokenClient1.RequestAuthorizationCodeTokenAsync(
            code: client1Authorization.Code!,
            redirectUri: "https://client1/callback",
            cancellationToken: TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();
        tokenResponse.AccessToken.Should().NotBeNull();
        tokenResponse.IdentityToken.Should().NotBeNull();
        tokenResponse.RefreshToken.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");


        var client2Authorization =
            await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client2",
                responseType: "code",
                scope: "openid profile",
                redirectUri: "https://client2/callback",
                state: "state2",
                nonce: "nonce2");

        client2Authorization.IsError.Should().BeFalse();
        client2Authorization.IdentityToken.Should().BeNull();
        client2Authorization.State.Should().Be("state2");

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        clientList = ticket.Properties.GetClientList();
        clientList.Should().Contain("client1");
        clientList.Should().Contain("client2");
    }

    [Fact]
    public async Task login_when_introspection_called_expect_session_renewed()
    {
        //Setup
        AuthenticationTicket? ticket = null;

        ticketStore = _mockPipeline.GetTicketStore();
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Get intial issued and expires times after login, before any other clients have been added to the session
        var issuedUtc = ticket.Properties.IssuedUtc!.Value;
        var expiresUtc = ticket.Properties.ExpiresUtc!.Value;

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        // Auth code grant
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
        client1Authorization.Code.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Exchange code for tokens
        var client = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client1",

            });

        var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(
            code: client1Authorization.Code!,
            redirectUri: "https://client1/callback",
            cancellationToken: TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();
        tokenResponse.AccessToken.Should().NotBeNull();
        tokenResponse.IdentityToken.Should().NotBeNull();
        tokenResponse.RefreshToken.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        // Expired is not update on code exchange
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc);
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc);

        // Advance time by 5 minutes to simulate time passing

        // Use reference token with introspection to update the session
        var introspectionResponse = await _mockPipeline.BackChannelClient!
            .IntrospectTokenAsync(new TokenIntrospectionRequest()
            {
                Address = IdentityServerPipeline.IntrospectionEndpoint,
                ClientId = "client1",
                Token = tokenResponse.AccessToken
            }, TestContext.Current.CancellationToken);

        introspectionResponse.IsError.Should().BeFalse();
        introspectionResponse.IsActive.Should().BeTrue();


        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        ticket.Properties.IssuedUtc.Should().Be(issuedUtc.AddMinutes(5));
        ticket.Properties.ExpiresUtc.Should().BeAfter(expiresUtc);

    }

    [Fact]
    public async Task login_when_refresh_token_called_expect_session_renewed()
    {
        //Setup
        AuthenticationTicket? ticket = null;

        ticketStore = _mockPipeline.GetTicketStore();
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Get intial issued and expires times after login, before any other clients have been added to the session
        var issuedUtc = ticket.Properties.IssuedUtc!.Value;
        var expiresUtc = ticket.Properties.ExpiresUtc!.Value;

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        // Auth code grant
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
        client1Authorization.Code.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Exchange code for tokens
        var client = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client1",

            });

        var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(
            code: client1Authorization.Code!,
            redirectUri: "https://client1/callback",
            cancellationToken: TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();
        tokenResponse.AccessToken.Should().NotBeNull();
        tokenResponse.IdentityToken.Should().NotBeNull();
        tokenResponse.RefreshToken.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        // Expired is not update on code exchange
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc);
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc);

        var refreshTokenResponse = await _mockPipeline.BackChannelClient!
            .RequestRefreshTokenAsync(new RefreshTokenRequest()
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client1",
                RefreshToken = tokenResponse.RefreshToken
            }, TestContext.Current.CancellationToken);

        refreshTokenResponse.IsError.Should().BeFalse();
        refreshTokenResponse.AccessToken.Should().NotBeNull();
        refreshTokenResponse.RefreshToken.Should().NotBeNull();

        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc.AddMinutes(5));
        ticket.Properties.ExpiresUtc.Should().BeAfter(expiresUtc);
    }

    [Fact]
    public async Task login_when_userinfo_called_expect_session_renewed()
    {
        //Setup
        AuthenticationTicket? ticket = null;

        ticketStore = _mockPipeline.GetTicketStore();
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Get intial issued and expires times after login, before any other clients have been added to the session
        var issuedUtc = ticket.Properties.IssuedUtc!.Value;
        var expiresUtc = ticket.Properties.ExpiresUtc!.Value;

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        // Auth code grant
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
        client1Authorization.Code.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Exchange code for tokens
        var client = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client1",

            });

        var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(
            code: client1Authorization.Code!,
            redirectUri: "https://client1/callback",
            cancellationToken: TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();
        tokenResponse.AccessToken.Should().NotBeNull();
        tokenResponse.IdentityToken.Should().NotBeNull();
        tokenResponse.RefreshToken.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        // Expired is not update on code exchange
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc);
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc);

        var userInfoResponse = await _mockPipeline.BackChannelClient!
            .GetUserInfoAsync(new UserInfoRequest()
            {
                Address = IdentityServerPipeline.UserInfoEndpoint,
                Token = tokenResponse.AccessToken
            }, TestContext.Current.CancellationToken);

        userInfoResponse.IsError.Should().BeFalse();

        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc.AddMinutes(5));
        ticket.Properties.ExpiresUtc.Should().BeAfter(expiresUtc);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task logout_when_multiple_clients_should_render_front_channel_signout_iframes()
    {
        ticketStore = _mockPipeline.GetTicketStore();

        await _mockPipeline.LoginAsync("bob");
        var sid = _mockPipeline.GetSessionCookie().Value;

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        var client1Authorization = await _mockPipeline.RequestAuthorizationEndpointAsync(
            clientId: "client1",
            responseType: "code",
            scope: "openid profile api1 offline_access",
            redirectUri: "https://client1/callback",
            state: "state",
            nonce: "nonce");

        client1Authorization.IsError.Should().BeFalse();
        client1Authorization.Code.Should().NotBeNull();

        var tokenClient1 = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client1",
            });

        var client1TokenResponse = await tokenClient1.RequestAuthorizationCodeTokenAsync(
            code: client1Authorization.Code!,
            redirectUri: "https://client1/callback",
            cancellationToken: TestContext.Current.CancellationToken);

        client1TokenResponse.IsError.Should().BeFalse();
        client1TokenResponse.IdentityToken.Should().NotBeNull();

        var client2Authorization = await _mockPipeline.RequestAuthorizationEndpointAsync(
            clientId: "client2",
            responseType: "code",
            scope: "openid profile",
            redirectUri: "https://client2/callback",
            state: "state2",
            nonce: "nonce2");

        client2Authorization.IsError.Should().BeFalse();

        var ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        var clientList = ticket!.Properties.GetClientList();
        clientList.Should().Contain("client1");
        clientList.Should().Contain("client2");

        var endSessionUrl = IdentityServerPipeline.EndSessionEndpoint +
                            "?id_token_hint=" + Uri.EscapeDataString(client1TokenResponse.IdentityToken!);

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