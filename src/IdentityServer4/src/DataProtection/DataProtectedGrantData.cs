namespace IdentityServer4.Storage.Stores.DataProtection;

public class DataProtectedGrantData
{
    public int PersistentGrantDataContainerVersion { get; set; } = 1;
    public bool DataProtected { get; set; }
    public string Payload { get; set; }
}