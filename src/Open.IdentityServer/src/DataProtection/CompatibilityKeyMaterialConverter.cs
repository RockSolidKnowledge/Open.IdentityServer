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
/// Deserializes CompatibilityKeyMaterial into SigningKey object
/// </summary>
/// <param name="dataProtectionProvider"></param>
public class CompatibilityKeyMaterialConverter(IDataProtectionProvider dataProtectionProvider)
{
    private IDataProtector dataProtector = dataProtectionProvider.CreateProtector("DataProtectionKeyProtector");
    
    public static readonly JsonSerializerOptions Settings = new()
    {
        //TODO: do we need any specific config
        IncludeFields = true,
    };

    /// <summary>
    /// Deserialized <see cref="CompatibilityKeyMaterial"/> into <see cref="SigningKey"/>
    /// </summary>
    /// <param name="keyMaterial">material to deserialize </param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public SigningKey Convert(CompatibilityKeyMaterial keyMaterial)
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
            var keyData = JsonSerializer.Deserialize<RsaCompatibilityKeyData>(unprotectedData, Settings);

            signingKey.Created = keyData.Created;
            signingKey.Credentials = new SigningCredentials(new RsaSecurityKey(keyData.Parameters) { KeyId = keyData.Id }, keyData.Algorithm);
        }
        
        if (!keyMaterial.IsX509Certificate && keyMaterial.Algorithm.StartsWith('E'))
        {
            var keyData = JsonSerializer.Deserialize<EcCompatibilityKeyData>(unprotectedData, Settings);

            signingKey.Created = keyData.Created;

            ECCurve curve = keyMaterial.Algorithm switch
            {
                "ES256" => ECCurve.NamedCurves.nistP256,
                "ES384" => ECCurve.NamedCurves.nistP384,
                "ES521" => ECCurve.NamedCurves.nistP521,
                _ => throw new ArgumentOutOfRangeException("Unexpected algorith value for EC Curve")
            };

            var ecdsa = ECDsa.Create(new ECParameters { Curve = curve, D = keyData.D, Q = keyData.Q });
            
            signingKey.Credentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa) { KeyId = keyData.Id }, keyData.Algorithm);
        }

        if (keyMaterial.IsX509Certificate)
        {
            var keyData = JsonSerializer.Deserialize<X509CompatibilityKeyData>(unprotectedData, Settings);
            
            var cert = X509CertificateLoader.LoadPkcs12(System.Convert.FromBase64String(keyData.CertificateRawData), null);
            
            signingKey.Credentials = new SigningCredentials(new X509SecurityKey(cert), keyData.Algorithm);
        }
        
        return signingKey;
    }
}