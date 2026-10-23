// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
#nullable enable
using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// The level of trust given to a client metadata reference
/// </summary>
public enum ClientMetadataTrustResult
{
    /// <summary>
    /// You can trust the reference
    /// </summary>
    Trusted,
    /// <summary>
    /// You can not trust the reference, and should not attempt to follow it
    /// </summary>
    NotTrusted,
    
    /// <summary>
    /// Don't have an opinion 
    /// </summary>
    DontKnow
};

/// <summary>
/// Used to determine if a Client Metadata Reference can be used to retrieve client Metadata
/// </summary>
public interface IClientMetadataReferenceTrustPolicy
{
    /// <summary>
    /// Used to determine if the client metadata reference should be trusted
    /// Multiple implementations will be considered 
    /// </summary>
    /// <param name="reference"></param>
    /// <returns>true if it should be trusted, false if not</returns>
    ValueTask<bool> IsTrusted(Uri reference);
}