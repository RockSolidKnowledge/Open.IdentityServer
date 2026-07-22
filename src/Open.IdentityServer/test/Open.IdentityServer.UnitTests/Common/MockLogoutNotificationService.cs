// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Open.IdentityServer.UnitTests.Common;

public class MockLogoutNotificationService : ILogoutNotificationService
{
    public bool GetFrontChannelLogoutNotificationsUrlsCalled { get; set; }
    public List<string> FrontChannelLogoutNotificationsUrls { get; set; } = new List<string>();

    public bool SendBackChannelLogoutNotificationsCalled { get; set; }
    public List<BackChannelLogoutRequest> BackChannelLogoutRequests { get; set; } = new List<BackChannelLogoutRequest>();

    public Task<IEnumerable<string>> GetFrontChannelLogoutNotificationsUrlsAsync(LogoutNotificationContext context)
    {
        GetFrontChannelLogoutNotificationsUrlsCalled = true;
        return Task.FromResult(FrontChannelLogoutNotificationsUrls.AsEnumerable());
    }

    public Task<IEnumerable<BackChannelLogoutRequest>> GetBackChannelLogoutNotificationsAsync(LogoutNotificationContext context)
    {
        SendBackChannelLogoutNotificationsCalled = true;
        return Task.FromResult(BackChannelLogoutRequests.AsEnumerable());
    }
}