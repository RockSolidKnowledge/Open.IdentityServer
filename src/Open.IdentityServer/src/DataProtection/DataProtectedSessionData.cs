namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Envelope used by <see cref="Open.IdentityServer.Stores.ServerSessionTicketStore"/>
/// to wrap a serialized server session payload together with the metadata needed
/// to determine the version of the envelope.
/// </summary>
public class DataProtectedSessionData
{
    /// <summary>
    /// Schema version of this envelope.
    /// Incremented when the shape of the payload changes.
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Payload of the grant data, either raw JSON or protected string
    /// </summary>
    public string Payload { get; set; }
}