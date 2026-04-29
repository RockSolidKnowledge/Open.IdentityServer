using System.Collections.Generic;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.EntityFramework.Stores;

/// <summary>
/// Duende key material store
/// </summary>
public class IdentityServerKeyStore(IIdentityServerKeyDbContext dbContext): IIdentityServerKeyStore
{
    /// <summary>
    /// Gets all keys stored in Duende key store
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IdentityServerKeyMaterial> GetKeys() => dbContext.Keys;
}