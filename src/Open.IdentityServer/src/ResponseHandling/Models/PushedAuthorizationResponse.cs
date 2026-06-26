using System;
using System.Text.Json.Serialization;

namespace Open.IdentityServer.ResponseHandling;

/// <summary>
/// 
/// </summary>
/// <param name="uri">The URI that represents the PAR</param>
/// <param name="lifetime">The lifetime of the URI in seconds</param>
public class PushedAuthorizationResponse(Uri uri , long lifetime)
{
       /// <summary>
       /// The URN to send to the authorization endpoint to obtain the authcode, instead of parametes
       /// </summary>
       [JsonPropertyName("request_uri")]
       public string Uri { get; } = uri.ToString();
       
       /// <summary>
       /// The lifetime in seconds of the URN
       /// </summary>
       [JsonPropertyName("expires_in")]
       public long Lifetime { get; } = lifetime;
}