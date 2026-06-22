// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Models;
using System.Linq;
using System;
using Open.IdentityServer.Extensions;

namespace Open.IdentityServer.Services;

/// <summary>
/// The default key material service
/// </summary>
/// <seealso cref="Open.IdentityServer.Services.IKeyMaterialService" />
public class DefaultKeyMaterialService : IKeyMaterialService
{
    private readonly IEnumerable<ISigningCredentialStore> _signingCredentialStores;
    private readonly IEnumerable<IValidationKeysStore> _validationKeysStores;
    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultKeyMaterialService"/> class.
    /// </summary>
    /// <param name="validationKeysStores">The validation keys stores.</param>
    /// <param name="signingCredentialStores">The signing credential store.</param>
    /// <param name="telemetry">The telemetry service.</param>
    public DefaultKeyMaterialService(
        IEnumerable<IValidationKeysStore> validationKeysStores, 
        IEnumerable<ISigningCredentialStore> signingCredentialStores, 
        ITelemetryService telemetry)
    {
        _signingCredentialStores = signingCredentialStores;
        _telemetry = telemetry;
        _validationKeysStores = validationKeysStores;
    }

    /// <inheritdoc/>
    public async Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null)
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        if (_signingCredentialStores.Any())
        {
            if (allowedAlgorithms.IsNullOrEmpty())
            {
                return await _signingCredentialStores.First().GetSigningCredentialsAsync();
            }

            var credential = (await GetAllSigningCredentialsAsync()).FirstOrDefault(c => allowedAlgorithms.Contains(c.Algorithm));
            if (credential is null)
            {
                throw new InvalidOperationException($"No signing credential for algorithms ({allowedAlgorithms.ToSpaceSeparatedString()}) registered.");
            }

            return credential;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        var credentials = new List<SigningCredentials>();

        foreach (var store in _signingCredentialStores)
        {
            credentials.Add(await store.GetSigningCredentialsAsync());
        }

        return credentials;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        var keys = new List<SecurityKeyInfo>();

        foreach (var store in _validationKeysStores)
        {
            keys.AddRange(await store.GetValidationKeysAsync());
        }

        return keys;
    }
}