// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Factory for the creating signing credential store
/// </summary>
/// <param name="identityServerKeyStore">The key store used to retrieve signing keys.</param>
/// <param name="dataProtectedIdentityServerKeyMaterialConverter">Converter used to decrypt and convert data-protected key material into usable credentials.</param>
/// <param name="timeProvider">Time provider implementation</param>
/// <param name="options">Compatibility key store options</param>
/// <param name="telemetry">The telemetry service</param>
/// <param name="logger">The logger</param>
public class IdentityServerSigningCredentialStoreFactory(IIdentityServerKeyStore identityServerKeyStore,
    DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter,
    TimeProvider timeProvider,
    CompatibilityKeyStoreOptions options,
    ITelemetryService telemetry,
    ILogger<IdentityServerSigningCredentialStore> logger): IIdentityServerSigningCredentialStoreFactory
{
    /// <inheritdoc />
    public IdentityServerSigningCredentialStore Create(string alg)
    {
        return new IdentityServerSigningCredentialStore(identityServerKeyStore, dataProtectedIdentityServerKeyMaterialConverter, timeProvider, options, telemetry, logger)
        {
            Algorithm = alg
        };
    }
}