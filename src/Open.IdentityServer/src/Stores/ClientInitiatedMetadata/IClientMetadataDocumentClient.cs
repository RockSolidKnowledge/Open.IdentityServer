#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

#nullable enable

/// <summary>
/// Client abstraction for loading the metadata document
/// </summary>
public interface IClientMetadataDocumentClient
{
    /// <summary>
    /// Load the metadata document found at the supplied url
    /// </summary>
    /// <param name="metadataUrl">The URL to the metadata</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>the document or null if failed to load</returns>
    Task<string> GetDocument(Uri metadataUrl, CancellationToken ct = default);
}