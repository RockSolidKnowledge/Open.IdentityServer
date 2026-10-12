using System;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores.Compatibility;

public class ExtendedServerSideSession: ServerSideSession
{
    public TimeSpan? Lifetime { get; set; }
    public int DataLength { get; set; }
}