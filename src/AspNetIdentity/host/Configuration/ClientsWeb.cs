// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.using System.Collections.Generic;

using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace Host.Configuration
{
    public static class ClientsWeb
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                ///////////////////////////////////////////
                // JS OIDC Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "js_oidc",
                    ClientName = "JavaScript OIDC Client",
                    ClientUri = "http://identityserver.io",
                    
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    
                    RedirectUris = 
                    {
                        "https://localhost:44300/index.html",
                        "https://localhost:44300/callback.html",
                        "https://localhost:44300/silent.html",
                        "https://localhost:44300/popup.html"
                    },

                    PostLogoutRedirectUris = { "https://localhost:44300/index.html" },
                    AllowedCorsOrigins = { "https://localhost:44300" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "feature1", "feature2"
                    }
                },
                
                ///////////////////////////////////////////
                // MVC Automatic Token Management Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "mvc.tokenmanagement",
                    
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    AccessTokenLifetime = 75,

                    RedirectUris = { "https://localhost:44301/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:44301/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:44301/signout-callback-oidc" },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "feature1", "feature2"
                    }
                },
                
                ///////////////////////////////////////////
                // MVC Code Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "mvc.code",
                    ClientName = "MVC Code Flow",
                    ClientUri = "http://identityserver.io",

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RequireConsent = true,
                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:44302/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:44302/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:44302/signout-callback-oidc" },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "feature1", "feature2", 
                        "transaction"
                    }
                },
                
                ///////////////////////////////////////////
                // MVC Hybrid Flow Sample (Back Channel logout)
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "mvc.hybrid.backchannel",
                    ClientName = "MVC Hybrid (with BackChannel logout)",
                    ClientUri = "http://identityserver.io",

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = false,

                    RedirectUris = { "https://localhost:44303/signin-oidc" },
                    BackChannelLogoutUri = "http://localhost:44303/logout",
                    PostLogoutRedirectUris = { "https://localhost:44303/signout-callback-oidc" },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "feature1", "feature2"
                    }
                }
            };
        }
    }
}