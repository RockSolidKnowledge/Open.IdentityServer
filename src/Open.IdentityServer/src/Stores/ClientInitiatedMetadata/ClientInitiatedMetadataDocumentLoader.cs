#nullable enable
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Storage.Services;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Loads and maps a client metadata document
/// </summary>
/// <param name="mapper">The mapper to convert the document to a client</param>
/// <param name="documentClient">Client used to obtain the document</param>
/// <param name="logger">logs any issues</param>
public class ClientInitiatedMetadataDocumentLoader(
    IMapClientMetadataDocumentToClient mapper,
    IClientMetadataDocumentClient documentClient,
    ILogger<ClientInitiatedMetadataDocumentLoader> logger
    ) : IClientInitiatedMetadataDocumentLoader
{
    /// <summary>
    /// Loads a client configuration from a client initiated metadata reference
    /// </summary>
    /// <param name="clientReference">the uri to the client metadata document</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Returns the loaded client or null if failed</returns>
    public async ValueTask<Client?> Load(Uri clientReference , CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Attempting to load {clientReference}",clientReference);
            return await LoadClient(clientReference, ct);
        }
        catch (ClientMetadataDocumentGetException e)
        {
            logger.LogError(e,"Failed to load client metadata document from {clientReference}",clientReference);
            return null;
        }
        catch (JsonException e)
        {
            logger.LogError(e,"Failed to deserialize client metadata document from {clientReference}",clientReference);
            return null;
        }
    }

    private async ValueTask<Client?> LoadClient(Uri clientReference, CancellationToken ct)
    {
        string documentAsText = await documentClient.GetDocument(clientReference, ct);
            
        ClientMetadataDocument? document = JsonSerializer.Deserialize<ClientMetadataDocument>(documentAsText);

        if (document == null)
        {
            return null;
        }

        Client client = mapper.Map(document);

        return client;
    }
}