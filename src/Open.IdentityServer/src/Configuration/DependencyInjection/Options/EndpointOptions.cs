// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Configures which endpoints are enabled or disabled.
/// </summary>
public class EndpointsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the authorize endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the authorize endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableAuthorizeEndpoint { get; set; } = true;
        
    /// <summary>
    /// Gets or sets if JWT request_uri processing is enabled on the authorize endpoint. 
    /// </summary>
    public bool EnableJwtRequestUri { get; set; } = false;

    /// <summary>
    /// Gets or sets the supported JWT request signing algorithms.
    /// Defaults to RS256, RS384, RS512, PS256, PS384, PS512, ES256, ES384, ES512.
    /// A null or empty collection will omit the "request_object_signing_alg_values_supported" entry from the discovery document.
    /// </summary>
    public ICollection<string> SupportedJwtRequestSigningAlgorithms { get; set; } = [
        OidcConstants.Algorithms.Asymmetric.RS256,
        OidcConstants.Algorithms.Asymmetric.RS384,
        OidcConstants.Algorithms.Asymmetric.RS512,
        OidcConstants.Algorithms.Asymmetric.PS256,
        OidcConstants.Algorithms.Asymmetric.PS384,
        OidcConstants.Algorithms.Asymmetric.PS512,
        OidcConstants.Algorithms.Asymmetric.ES256,
        OidcConstants.Algorithms.Asymmetric.ES384,
        OidcConstants.Algorithms.Asymmetric.ES512
    ];

    /// <summary>
    /// Gets or sets a value indicating whether the token endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the token endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableTokenEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the user info endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user info endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableUserInfoEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the discovery document endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the disdovery document endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableDiscoveryEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the end session endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the end session endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableEndSessionEndpoint { get; set; } = true;
        
    /// <summary>
    /// Gets or sets a value indicating whether the check session endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the check session endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableCheckSessionEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the token revocation endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the token revocation endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableTokenRevocationEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the introspection endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the introspection endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableIntrospectionEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the device authorization endpoint is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the device authorization endpoint is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableDeviceAuthorizationEndpoint { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether the pushed authorization endpoint is enabled.
    /// </summary>
    public bool EnablePushedAuthorizationRequestEndpoint { get; set; } = true;
}