// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Test;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.Login;

public class LoginServerSideSessionTests
{
    private const string Category = "LoginServerSideSessionTests";

    private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();
    private IIdentityServerServerSideSessionStore? sessionStore = null;

    public LoginServerSideSessionTests()
    {
        _mockPipeline.EnableServerSideSessions = true;
        
        _mockPipeline.Clients.AddRange([
            new Client
            {
                ClientId = "client1",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid", "profile" },
                RedirectUris = new List<string> { "https://client1/callback" },
                AllowAccessTokensViaBrowser = true
            },
            new Client
            {
                ClientId = "client2",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = true,
                AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                RedirectUris = new List<string> { "https://client2/callback" },
                AllowAccessTokensViaBrowser = true
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
    public async Task Login_WhenUserChangfes_ShouldUpdateSessionInServerStore()
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
}