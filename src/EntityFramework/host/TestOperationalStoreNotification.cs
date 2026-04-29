// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.EntityFramework;
using Open.IdentityServer.EntityFramework.Entities;

namespace IdentityServerHost;

public class TestOperationalStoreNotification : IOperationalStoreNotification
{
    public TestOperationalStoreNotification()
    {
        Console.WriteLine("ctor");
    }

    public Task PersistedGrantsRemovedAsync(IEnumerable<PersistedGrant> persistedGrants)
    {
        foreach (var grant in persistedGrants)
        {
            Console.WriteLine("cleaned: " + grant.Type);
        }
        return Task.CompletedTask;
    }

    public Task DeviceCodesRemovedAsync(IEnumerable<DeviceFlowCodes> deviceCodes)
    {
        foreach (var deviceCode in deviceCodes) 
        {
            Console.WriteLine("cleaned device code");
        }
        return Task.CompletedTask;
    }
}