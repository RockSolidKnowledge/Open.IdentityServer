// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// Interface for the key material service
/// </summary>
public interface IKeyMaterialService
{
    /// <summary>
    /// Gets all validation keys.
    /// </summary>
    /// <returns>
    /// A task that resolves to a collection of validation keys used for token signature validation.
    /// </returns>
    Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync();

    /// <summary>
    /// Gets the signing credentials.
    /// </summary>
    /// <param name="allowedAlgorithms">Collection of algorithms used to filter the server supported algorithms. 
    /// A value of null or empty indicates that the server default should be returned.</param>
    /// <returns>
    /// A task that resolves to the signing credentials matching the allowed algorithms, or the server default if none are specified.
    /// </returns>
    Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null);

    /// <summary>
    /// Gets all signing credentials.
    /// </summary>
    /// <returns>
    /// A task that resolves to a collection of all available signing credentials.
    /// </returns>
    Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync();
}