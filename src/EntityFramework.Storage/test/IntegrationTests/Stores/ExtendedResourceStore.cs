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

    protected override Models.ApiScope ToApiScopeModel(Entities.ApiScope scope)
    {
        var model = scope.ToModel<ExtendedApiScope>();

        model.Created = scope.Created;
        model.Updated = scope.Updated;
        model.LastAccessed = scope.LastAccessed;

        return model;
    }
}
