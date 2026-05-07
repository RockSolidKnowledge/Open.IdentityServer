// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Open.IdentityModel.Client;

/// <summary>
/// Models an OpenID Connect dynamic client registration response
/// </summary>
/// <seealso cref="IdentityModel.Client.ProtocolResponse" />
public class DynamicClientRegistrationResponse : ProtocolResponse
{
    /// <summary>Gets a human-readable description of the error, if present.</summary>
    public string? ErrorDescription         => Json?.TryGetString("error_description");
    /// <summary>Gets the client identifier issued by the authorization server.</summary>
    public string? ClientId                 => Json?.TryGetString(OidcConstants.RegistrationResponse.ClientId);
    /// <summary>Gets the client secret issued by the authorization server, or <see langword="null"/> if none was issued.</summary>
    public string? ClientSecret             => Json?.TryGetString(OidcConstants.RegistrationResponse.ClientSecret);
    /// <summary>Gets the registration access token that can be used to perform subsequent operations on the client registration.</summary>
    public string? RegistrationAccessToken  => Json?.TryGetString(OidcConstants.RegistrationResponse.RegistrationAccessToken);
    /// <summary>Gets the URI of the client configuration endpoint for this client.</summary>
    public string? RegistrationClientUri    => Json?.TryGetString(OidcConstants.RegistrationResponse.RegistrationClientUri);
    /// <summary>Gets the time at which the client identifier was issued, as Unix time.</summary>
    public long? ClientIdIssuedAt           => Json?.TryGetInt(OidcConstants.RegistrationResponse.ClientIdIssuedAt);
    /// <summary>Gets the time at which the client secret expires, as Unix time. A value of 0 indicates the secret does not expire.</summary>
    public long? ClientSecretExpiresAt      => Json?.TryGetInt(OidcConstants.RegistrationResponse.ClientSecretExpiresAt);
    /// <summary>Gets the software statement included in the registration request, if present.</summary>
    public string? SoftwareStatement        => Json?.TryGetString(OidcConstants.RegistrationResponse.SoftwareStatement);
}