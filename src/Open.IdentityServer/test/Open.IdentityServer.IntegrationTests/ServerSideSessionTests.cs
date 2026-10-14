// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Test;
using Xunit;

namespace Open.IdentityServer.IntegrationTests;

public abstract class ServerSideSessionTests
{
    protected IdentityServerPipeline _mockPipeline = new();
    protected FakeTimeProvider fakeTimeProvider = new();
    protected ITicketStore? ticketStore;
    protected IIdentityServerServerSideSessionStore? sessionStore;

    protected ServerSideSessionTests()
    {
        _mockPipeline.EnableServerSideSessions = true;
        
        fakeTimeProvider.SetUtcNow(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        _mockPipeline.Clients.AddRange([
            new Client
            {
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
                CoordinateLifetimeWithUserSession = true,
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
                ApiSecrets = [
                    new Secret("secret".Sha256())
                ],
                Scopes = ["api1", "api2"]
            },
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

    protected async Task<(AuthorizeResponse, TokenResponse)> AuthCodeAndTokenRequest(string clientId, string scope, string redirectUri)
    {
        var codeResponse = await AuthCodeRequest(clientId, scope, redirectUri);
        var tokenResponse = await AuthCodeTokenRequest(clientId, redirectUri, codeResponse.Code!);
        return (codeResponse, tokenResponse);
    }

    protected async Task<AuthorizeResponse> AuthCodeRequest(string clientId, string scope, string redirectUri)
    {
        var authResponse = await _mockPipeline.RequestAuthorizationEndpointAsync(
            clientId: clientId,
            responseType: "code",
            scope: scope,
            redirectUri: redirectUri,
            state: $"state-{clientId}",
            nonce: $"nonce-{clientId}");

        authResponse.IsError.Should().BeFalse();
        authResponse.IdentityToken.Should().BeNull();
        authResponse.State.Should().Be($"state-{clientId}");
        authResponse.Code.Should().NotBeNull();

        return authResponse;
    }

    protected async Task<TokenResponse> AuthCodeTokenRequest(string clientId, string redirectUri, string code)
    {
        var tokenClient1 = new TokenClient(
            _mockPipeline.BackChannelClient!,
            new TokenClientOptions
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = clientId,
            });

        var tokenResponse = await tokenClient1.RequestAuthorizationCodeTokenAsync(
            code: code,
            redirectUri: redirectUri,
            cancellationToken: TestContext.Current.CancellationToken);

        tokenResponse.IsError.Should().BeFalse();
        tokenResponse.AccessToken.Should().NotBeNull();
        tokenResponse.IdentityToken.Should().NotBeNull();

        return tokenResponse;
    }
}