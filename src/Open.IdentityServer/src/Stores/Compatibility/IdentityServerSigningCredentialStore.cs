// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Duende compatibility signing key store
/// </summary>
/// <param name="identityServerKeyStore">The key store used to retrieve signing keys.</param>
/// <param name="dataProtectedIdentityServerKeyMaterialConverter">Converter used to decrypt and convert data-protected key material into usable credentials.</param>
/// <param name="timeProvider">Time provider implementation</param>
/// <param name="options">Compatibility key store options</param>
public class IdentityServerSigningCredentialStore(
    IIdentityServerKeyStore identityServerKeyStore,
    DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter,
    TimeProvider timeProvider,
    CompatibilityKeyStoreOptions options
    ): 
    IdentityServerSigningKeyStore(identityServerKeyStore, dataProtectedIdentityServerKeyMaterialConverter, timeProvider, options), 
    ISigningCredentialStore
{

    /// <summary>
    /// Gets the key to be used for signing from the key store, will be the newest key 
    /// </summary>
    /// <returns>a key to be used for signing</returns>
    public async Task<SigningCredentials> GetSigningCredentialsAsync()
    {
        return GetKeys()
            .Select(x => x.Credentials)
            .FirstOrDefault();
    }
}