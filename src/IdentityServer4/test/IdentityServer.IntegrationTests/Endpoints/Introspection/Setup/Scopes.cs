// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer4.IntegrationTests.Endpoints.Introspection
{
    internal class Scopes
    {
        public static IEnumerable<ApiResource> GetApiScopes()
        {
            return new ApiResource[]
            {
                new ApiResource
                {
                    Name = "api1",
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    }
                },
                new ApiResource
                {
                    Name = "api2",
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "api2"
                        }
                    }
                },
                 new ApiResource
                {
                    Name = "api3",
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "api3-a"
                        },
                        new Scope
                        {
                            Name = "api3-b"
                        }
                    }
                }
            };
        }
    }
}