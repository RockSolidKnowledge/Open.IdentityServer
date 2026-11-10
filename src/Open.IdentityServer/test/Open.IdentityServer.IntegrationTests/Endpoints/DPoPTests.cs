// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Test;
using Open.IdentityServer.Test.Utilities;

namespace Open.IdentityServer.IntegrationTests.Endpoints;

public abstract class DPoPTests
{
    protected readonly IdentityServerPipeline mockPipeline = new();
    protected readonly FakeTimeProvider fakeTimeProvider = new();

    protected IAuthorizationCodeStore authorizationCodeStore;
    protected IRefreshTokenStore refreshTokenStore;
    protected IReferenceTokenStore referenceTokenStore;

    protected readonly DPoPProofGenerator dPoPProofGenerator;

    protected readonly DateTimeOffset fakeNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    protected string client_id = "client";
    protected string client_secret = "secret";

    protected string scope_name = "api";
    protected string scope_secret = "api_secret";
    
    protected static Client DPoPClient = new Client
    {
        ClientName = "Code Client",
        Enabled = true,
        ClientId = "dpop-client",

        AllowedGrantTypes = GrantTypes.Code,
        AllowedScopes =
        [
            "openid", "profile", "api"
        ],

        RequireConsent = false,
        RequirePkce = false,
        RequireClientSecret = false,
        RedirectUris = (List<string>)[
            "https://server/cb",
            "oob://implicit/cb",
        ],

        AuthorizationCodeLifetime = 60,
        AllowOfflineAccess = true,
    };

    protected DPoPTests()
    {
        dPoPProofGenerator = new DPoPProofGenerator(fakeTimeProvider);
        
        mockPipeline.Clients.Add(DPoPClient);

        mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "bob",
            Username = "bob",
            Password = "password",
            Claims =
            [
                new Claim("name", "Bob Loblaw"),
                new Claim("email", "bob@loblaw.com"),
                new Claim("role", "Attorney")
            ]
        });

        mockPipeline.IdentityScopes.AddRange([new IdentityResources.OpenId()]);

        mockPipeline.ApiResources.AddRange([
            new ApiResource
            {
                Name = "api",
                ApiSecrets = new List<Secret> { new Secret(scope_secret.Sha256()) },
                Scopes = {scope_name}
            }
        ]);

        mockPipeline.ApiScopes.AddRange([
            new ApiScope
            {
                Name = scope_name
            }
        ]);

        mockPipeline.OnPreConfigure += app =>
        {
            authorizationCodeStore = app.ApplicationServices.GetRequiredService<IAuthorizationCodeStore>();
            refreshTokenStore = app.ApplicationServices.GetRequiredService<IRefreshTokenStore>();
            referenceTokenStore = app.ApplicationServices.GetRequiredService<IReferenceTokenStore>();
        };

        mockPipeline.Initialize();
    }

    protected void ConfigureClient(
        ICollection<string> grantTypes = null, 
        AccessTokenType accessTokenType = AccessTokenType.Jwt, 
        bool requireDPoP = false, 
        DPoPTokenExpirationValidationMode validationMode = DPoPTokenExpirationValidationMode.Iat,
        bool allowTokensViaBrowser = false)
    {
        DPoPClient.AllowedGrantTypes = grantTypes ?? GrantTypes.Code;
        DPoPClient.AccessTokenType = accessTokenType;
        DPoPClient.RequireDPoP = requireDPoP;
        DPoPClient.DPoPValidationMode = validationMode;
        DPoPClient.AllowAccessTokensViaBrowser = allowTokensViaBrowser;
    }
}