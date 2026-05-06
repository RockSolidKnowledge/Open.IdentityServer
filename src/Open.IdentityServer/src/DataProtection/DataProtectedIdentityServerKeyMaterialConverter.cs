// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Deserializes <see cref="IdentityServerKeyMaterial"/> into SigningKey object
/// </summary>
/// <param name="dataProtectionProvider"></param>
public class DataProtectedIdentityServerKeyMaterialConverter(IDataProtectionProvider dataProtectionProvider)
{
    private IDataProtector dataProtector = dataProtectionProvider.CreateProtector("DataProtectionKeyProtector");
    
    public static readonly JsonSerializerOptions Settings = new()
    {
        IncludeFields = true,
    };

    /// <summary>
    /// Deserialized <see cref="IdentityServerKeyMaterial"/> into <see cref="SigningKey"/>
    /// </summary>
    /// <param name="keyMaterial"> to deserialize </param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns><see cref="SigningKey"/> constructed using provided <see cref="IdentityServerKeyMaterial"/></returns>
    public SigningKey Convert(IdentityServerKeyMaterial keyMaterial)
    {
        var signingKey = new SigningKey
        {
            Id = keyMaterial.Id,
        };

        var unprotectedData = keyMaterial.DataProtected ? 
            dataProtector.Unprotect(keyMaterial.Data) : 
            keyMaterial.Data;

        if (!keyMaterial.IsX509Certificate &&
            (keyMaterial.Algorithm.StartsWith('R') || keyMaterial.Algorithm.StartsWith('P')))
        {
            var keyData = JsonSerializer.Deserialize<RsaIdentityServerKeyData>(unprotectedData, Settings);

            signingKey.Created = keyData.Created;
            signingKey.Credentials = new SigningCredentials(new RsaSecurityKey(keyData.Parameters) { KeyId = keyData.Id }, keyData.Algorithm);
        }
        
        if (!keyMaterial.IsX509Certificate && keyMaterial.Algorithm.StartsWith('E'))
        {
            var keyData = JsonSerializer.Deserialize<EcIdentityServerKeyData>(unprotectedData, Settings);

            signingKey.Created = keyData.Created;

            ECCurve curve = keyMaterial.Algorithm switch
            {
                "ES256" => CryptoHelper.GetCurveFromCrvValue("P-256"),
                "ES384" => CryptoHelper.GetCurveFromCrvValue("P-384"),
                "ES521" => CryptoHelper.GetCurveFromCrvValue("P-521"),
                _ => throw new ArgumentOutOfRangeException(nameof(keyMaterial.Algorithm), "Unexpected algorithm value for EC Curve")
            };

            var ecdsa = ECDsa.Create(new ECParameters { Curve = curve, D = keyData.D, Q = keyData.Q });
            
            signingKey.Credentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa) { KeyId = keyData.Id }, keyData.Algorithm);
        }

        if (keyMaterial.IsX509Certificate)
        {
            var keyData = JsonSerializer.Deserialize<X509IdentityServerKeyData>(unprotectedData, Settings);
            
            var cert = X509CertificateLoader.LoadPkcs12(System.Convert.FromBase64String(keyData.CertificateRawData), null);
            
            signingKey.Credentials = new SigningCredentials(new X509SecurityKey(cert), keyData.Algorithm);
        }
        
        return signingKey;
    }
}