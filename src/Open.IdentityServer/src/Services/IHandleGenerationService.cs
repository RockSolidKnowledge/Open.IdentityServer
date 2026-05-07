// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// Interface for the handle generation service
/// </summary>
public interface IHandleGenerationService
{
    /// <summary>
    /// Generates a handle.
    /// </summary>
    /// <param name="length">The desired length of the generated handle. Defaults to 32.</param>
    /// <returns>
    /// A task that resolves to a unique handle string of the specified length.
    /// </returns>
    Task<string> GenerateAsync(int length = 32);
}