using System;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores.Compatibility;

public class ExtendedIdentityServerServerSideSessions: IdentityServerServerSideSessions
{
    public TimeSpan? Lifetime { get; set; }
    public int DataLength { get; set; }
}