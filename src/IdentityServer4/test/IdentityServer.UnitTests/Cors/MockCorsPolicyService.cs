// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using System.Threading.Tasks;

namespace IdentityServer4.UnitTests.Hosting.Cors
{
    public class MockCorsPolicyService : ICorsPolicyService
    {
        public bool WasCalled { get; set; }
        public bool Response { get; set; }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            WasCalled = true;
            return Task.FromResult(Response);
        }
    }
}
