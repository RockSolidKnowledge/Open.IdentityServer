// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Models making HTTP requests for back-channel logout notification.
    /// </summary>
    public class BackChannelLogoutHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<BackChannelLogoutHttpClient> _logger;

        /// <summary>
        /// Constructor for BackChannelLogoutHttpClient.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public BackChannelLogoutHttpClient(HttpClient client, ILogger<BackChannelLogoutHttpClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Posts the payload to the url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
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
}