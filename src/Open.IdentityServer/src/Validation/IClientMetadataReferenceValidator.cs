// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
#nullable enable
namespace Open.IdentityServer.Validation;

/// <summary>
/// Validates if a client id should be treated as a client metadata identifier
/// for remote retrieval of client metadata
/// </summary>
public interface IClientMetadataReferenceValidator
{
    /// <summary>
    /// Takes the client id of the inbound OAuth request, and determines
    /// if the client id is a reference to external client id metadata
    /// as opposed to client metadata stored in the underling client store
    ///
    /// This method is intended to be a quick check to see if it looks like one
    /// used to decide if to explore further or fallback to the internal store
    /// </summary>
    /// <param name="clientId">the client id from the OAuth request</param>
    /// <returns>true if it appears to be a reference to external metadata</returns>
    bool IsMetadataReference(string clientId);

    /// <summary>
    /// In depth verification of the reference, including any trust checks
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>true if the reference is valid</returns>
    ValueTask<bool> IsValidMetadataReference(string clientId);
}
