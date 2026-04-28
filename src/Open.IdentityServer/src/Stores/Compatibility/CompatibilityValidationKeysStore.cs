using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Duende compatibility validation key store
/// </summary>
/// <param name="compatibilityKeyStore"/>
public class CompatibilityValidationKeysStore(
    ICompatibilityKeyStore compatibilityKeyStore, 
    CompatibilityKeyMaterialConverter compatibilityKeyMaterialConverter): IValidationKeysStore
{
    /// <summary>
    /// Gets all keys to be used for validation in the Duende key store, will be any other keys that are not retired, or the
    /// newest
    /// </summary>
    /// <returns>list of validation keys info</returns>
    public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        return compatibilityKeyStore.GetKeys()
            .Where(x => x.Use == "signing")
            .Select(compatibilityKeyMaterialConverter.Convert)
            .OrderByDescending(x => x.Created)
            .Select(x => new SecurityKeyInfo { Key = x.Credentials.Key, SigningAlgorithm = x.Credentials.Algorithm })
            .Skip(1)
            .ToList();
    }
}