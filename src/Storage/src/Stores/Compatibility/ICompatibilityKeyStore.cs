#nullable enable

using System.Collections.Generic;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Retrieval of key material
/// </summary>
public interface ICompatibilityKeyStore
{
    /// <summary>
    /// Get all key material stored in databased
    /// </summary>
    /// <returns></returns>
    IEnumerable<CompatibilityKeyMaterial> GetKeys();
}