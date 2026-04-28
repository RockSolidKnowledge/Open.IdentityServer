using System.Security.Cryptography;

namespace Open.IdentityServer.Models;

/// <summary>
/// RsaKey class representing RSA key data field in database
/// </summary>
public class RsaCompatibilityKeyData: CompatibilityKeyData
{
    /// <summary>
    /// Get or set value of RSA parameters
    /// </summary>
    public RSAParameters Parameters { get; set; }
}


