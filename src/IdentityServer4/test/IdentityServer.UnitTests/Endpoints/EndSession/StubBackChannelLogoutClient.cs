// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4.Validation;

namespace IdentityServer.UnitTests.Endpoints.EndSession
{
    internal class StubBackChannelLogoutClient : IBackChannelLogoutService
    {
        public bool SendLogoutsWasCalled { get; set; }

        public Task SendLogoutNotificationsAsync(IEnumerable<BackChannelLogoutModel> clients)
        {
            SendLogoutsWasCalled = true;
            return Task.CompletedTask;
        }
    }
}
