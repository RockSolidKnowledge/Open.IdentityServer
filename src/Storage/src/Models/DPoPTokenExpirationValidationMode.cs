namespace Open.IdentityServer.Storage.Models;

/// <summary>
/// 
/// </summary>
public enum DPoPTokenExpirationValidationMode
{
    /// <summary>
    /// 
    /// </summary>
    Custom = 0,

    /// <summary>
    /// 
    /// </summary>
    Iat = 1,

    /// <summary>
    /// 
    /// </summary>
    Nonce = 2,

    /// <summary>
    /// 
    /// </summary>
    IatAndNonce = Iat + Nonce,
}