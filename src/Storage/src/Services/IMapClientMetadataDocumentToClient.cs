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
    /// 
    /// </summary>
    /// <param name="error"></param>
   /// <param name="innerException"></param>
    public InvalidClientMetadataDocumentException(string error,  Exception? innerException = null) : base(error, innerException)
    {
        
    }
}

/// <summary>
/// Maps a Client Metadata Document as per RFC to an
/// IdentityServer Client
/// </summary>
public interface IMapClientMetadataDocumentToClient
{
    /// <summary>
    /// Maps an instance of the Client Metadata Document as per RFC to an IdentityServer Client
    /// </summary>
    /// <param name="document">Client Metadata Document</param>
    /// <param name="internalPropertyPrefix">The prefix for all properties added to the client property bag that do not have a mapping to the Client object</param>
    /// <returns>Mapped Client</returns>
    public Client Map(ClientMetadataDocument document , string internalPropertyPrefix );
}