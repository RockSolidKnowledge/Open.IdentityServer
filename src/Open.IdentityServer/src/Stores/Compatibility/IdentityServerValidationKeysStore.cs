// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Duende compatibility validation key store
/// </summary>
/// <param name="identityServerKeyStore">The key store used to retrieve all available signing keys.</param>
/// <param name="dataProtectedIdentityServerKeyMaterialConverter">Converter used to decrypt and convert data-protected key material into usable credentials.</param>
public class IdentityServerValidationKeysStore(
    IIdentityServerKeyStore identityServerKeyStore, 
    DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter): IValidationKeysStore
{
    /// <summary>
    /// Gets all keys to be used for validation in the Duende key store, will be any other keys that are not retired, or the
    /// newest
    /// </summary>
    /// <returns>list of validation keys info</returns>
    public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        return identityServerKeyStore.GetKeys()
            .Where(x => x.Use == "signing")
            .Select(dataProtectedIdentityServerKeyMaterialConverter.Convert)
            .OrderByDescending(x => x.Created)
            .Select(x => new SecurityKeyInfo { Key = x.Credentials.Key, SigningAlgorithm = x.Credentials.Algorithm })
            .ToList();
    }
}