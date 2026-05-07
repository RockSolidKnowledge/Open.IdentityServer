// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Services;

/// <summary>
/// Provides functionality for sending HTTP POST requests to back-channel logout endpoints.
/// </summary>
public class DefaultBackChannelLogoutHttpClient : IBackChannelLogoutHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger<DefaultBackChannelLogoutHttpClient> _logger;

    /// <summary>
    /// Constructor for BackChannelLogoutHttpClient.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> used to send back-channel logout POST requests.</param>
    /// <param name="loggerFactory">The logger factory used to create a logger for this class.</param>
    public DefaultBackChannelLogoutHttpClient(HttpClient client, ILoggerFactory loggerFactory)
    {
        _client = client;
        _logger = loggerFactory.CreateLogger<DefaultBackChannelLogoutHttpClient>();
    }

    /// <summary>
    /// Posts the payload to the url.
    /// </summary>
    /// <param name="url">The back-channel logout endpoint URL to POST to.</param>
    /// <param name="payload">The form-URL-encoded key/value pairs to include in the request body.</param>
    public async Task PostAsync(string url, Dictionary<string, string> payload)
    {
        try
        {
            var response = await _client.PostAsync(url, new FormUrlEncodedContent(payload));
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Response from back-channel logout endpoint: {url} status code: {status}", url, (int)response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Response from back-channel logout endpoint: {url} status code: {status}", url, (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception invoking back-channel logout for url: {url}", url);
        }
    }
}