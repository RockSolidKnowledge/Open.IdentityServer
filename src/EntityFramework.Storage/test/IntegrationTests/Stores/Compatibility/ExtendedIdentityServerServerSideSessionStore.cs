using System;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores.Compatibility;

public class ExtendedIdentityServerServerSideSessionStore(
    IPersistedGrantDbContext dbContext,
    ITelemetryService telemetry,
    TimeProvider timeProvider,
    ILogger<IdentityServerServerSideSessionStore> logger): 
    IdentityServerServerSideSessionStore(dbContext, telemetry, timeProvider, logger)
{
    protected override Models.ServerSideSession ToModel(Entities.IdentityServerServerSideSessions client)
    {
        var model = client.ToModel<ExtendedServerSideSession>();

        model.DataLength = model.Data.Length;
        model.Lifetime = model.Expires?.Subtract(model.Renewed);

        return model;
    }
}