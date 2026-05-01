// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using Open.IdentityServer.Models;

namespace IdentityServer.UnitTests.Validation.Setup;

internal class TestScopes
{
    public static IEnumerable<IdentityResource> GetIdentity()
    {
        return
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        ];
    }

    public static IEnumerable<ApiResource> GetApis()
    {
        return
        [
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
                RequireResourceIndicator = true,
            },
            new ApiResource
            {
                Name = "urn:unauth.resource",
                Scopes = ["unauth:Read", "unauth:Write", "unauth:All"],
            },
        ];
    }

    public static IEnumerable<ApiScope> GetScopes()
    {
        return
        [
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
            new ApiScope
            {
                Name = "All",
                Description = "Shared All scope"
            }
        ];
    }
}