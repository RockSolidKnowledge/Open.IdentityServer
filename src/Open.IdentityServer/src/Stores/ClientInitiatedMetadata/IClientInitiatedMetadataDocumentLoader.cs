using System;
using System.Threading;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

#nullable enable
namespace Open.IdentityServer.Stores;

/// <summary>
/// Load the client via metadata document
/// </summary>
public interface IClientInitiatedMetadataDocumentLoader
{
    /// <summary>
    /// Loads a client based on the client metadata document, located by the client Reference
    /// </summary>
    /// <param name="clientReference">The URL to the client metadata document</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The client or null if failed to load a valid client</returns>
    public ValueTask<Client?> Load(Uri clientReference , CancellationToken ct = default);
}