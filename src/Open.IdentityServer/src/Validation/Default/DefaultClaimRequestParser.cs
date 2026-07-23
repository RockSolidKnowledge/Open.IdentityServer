// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <inheritdoc/>
public class DefaultClaimRequestParser : IClaimRequestParser
{
    private readonly ILogger<DefaultClaimRequestParser> _logger;
    
    private readonly string[] _topLevelKeys = new[]
    {
        OidcConstants.ClaimRequestKeys.UserInfo,
        OidcConstants.ClaimRequestKeys.IdToken
    };

    /// <summary>
    /// Creates a new instance of <see cref="DefaultClaimRequestParser"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public DefaultClaimRequestParser(ILogger<DefaultClaimRequestParser> logger)
    {
        _logger = logger;
    }

    private static readonly JsonSerializerOptions ClaimRequestOptions = new()
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    
    /// <inheritdoc/>
    public ParsedClaimsRequest Parse(string claimsRequest)
    {
        var result = new ParsedClaimsRequest();

        if (!TryParseJson(claimsRequest, out var document))
        {
            _logger.LogError("Invalid JSON in claims request: {ClaimsRequest}", claimsRequest);
            result.Error = OidcConstants.AuthorizeErrors.InvalidRequest;
            return result;
        }

        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (!_topLevelKeys.Contains(property.Name))
            {
                _logger.LogInformation("Skipping unknown top-level key in claims request: {Key}", property.Name);
                continue;
            }
            
            if (!TryParseClaimRequests(property, out var claimRequests))
            {
                _logger.LogError("Invalid claim requests in {Property} claims request: {ClaimsRequest}", property.Name, property.Value);
                result.Error = OidcConstants.AuthorizeErrors.InvalidRequest;
                return result;
            }
            
            if (property.Name == OidcConstants.ClaimRequestKeys.UserInfo)
            {
                result.UserInfoClaims = claimRequests;
            }
            else if (property.Name == OidcConstants.ClaimRequestKeys.IdToken)
            {
                result.IdTokenClaims = claimRequests;
            }
        }

        return result;
    }
    
    private bool TryParseJson(string claimsRequest, out JsonDocument document)
    {
        document = null;
        try
        {
            document = JsonDocument.Parse(claimsRequest);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool TryParseClaimRequests(JsonProperty json, out Dictionary<string, ClaimRequest> claimRequests)
    {
        claimRequests = new Dictionary<string, ClaimRequest>();
        
        if (json.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var parsedClaimRequests = json.Value.Deserialize<Dictionary<string, ClaimRequest>>(ClaimRequestOptions);
        if (parsedClaimRequests == null)
        {
            return false;
        }

        foreach (var kvp in parsedClaimRequests)
        {
            // Replace null with default voluntary claim request
            claimRequests[kvp.Key] = kvp.Value ?? new ClaimRequest();
        }

        return true;
    }
}