// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Text.Json.Serialization;

namespace Open.IdentityServer.ResponseHandling;

/// <summary>
/// Represents the JSON object for a successful PAR result
/// </summary>
/// <param name="uri">The URI that represents the PAR</param>
/// <param name="lifetime">The lifetime of the URI in seconds</param>
public class PushedAuthorizationResponse(Uri uri , long lifetime)
{
       /// <summary>
       /// The URN to send to the authorization endpoint to obtain the authcode, instead of parametes
       /// </summary>
       [JsonPropertyName(OidcConstants.AuthorizeRequest.RequestUri)]
       public string Uri { get; } = uri.ToString();
       
       /// <summary>
       /// The lifetime in seconds of the URN
       /// </summary>
       [JsonPropertyName(OidcConstants.AuthorizeResponse.ExpiresIn)]
       public long Lifetime { get; } = lifetime;
}