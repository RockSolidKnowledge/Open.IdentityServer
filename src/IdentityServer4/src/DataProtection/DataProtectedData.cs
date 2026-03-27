namespace IdentityServer4.Storage.Stores.DataProtection;

public class DataProtectedData
{
    public int Version { get; set; }
    public bool Protected { get; set; }
    public string Payload { get; set; }
}