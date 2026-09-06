using Open.IdentityServer.Models;
using System;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores;

internal class ExtendedApiResource : ApiResource
{
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? LastAccessed { get; set; }

    public string? X
    {
        get => Properties.ContainsKey("x") ? Properties["x"] : null;
        set => Properties["x"] = value;
    }
}