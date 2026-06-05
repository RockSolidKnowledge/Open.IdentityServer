// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// This class is the base implementation of the IdentityServer compatibility signing key store that retrieves legacy
/// from the existing IdentityServer read-only key store.
/// </summary>
/// <param name="identityServerKeyStore">The key store used to retrieve signing keys.</param>
/// <param name="dataProtectedIdentityServerKeyMaterialConverter">Converter used to decrypt and convert data-protected key material into usable credentials.</param>
/// <param name="timeProvider">Time provider implementation</param>
/// <param name="options">Compatibility key store options</param>
public abstract class IdentityServerSigningKeyStore(
    IIdentityServerKeyStore identityServerKeyStore,
    DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter,
    TimeProvider timeProvider,
    CompatibilityKeyStoreOptions options)
{
    private readonly IIdentityServerKeyStore identityServerKeyStore = identityServerKeyStore;
    private readonly DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter = dataProtectedIdentityServerKeyMaterialConverter;
    private readonly CompatibilityKeyStoreOptions options = options;
    
    /// <summary>
    /// Get all signing keys from the store, filtering based on the compatibility key store configuration and use
    /// </summary>
    /// <returns></returns>
    protected IEnumerable<SigningKey> GetKeys()
    {
        return identityServerKeyStore.GetKeys()
            .Where(x => x.Use == "signing")
            .Select(dataProtectedIdentityServerKeyMaterialConverter.Convert)
            .Where(x => x.Created.Add(options.MaxLifetime) > timeProvider.GetUtcNow())
            .OrderByDescending(x => x.Created);
    }
}