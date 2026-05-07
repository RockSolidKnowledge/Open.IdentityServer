// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityModel.Internal;
using Open.IdentityModel.Jwk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Open.IdentityModel.Client;

/// <summary>
/// Models the response from an OpenID Connect discovery endpoint
/// </summary>
public class DiscoveryDocumentResponse : ProtocolResponse
{
    /// <summary>
    /// Gets or sets the discovery policy used to validate this response.
    /// </summary>
    public DiscoveryPolicy Policy { get; set; } = default!;
    
    /// <summary>
    /// Validates the discovery document against the policy and populates endpoint aliases.
    /// </summary>
    /// <param name="initializationData">A <see cref="DiscoveryPolicy"/> to validate against, or a <see cref="string"/> error message on non-success responses.</param>
    /// <returns>A task that completes when validation is finished and <see cref="MtlsEndpointAliases"/> has been populated.</returns>
    protected override Task InitializeAsync(object? initializationData = null)
    {
        if (HttpResponse?.IsSuccessStatusCode != true)
        {
            ErrorMessage = initializationData as string;
            return Task.CompletedTask;
        }

        Policy = initializationData as DiscoveryPolicy ?? new DiscoveryPolicy();

        var validationError = Validate(Policy);

        if (validationError.IsPresent())
        {
            Json = default;

            ErrorType = ResponseErrorType.PolicyViolation;
            ErrorMessage = validationError;
        }

        MtlsEndpointAliases =
            new MtlsEndpointAliases(Json?.TryGetValue(OidcConstants.Discovery.MtlsEndpointAliases));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets or sets the JSON web key set.
    /// </summary>
    /// <value>
    /// The key set.
    /// </value>
    public JsonWebKeySet? KeySet { get; set; }
        
    /// <summary>
    /// Gets the MTLS endpoint aliases
    /// </summary>
    /// <value>
    /// The MTLS endpoint aliases discovered from the discovery document.
    /// </value>
    public MtlsEndpointAliases? MtlsEndpointAliases { get; internal set; }
        
    /// <summary>Gets the issuer identifier.</summary>
    public string? Issuer => TryGetString(OidcConstants.Discovery.Issuer);
    
    /// <summary>Gets the URL of the authorization endpoint.</summary>
    public string? AuthorizeEndpoint => TryGetString(OidcConstants.Discovery.AuthorizationEndpoint);
    
    /// <summary>Gets the URL of the token endpoint.</summary>
    public string? TokenEndpoint => TryGetString(OidcConstants.Discovery.TokenEndpoint);
    
    /// <summary>Gets the URL of the userinfo endpoint.</summary>
    public string? UserInfoEndpoint => TryGetString(OidcConstants.Discovery.UserInfoEndpoint);
    
    /// <summary>Gets the URL of the introspection endpoint.</summary>
    public string? IntrospectionEndpoint => TryGetString(OidcConstants.Discovery.IntrospectionEndpoint);
    
    /// <summary>Gets the URL of the token revocation endpoint.</summary>
    public string? RevocationEndpoint => TryGetString(OidcConstants.Discovery.RevocationEndpoint);
    
    /// <summary>Gets the URL of the device authorization endpoint.</summary>
    public string? DeviceAuthorizationEndpoint => TryGetString(OidcConstants.Discovery.DeviceAuthorizationEndpoint);
    
    /// <summary>Gets the URL of the backchannel authentication endpoint.</summary>
    public string? BackchannelAuthenticationEndpoint => TryGetString(OidcConstants.Discovery.BackchannelAuthenticationEndpoint);
    
    /// <summary>Gets the URL of the JSON Web Key Set document.</summary>
    public string? JwksUri => TryGetString(OidcConstants.Discovery.JwksUri);
    
    /// <summary>Gets the URL of the end session endpoint.</summary>
    public string? EndSessionEndpoint => TryGetString(OidcConstants.Discovery.EndSessionEndpoint);
    
    /// <summary>Gets the URL of the check session iframe.</summary>
    public string? CheckSessionIframe => TryGetString(OidcConstants.Discovery.CheckSessionIframe);
    
    /// <summary>Gets the URL of the client registration endpoint.</summary>
    public string? RegistrationEndpoint => TryGetString(OidcConstants.Discovery.RegistrationEndpoint);
    
    /// <summary>Gets the URL of the pushed authorization request endpoint.</summary>
    public string? PushedAuthorizationRequestEndpoint => TryGetString(OidcConstants.Discovery.PushedAuthorizationRequestEndpoint);
    
    /// <summary>Gets a value indicating whether front-channel logout is supported.</summary>
    public bool? FrontChannelLogoutSupported => TryGetBoolean(OidcConstants.Discovery.FrontChannelLogoutSupported);
    
    /// <summary>Gets a value indicating whether front-channel logout session management is supported.</summary>
    public bool? FrontChannelLogoutSessionSupported => TryGetBoolean(OidcConstants.Discovery.FrontChannelLogoutSessionSupported);
    
    /// <summary>Gets the grant types supported by the authorization server.</summary>
    public IEnumerable<string> GrantTypesSupported => TryGetStringArray(OidcConstants.Discovery.GrantTypesSupported);
    
    /// <summary>Gets the PKCE code challenge methods supported by the authorization server.</summary>
    public IEnumerable<string> CodeChallengeMethodsSupported => TryGetStringArray(OidcConstants.Discovery.CodeChallengeMethodsSupported);
    
    /// <summary>Gets the OAuth scopes supported by the authorization server.</summary>
    public IEnumerable<string> ScopesSupported => TryGetStringArray(OidcConstants.Discovery.ScopesSupported);
    
    /// <summary>Gets the subject identifier types supported by the authorization server.</summary>
    public IEnumerable<string> SubjectTypesSupported => TryGetStringArray(OidcConstants.Discovery.SubjectTypesSupported);
    
    /// <summary>Gets the OAuth response modes supported by the authorization server.</summary>
    public IEnumerable<string> ResponseModesSupported => TryGetStringArray(OidcConstants.Discovery.ResponseModesSupported);
    
    /// <summary>Gets the OAuth response types supported by the authorization server.</summary>
    public IEnumerable<string> ResponseTypesSupported => TryGetStringArray(OidcConstants.Discovery.ResponseTypesSupported);
    
    /// <summary>Gets the claims supported by the authorization server.</summary>
    public IEnumerable<string> ClaimsSupported => TryGetStringArray(OidcConstants.Discovery.ClaimsSupported);
    
    /// <summary>Gets the client authentication methods supported by the token endpoint.</summary>
    public IEnumerable<string> TokenEndpointAuthenticationMethodsSupported => TryGetStringArray(OidcConstants.Discovery.TokenEndpointAuthenticationMethodsSupported);
    
    /// <summary>Gets the backchannel token delivery modes supported by the authorization server.</summary>
    public IEnumerable<string> BackchannelTokenDeliveryModesSupported => TryGetStringArray(OidcConstants.Discovery.BackchannelTokenDeliveryModesSupported);
    
    /// <summary>Gets a value indicating whether the backchannel user code parameter is supported.</summary>
    public bool? BackchannelUserCodeParameterSupported => TryGetBoolean(OidcConstants.Discovery.BackchannelUserCodeParameterSupported);
    
    /// <summary>Gets a value indicating whether pushed authorization requests are required.</summary>
    public bool? RequirePushedAuthorizationRequests => TryGetBoolean(OidcConstants.Discovery.RequirePushedAuthorizationRequests);

    // generic
    /// <summary>
    /// Tries to get a raw JSON element value from the discovery document by name.
    /// </summary>
    /// <param name="name">The property name to look up.</param>
    /// <returns>The <see cref="JsonElement"/> for the property, or <see langword="null"/> if not present.</returns>
    public JsonElement? TryGetValue(string name) => Json?.TryGetValue(name);
    
    /// <summary>
    /// Tries to get a string value from the discovery document by name.
    /// </summary>
    /// <param name="name">The property name to look up.</param>
    /// <returns>The string value, or <see langword="null"/> if not present.</returns>
    public string? TryGetString(string name) => Json?.TryGetString(name);
    
    /// <summary>
    /// Tries to get a boolean value from the discovery document by name.
    /// </summary>
    /// <param name="name">The property name to look up.</param>
    /// <returns>The boolean value, or <see langword="null"/> if not present.</returns>
    public bool? TryGetBoolean(string name) => Json?.TryGetBoolean(name);
    
    /// <summary>
    /// Tries to get a string array from the discovery document by name.
    /// </summary>
    /// <param name="name">The property name to look up.</param>
    /// <returns>The array of strings, or an empty array if not present.</returns>
    public IEnumerable<string> TryGetStringArray(string name) => Json?.TryGetStringArray(name) ?? Array.Empty<string>();

    private string Validate(DiscoveryPolicy policy)
    {
        if (policy.ValidateIssuerName)
        {
            IAuthorityValidationStrategy strategy = policy.AuthorityValidationStrategy ?? DiscoveryPolicy.DefaultAuthorityValidationStrategy;

            AuthorityValidationResult issuerValidationResult = strategy.IsIssuerNameValid(Issuer!, policy.Authority);

            if (!issuerValidationResult.Success)
            {
                return issuerValidationResult.ErrorMessage;
            }
        }

        var error = ValidateEndpoints(Json, policy);
        if (error.IsPresent())
        {
            return error;
        }

        return string.Empty;
    }

    /// <summary>
    /// Checks if the issuer matches the authority.
    /// </summary>
    /// <param name="issuer">The issuer.</param>
    /// <param name="authority">The authority.</param>
    /// <returns><see langword="true"/> if the issuer name is valid for the given authority using the default validation strategy; otherwise, <see langword="false"/>.</returns>
    public bool ValidateIssuerName(string issuer, string authority)
    {
        return DiscoveryPolicy.DefaultAuthorityValidationStrategy.IsIssuerNameValid(issuer, authority).Success;
    }

    /// <summary>
    /// Checks if the issuer matches the authority.
    /// </summary>
    /// <param name="issuer">The issuer.</param>
    /// <param name="authority">The authority.</param>
    /// <param name="nameComparison">The comparison mechanism that should be used when performing the match.</param>
    /// <returns><see langword="true"/> if the issuer name matches the authority using the specified string comparison; otherwise, <see langword="false"/>.</returns>
    public bool ValidateIssuerName(string issuer, string authority, StringComparison nameComparison)
    {
        return new StringComparisonAuthorityValidationStrategy(nameComparison).IsIssuerNameValid(issuer, authority).Success;
    }

    /// <summary>
    /// Checks if the issuer matches the authority.
    /// </summary>
    /// <param name="issuer">The issuer.</param>
    /// <param name="authority">The authority.</param>
    /// <param name="validationStrategy">The strategy to use.</param>
    /// <returns><see langword="true"/> if the issuer name is valid for the given authority; otherwise, <see langword="false"/>.</returns>
    private bool ValidateIssuerName(string issuer, string authority, IAuthorityValidationStrategy validationStrategy)
    {
        return validationStrategy.IsIssuerNameValid(issuer, authority).Success;
    }
    
    /// <summary>
    /// Validates the endpoints and jwks_uri according to the security policy.
    /// </summary>
    /// <param name="json">The json.</param>
    /// <param name="policy">The policy.</param>
    /// <returns>An error message string if any endpoint fails validation; otherwise, an empty string.</returns>
    public string ValidateEndpoints(JsonElement? json, DiscoveryPolicy policy)
    {
        if(json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        // allowed hosts
        var allowedHosts = new HashSet<string>(policy.AdditionalEndpointBaseAddresses.Select(e => new Uri(e).Authority))
        {
            new Uri(policy.Authority).Authority
        };

        // allowed authorities (hosts + base address)
        var allowedAuthorities = new HashSet<string>(policy.AdditionalEndpointBaseAddresses)
        {
            policy.Authority
        };
            
        // Can't actually be null, because we check that and throw at the beginning of the method
        foreach (var element in json?.EnumerateObject()!) 
        {
            if (element.Name.EndsWith("endpoint", StringComparison.OrdinalIgnoreCase) ||
                element.Name.Equals(OidcConstants.Discovery.JwksUri, StringComparison.OrdinalIgnoreCase) ||
                element.Name.Equals(OidcConstants.Discovery.CheckSessionIframe, StringComparison.OrdinalIgnoreCase))
            {
                var endpoint = element.Value.ToString();
            
                var isValidUri = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri);
                if (!isValidUri)
                {
                    return $"Malformed endpoint: {endpoint}";
                }
            
                if (!DiscoveryEndpoint.IsValidScheme(uri!))
                {
                    return $"Malformed endpoint: {endpoint}";
                }
            
                if (!DiscoveryEndpoint.IsSecureScheme(uri!, policy))
                {
                    return $"Endpoint does not use HTTPS: {endpoint}";
                }
            
                if (policy.ValidateEndpoints)
                {
                    // if endpoint is on exclude list, don't validate
                    if (policy.EndpointValidationExcludeList.Contains(element.Name))
                    {
                        continue;
                    }
            
                    bool isAllowed = false;
                    foreach (var host in allowedHosts)
                    {
                        if (string.Equals(host, uri!.Authority))
                        {
                            isAllowed = true;
                        }
                    }
            
                    if (!isAllowed)
                    {
                        return $"Endpoint is on a different host than authority: {endpoint}";
                    }
            
                    IAuthorityValidationStrategy strategy = policy.AuthorityValidationStrategy ?? DiscoveryPolicy.DefaultAuthorityValidationStrategy;
                    AuthorityValidationResult endpointValidationResult = strategy.IsEndpointValid(endpoint, allowedAuthorities);
                    if (!endpointValidationResult.Success)
                    {
                        return endpointValidationResult.ErrorMessage;
                    }
                }
            }
        }

        if (policy.RequireKeySet)
        {
            if (string.IsNullOrWhiteSpace(JwksUri))
            {
                return "Keyset is missing";
            }
        }

        return string.Empty;
    }
}