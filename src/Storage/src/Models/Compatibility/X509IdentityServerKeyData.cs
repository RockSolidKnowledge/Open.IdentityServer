// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Models;

/// <summary>
/// X509Cert class representing X509 cert data field in database
/// </summary>
public class X509IdentityServerKeyData: IdentityServerKeyData
{
    /// <summary>
    /// X509Cert raw pfx base64 encoded
    /// </summary>
    public string CertificateRawData { get; set; }
}