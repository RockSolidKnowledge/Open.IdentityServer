// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Cryptography.X509Certificates;


namespace Open.IdentityModel;

/// <summary>
/// Provides entry points for accessing X.509 certificate stores by location.
/// </summary>
public static class X509
{
    /// <summary>Gets an accessor for the current user's X.509 certificate store.</summary>
    public static X509CertificatesLocation CurrentUser => new X509CertificatesLocation(StoreLocation.CurrentUser);
    /// <summary>Gets an accessor for the local machine's X.509 certificate store.</summary>
    public static X509CertificatesLocation LocalMachine => new X509CertificatesLocation(StoreLocation.LocalMachine);
}