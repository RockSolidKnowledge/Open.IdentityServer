// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Models making HTTP requests for JWTs from the authorize endpoint.
    /// </summary>
    public class JwtRequestUriHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<JwtRequestUriHttpClient> _logger;

        /// <summary>
        /// Constructor for DefaultJwtRequestUriHttpClient.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public JwtRequestUriHttpClient(HttpClient client, ILogger<JwtRequestUriHttpClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Gets a JWT from the url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> GetJwtAsync(string url)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(req);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogDebug("Success http response from jwt url {url}", url);

                var json = await response.Content.ReadAsStringAsync();
                return json;
            }

            _logger.LogError("Invalid http status code {status} from jwt url {url}", response.StatusCode, url);

            return null;
        }
    }
}
