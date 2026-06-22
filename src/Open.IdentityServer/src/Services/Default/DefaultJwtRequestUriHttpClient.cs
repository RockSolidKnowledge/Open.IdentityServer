// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Open.IdentityServer.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;

namespace Open.IdentityServer.Services;

/// <summary>
/// Default JwtRequest client
/// </summary>
public class DefaultJwtRequestUriHttpClient : IJwtRequestUriHttpClient
{
    private readonly HttpClient _client;
    private readonly IdentityServerOptions _options;
    private readonly ITelemetryService _telemetry;
    private readonly ILogger<DefaultJwtRequestUriHttpClient> _logger;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="client">An HTTP client</param>
    /// <param name="options">The options.</param>
    /// <param name="telemetry">The telemetry service.</param>
    /// <param name="loggerFactory">The logger factory</param>
    public DefaultJwtRequestUriHttpClient(
        HttpClient client, 
        IdentityServerOptions options, 
        ITelemetryService telemetry,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _options = options;
        _telemetry = telemetry;
        _logger = loggerFactory.CreateLogger<DefaultJwtRequestUriHttpClient>();
    }

    /// <inheritdoc />
    public async Task<string> GetJwtAsync(string url, Client client)
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        var req = new HttpRequestMessage(HttpMethod.Get, url);
            
        req.Options.Set(new HttpRequestOptionsKey<Client>(IdentityServerConstants.JwtRequestClientKey), client);

        var response = await _client.SendAsync(req);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            if (_options.StrictJarValidation)
            {
                if (!string.Equals(response.Content.Headers.ContentType.MediaType,
                        $"application/{JwtClaimTypes.JwtTypes.AuthorizationRequest}", StringComparison.Ordinal))
                {
                    _logger.LogError("Invalid content type {type} from jwt url {url}", response.Content.Headers.ContentType.MediaType, url);
                    return null;
                }
            }

            _logger.LogDebug("Success http response from jwt url {url}", url);
                
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
                
        _logger.LogError("Invalid http status code {status} from jwt url {url}", response.StatusCode, url);
        return null;
    }
}