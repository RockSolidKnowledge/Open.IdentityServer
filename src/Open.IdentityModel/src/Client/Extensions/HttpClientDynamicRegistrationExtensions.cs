// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityModel.Internal;
using Open.IdentityModel.Jwk;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Open.IdentityModel.Client;

/// <summary>
/// HttpClient extensions for dynamic registration
/// </summary>
public static class HttpClientDynamicRegistrationExtensions
{
    /// <summary>
    /// Send a dynamic registration request.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to a <see cref="DynamicClientRegistrationResponse"/> containing the registered client's metadata and credentials, or error details if the request failed.</returns>
    public static async Task<DynamicClientRegistrationResponse> RegisterClientAsync(
        this HttpMessageInvoker client, DynamicClientRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var clone = request.Clone();

        clone.Method = HttpMethod.Post;
        clone.Content = new StringContent(
            JsonSerializer.Serialize(request.Document, ClientMessagesSourceGenerationContext.Default.DynamicClientRegistrationDocument),
            Encoding.UTF8,
            "application/json");
        clone.Prepare();

        if (request.Token.IsPresent())
        {
            clone.SetBearerToken(request.Token!);
        }

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(clone, cancellationToken).ConfigureAwait();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ProtocolResponse.FromException<DynamicClientRegistrationResponse>(ex);
        }

        return await ProtocolResponse.FromHttpResponseAsync<DynamicClientRegistrationResponse>(response).ConfigureAwait();
    }
}