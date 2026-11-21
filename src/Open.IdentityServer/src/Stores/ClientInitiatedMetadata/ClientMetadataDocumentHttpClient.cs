#nullable enable
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Loads a client metadata document using HTTP 
/// </summary>
public class ClientMetadataDocumentHttpClient(HttpClient client) : IClientMetadataDocumentClient
{
    /// <summary>
    /// Load the metadata document from the provided URL
    /// </summary>
    /// <param name="metadataUrl">The location of the metadata document</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>the metadata document</returns>
    /// <exception cref="ClientMetadataDocumentGetException">thrown when an error occurs loading the document</exception>
    public async Task<string> GetDocument(Uri metadataUrl, CancellationToken ct = default)
    {
        try
        {
            string document = await client.GetStringAsync(metadataUrl, ct);

            return document;
        }
        catch (HttpRequestException e)
        {
            throw new ClientMetadataDocumentGetException($"Failed to load {metadataUrl} {e.Message}", e);
        }
    }
}