namespace Open.IdentityServer.Configuration;

/// <summary>
/// Options for configuring persisted grant behavior
/// </summary>
public class PersistentGrantsOptions
{
    /// <summary>
    /// Specifies if data field of persisted grants should be protected using ASP.NET Core DataProtection
    /// </summary>
    public bool DataProtectData { get; set; }
}