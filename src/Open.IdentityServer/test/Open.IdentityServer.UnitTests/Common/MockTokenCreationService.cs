// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using System.Threading.Tasks;

namespace Open.IdentityServer.UnitTests.Common;

class MockTokenCreationService : ITokenCreationService
{
    public string Token { get; set; }

    public Task<string> CreateTokenAsync(Token token)
    {
        return Task.FromResult(Token);
    }
}