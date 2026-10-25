// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
#nullable enable
using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Used to determine if a Client Metadata Reference can be used to retrieve client Metadata
/// </summary>
public interface IClientMetadataReferenceTrustValidator
{
    /// <summary>
    /// Used to determine if the client metadata reference should be trusted
    /// Multiple implementations will be considered 
    /// </summary>
    /// <param name="reference"></param>
    /// <returns>true if it should be trusted, false if not</returns>
    ValueTask<bool> IsTrusted(Uri reference);
}