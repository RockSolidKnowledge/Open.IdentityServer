namespace Open.IdentityServer.Models;

/// <summary>
/// X509Cert class representing X509 cert data field in database
/// </summary>
public class X509CompatibilityKeyData: CompatibilityKeyData
{
    /// <summary>
    /// 
    /// </summary>
    public string CertificateRawData { get; set; }
}