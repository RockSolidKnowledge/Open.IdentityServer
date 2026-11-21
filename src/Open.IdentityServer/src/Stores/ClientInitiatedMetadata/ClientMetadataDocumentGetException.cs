#nullable enable
using System;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Raised by Metadata Document Client
/// </summary>
public class ClientMetadataDocumentGetException : Exception
{
    /// <summary>
    /// Raised when failing to load the client metadata document
    /// </summary>
    /// <param name="message">Reason for the error</param>
    public ClientMetadataDocumentGetException(string message) : base(message)
    {
        
    }

    /// <summary>
    /// Raised when failing to load the client metadata document
    /// </summary>
    /// <param name="message">Reason for the error</param>
    /// <param name="innerException">Underlying exception</param>
    public ClientMetadataDocumentGetException(string message, Exception innerException) : base(message, innerException)
    {
        
    }
}