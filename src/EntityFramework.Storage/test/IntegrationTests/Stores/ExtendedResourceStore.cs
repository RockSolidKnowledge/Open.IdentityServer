// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores;

internal class ExtendedResourceStore : ResourceStore
{
    public ExtendedResourceStore(IConfigurationDbContext context, ITelemetryService telemetry, ILogger<ExtendedResourceStore> logger) 
        : base(context, telemetry, logger)
    {
    }

    protected override Models.ApiResource ToApiResourceModel(Entities.ApiResource resource)
    {
        var model = resource.ToModel<ExtendedApiResource>();

        model.Created = resource.Created;
        model.Updated = resource.Updated;
        model.LastAccessed = resource.LastAccessed;

        return model;
    }

    protected override Models.IdentityResource ToIdentityResourceModel(Entities.IdentityResource resource)
    {
        var model = resource.ToModel<ExtendedIdentityResource>();

        model.Created = resource.Created;
        model.Updated = resource.Updated;

        return model;
    }

    protected override Models.ApiScope ToApiScopeModel(Entities.ApiScope scope)
    {
        var model = scope.ToModel<ExtendedApiScope>();

        model.Created = scope.Created;
        model.Updated = scope.Updated;
        model.LastAccessed = scope.LastAccessed;

        return model;
    }
}
