using System;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores.Compatibility;

public class ExtendedIdentityServerServerSideSessionStore(
    IPersistedGrantDbContext dbContext,
    IAuthenticationTicketSerializer authTicketSerializer,
    ITelemetryService telemetry,
    TimeProvider timeProvider,
    ILogger<IdentityServerServerSideSessionStore> logger): 
    IdentityServerServerSideSessionStore(dbContext, authTicketSerializer, telemetry, timeProvider, logger)
{
    protected override Models.ServerSideSession ToModel(Entities.ServerSideSession client)
    {
        var model = client.ToModel<ExtendedServerSideSession>(authTicketSerializer);

        model.DataLength = model.Data.Length;
        model.Lifetime = model.Expires?.Subtract(model.Renewed);

        return model;
    }
}