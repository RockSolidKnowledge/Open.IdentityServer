using System.Collections.Generic;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.EntityFramework.Stores;

/// <summary>
/// Duende key material store
/// </summary>
public class CompatibilityKeyStore: ICompatibilityKeyStore
{
    /// <summary>
    /// Gets all keys stored in Duende key store
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CompatibilityKeyMaterial> GetKeys()
    {
        throw new System.NotImplementedException();
    }
}