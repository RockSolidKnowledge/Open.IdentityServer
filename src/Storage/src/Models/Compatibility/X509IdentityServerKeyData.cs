namespace Open.IdentityServer.Models;

/// <summary>
/// X509Cert class representing X509 cert data field in database
/// </summary>
public class X509IdentityServerKeyData: IdentityServerKeyData
{
    /// <summary>
    /// 
    /// </summary>
    public string CertificateRawData { get; set; }
}