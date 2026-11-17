// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Open.IdentityServer.Configuration;

/// <summary>
/// Options for Mutual TLS features
/// </summary>
public class MutualTlsOptions
{
    /// <summary>
    /// Specifies if MTLS support should be enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Specifies the name of the authentication handler for X.509 client certificates.
    /// This scheme is required for MTLS client authentication: a client certificate is only accepted as a
    /// credential once this scheme has authenticated it. It is normally registered with
    /// <c>AddAuthentication().AddCertificate()</c> and is responsible for validating the certificate chain,
    /// validity period and revocation.
    /// </summary>
    public string ClientCertificateAuthenticationScheme { get; set; } = "Certificate";

    /// <summary>
    /// Specifies a separate domain to run the MTLS endpoints on.
    /// If the string does not contain any dots, a subdomain is assumed - e.g. main domain: identityserver.local, MTLS domain: mtls.identityserver.local
    /// If the string contains dots, a completely separate domain is assumend, e.g. main domain: identity.app.com, MTLS domain: mtls.app.com. In this case you must set a static issuer name on the options.
    /// </summary>
    public string DomainName { get; set; }

    /// <summary>
    /// Specifies whether a cnf claim gets emitted for access tokens if a client certificate was present.
    /// Normally the cnf claims only gets emitted if the client used the client certificate for authentication,
    /// setting this to true, will set the claim regardless of the authentication method. (defaults to false).
    /// </summary>
    public bool AlwaysEmitConfirmationClaim { get; set; }
}