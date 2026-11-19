// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;

namespace Open.IdentityServer.Storage.Services;
#nullable enable

/// <summary>
/// Thrown when it can't map the metadata document to a Client
/// </summary>
public class InvalidClientMetadataDocumentException  : Exception
{
    /// <summary>
    /// Thrown if an error occurs when mapping a Client Metadata Document to a Client
    /// </summary>
    /// <param name="error">The reason for the error</param>
   /// <param name="innerException">Any inner exception</param>
    public InvalidClientMetadataDocumentException(string error,  Exception? innerException = null) : base(error, innerException)
    {
        
    }
}

/// <summary>
/// Maps a Client Metadata Document as per RFC to an
/// IdentityServer Client </summary>
public interface IMapClientMetadataDocumentToClient
{
    /// <summary>
    /// Maps an instance of the Client Metadata Document as per RFC to an IdentityServer Client
    /// </summary>
    /// <param name="document">Client Metadata Document</param>
    /// <returns>Mapped Client</returns>
    public Client Map(ClientMetadataDocument document);
}