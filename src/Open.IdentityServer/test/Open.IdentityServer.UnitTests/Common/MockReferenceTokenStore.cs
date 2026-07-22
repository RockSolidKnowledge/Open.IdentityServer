// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.UnitTests.Common;

class MockReferenceTokenStore : IReferenceTokenStore
{
    public Task<Token> GetReferenceTokenAsync(string handle)
    {
        throw new NotImplementedException();
    }

    public Task RemoveReferenceTokenAsync(string handle)
    {
        throw new NotImplementedException();
    }

    public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
    {
        throw new NotImplementedException();
    }

    public Task<string> StoreReferenceTokenAsync(Token token)
    {
        throw new NotImplementedException();
    }
}