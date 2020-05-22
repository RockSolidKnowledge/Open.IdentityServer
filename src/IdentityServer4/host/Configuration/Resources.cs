// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;
using static IdentityServer4.IdentityServerConstants;

namespace Host.Configuration
{
    public class Resources
    {
        // identity resources represent identity data about a user that can be requested via the scope parameter (OpenID Connect)
        public static readonly IEnumerable<IdentityResource> IdentityResources =
            new[]
            {
                // some standard scopes from the OIDC spec
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),

                // custom identity resource with some consolidated claims
                new IdentityResource("custom.profile", new[] { JwtClaimTypes.Name, JwtClaimTypes.Email, "location" })
            };

        // API scopes represent values that describe scope of access and can be requested by the scope parameter (OAuth)
        public static readonly IEnumerable<ApiScope> ApiScopes =
            new[]
            {
                // local feature
                new ApiScope(LocalApi.ScopeName),

                // some generic scopes
                new ApiScope("scope1"),
                new ApiScope("scope2"), 
                new ApiScope("scope3"),
                new ApiScope("shared.scope"),

                // used as a dynamic scope
                new ApiScope("transaction", "Transaction")
                {
                    Description = "Some Transaction"
                }
            };

        // API resources are more formal representation of a resource with processing rules and their scopes (if any)
        public static readonly IEnumerable<ApiResource> ApiResources = 
            new[]
            {
                // simple version with ctor
                new ApiResource("resource1", "Resource 1")
                {
                    // this is needed for introspection when using reference tokens
                    ApiSecrets = { new Secret("secret".Sha256()) },

                    //AllowedSigningAlgorithms = { "RS256", "ES256" }

                    Scopes = { "scope1", "shared.scope" }
                },
                
                // expanded version if more control is needed
                new ApiResource("resource2", "Resource 2")
                {
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    //AllowedSigningAlgorithms = { "PS256", "ES256", "RS256" },

                    UserClaims =
                    {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email
                    },

                    Scopes = { "scope2", "shared.scope" }
                }
            };
    }
}
