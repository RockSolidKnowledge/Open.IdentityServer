using System.Security.Cryptography;

namespace Open.IdentityServer.Models;

/// <summary>
/// EcKey class representing EC key data field in database
/// </summary>
public class EcIdentityServerKeyData: IdentityServerKeyData
{
    /// <summary>
    /// Get or set D value
    /// </summary>
    public byte[] D { get; set; }
    
    /// <summary>
    /// Get or set Q value
    /// </summary>
    public ECPoint Q { get; set; }
}