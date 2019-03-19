// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Infrastructure;
using IdentityServer4.Validation;

namespace IdentityServer.UnitTests.Endpoints.EndSession
{
    internal class StubBackChannelLogoutClient : BackChannelLogoutClient
    {
        public StubBackChannelLogoutClient() : base(null, null, null, null)
        {
        }

        public bool SendLogoutsWasCalled { get; set; }

        public override Task SendLogoutsAsync(IEnumerable<BackChannelLogoutModel> clients)
        {
            SendLogoutsWasCalled = true;
            return Task.CompletedTask;
        }
    }
}
