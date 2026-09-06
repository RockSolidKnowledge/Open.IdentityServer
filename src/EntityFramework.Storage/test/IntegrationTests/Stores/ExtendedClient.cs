// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using System;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores;

internal class ExtendedClient : Client
{
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? LastAccessed { get; internal set; }

    public string? X
    {
        get => Properties.ContainsKey("x") ? Properties["x"] : null;
        set => Properties["x"] = value;
    }
}