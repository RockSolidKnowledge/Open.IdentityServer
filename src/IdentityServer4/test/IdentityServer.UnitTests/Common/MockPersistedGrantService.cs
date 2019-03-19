// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.UnitTests.Common
{
    public class MockPersistedGrantService : IPersistedGrantService
    {
        public IEnumerable<Consent> GetAllGrantsResult { get; set; }
        public bool RemoveAllGrantsWasCalled { get; set; }

        public Task<IEnumerable<Consent>> GetAllGrantsAsync(string subjectId)
        {
            return Task.FromResult(GetAllGrantsResult ?? Enumerable.Empty<Consent>());
        }

        public Task RemoveAllGrantsAsync(string subjectId, string clientId)
        {
            RemoveAllGrantsWasCalled = true;
            return Task.CompletedTask;
        }
    }
}
