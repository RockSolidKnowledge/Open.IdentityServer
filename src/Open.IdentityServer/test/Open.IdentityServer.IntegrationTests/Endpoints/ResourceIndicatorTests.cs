// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using IdentityServer.IntegrationTests.Common;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.IntegrationTests.Endpoints;

public abstract class ResourceIndicatorTests
{
    protected IdentityServerPipeline mockPipeline = new();

    public ResourceIndicatorTests()
    {
        mockPipeline.IdentityScopes.AddRange([
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        ]);
        
        mockPipeline.ApiResources.AddRange([
            new ApiResource
            {
                Name = "api",
                Scopes = { "resource", "resource2" }
            },
            new ApiResource
            {
                Name = "urn:valid.resource",
                Scopes = ["urn:valid.resource:Read", "urn:valid.resource:Write", "urn:valid.resource:All", "All"],
            },
            new ApiResource
            {
                Name = "https://valid.resource.com",
                Scopes = ["valid:Read", "valid:Write", "valid:All", "All"],
            },
            new ApiResource
            {
                Name = "urn:unauth.resource",
                Scopes = ["unauth:Read", "unauth:Write", "unauth:All"],
            },
        ]);
        
        mockPipeline.ApiScopes.AddRange([
            new ApiScope
            {
                Name = "resource",
                Description = "resource scope"
            },
            new ApiScope
            {
                Name = "resource2",
                Description = "resource scope"
            },
            new ApiScope
            {
                Name = "urn:valid.resource:Read",
                Description = "Valid Resource 'urn:' prefixed, Read scope"
            },
            new ApiScope
            {
                Name = "urn:valid.resource:Write",
                Description = "Valid Resource 'urn:' prefixed, Write scope"
            },
            new ApiScope
            {
                Name = "urn:valid.resource:All",
                Description = "Valid Resource 'urn:' prefixed, All scope"
            },
            new ApiScope
            {
                Name = "valid:Read",
                Description = "Valid Resource 'https:' prefixed, Read scope"
            },
            new ApiScope
            {
                Name = "valid:Write",
                Description = "Valid Resource 'https:' prefixed, Write scope"
            },
            new ApiScope
            {
                Name = "valid:All",
                Description = "Valid Resource 'https:' prefixed, All scope"
            },
            new ApiScope
            {
                Name = "unauth:Read",
                Description = "Unauth Resource 'urn:' prefixed, Read scope"
            },
            new ApiScope
            {
                Name = "unauth:Write",
                Description = "Unauth Resource 'urn:' prefixed, Write scope"
            },
            new ApiScope
            {
                Name = "unauth:All",
                Description = "Unauth Resource 'urn:' prefixed, All scope"
            },
        ]);
        
        mockPipeline.Clients.AddRange([CodeClient, HybridClient, ImplicitClient, ClientCredentialsClient]);
        
        mockPipeline.Initialize();
    }

    protected static Client CodeClient = new Client
    {
        ClientName = "Code Client",
        Enabled = true,
        ClientId = "codeclient",

        AllowedGrantTypes = GrantTypes.Code,
        AllowedScopes =
        [
            "openid", "profile", "resource", "resource2",
            "urn:valid.resource:Read", "urn:valid.resource:Write", "urn:valid.resource:All",
            "valid:Read", "valid:Write", "valid:All",
        ],

        RequireConsent = false,
        RequirePkce = false,
        RequireClientSecret = false,
        RedirectUris = (List<string>)["https://server/cb"],

        AuthorizationCodeLifetime = 60,
        AllowOfflineAccess = true,
    };
    
    protected static Client HybridClient = new Client
    {
        ClientName = "Hybrid Client",
        Enabled = true,
        ClientId = "hybridclient",
        ClientSecrets = (List<Secret>)[new Secret("secret".Sha256())],

        AllowedGrantTypes = GrantTypes.Hybrid,
        AllowedScopes =
        [
            "openid", "profile", "resource", "resource2",
            "urn:valid.resource:Read", "urn:valid.resource:Write", "urn:valid.resource:All",
            "valid:Read", "valid:Write", "valid:All", "All",
        ],
        AllowAccessTokensViaBrowser = true,

        RequireConsent = false,
        RequirePkce = false,

        RedirectUris = (List<string>)["https://server/cb"],

        AuthorizationCodeLifetime = 60
    };
    
    protected static Client ImplicitClient = new Client
    {
        ClientName = "Implicit Client",
        ClientId = "implicitclient",

        AllowedGrantTypes = GrantTypes.Implicit,
        AllowedScopes =
        [
            "openid", "profile", "resource", "resource2",
            "urn:valid.resource:Read", "urn:valid.resource:Write", "urn:valid.resource:All",
            "valid:Read", "valid:Write", "valid:All", "All",
        ],
        AllowAccessTokensViaBrowser = true,

        RequireConsent = false,

        RedirectUris = (List<string>)["oob://implicit/cb"]
    };

    protected static Client ClientCredentialsClient = new Client
    {
        ClientName = "Client Credentials Client",
        Enabled = true,
        ClientId = "client",
        ClientSecrets = (List<Secret>)[new Secret("secret".Sha256())],

        AllowedGrantTypes = GrantTypes.ClientCredentials,
        AllowedScopes =
        [
            "openid", "profile", "resource", "resource2",
            "urn:valid.resource:Read", "urn:valid.resource:Write", "urn:valid.resource:All",
            "valid:Read", "valid:Write", "valid:All",
            "All",
        ],

        AllowOfflineAccess = true,

        AccessTokenType = AccessTokenType.Jwt
    };
}