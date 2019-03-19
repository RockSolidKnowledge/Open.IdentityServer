// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Configuration
{
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
        /// Specifies the name of the authentication handler for X.509 client certificates
        /// </summary>
        public string ClientCertificateAuthenticationScheme { get; set; } = "Certificate";
    }
}