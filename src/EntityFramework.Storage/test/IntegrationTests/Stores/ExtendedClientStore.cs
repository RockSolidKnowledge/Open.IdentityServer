// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores;

internal class ExtendedClientStore : ClientStore
{
    public ExtendedClientStore(IConfigurationDbContext context, ITelemetryService telemetry, ILogger<ExtendedClientStore> logger) 
        : base(context, telemetry, logger)
    {
    }

    protected override Models.Client ToModel(Entities.Client client)
    {
        var model = client.ToModel<ExtendedClient>();

        model.Created = client.Created;
        model.Updated = client.Updated;
        model.LastAccessed = client.LastAccessed;

        return model;
    }
}
