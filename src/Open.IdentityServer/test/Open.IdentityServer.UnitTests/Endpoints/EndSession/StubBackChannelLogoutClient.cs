// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.UnitTests.Endpoints.EndSession;

internal class StubBackChannelLogoutClient : IBackChannelLogoutService
{
    public bool SendLogoutsWasCalled { get; set; }

    public Task SendLogoutNotificationsAsync(LogoutNotificationContext context)
    {
        SendLogoutsWasCalled = true;
        return Task.CompletedTask;
    }
}