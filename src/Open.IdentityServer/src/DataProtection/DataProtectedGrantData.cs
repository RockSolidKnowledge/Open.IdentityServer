namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Envelope used by <see cref="Open.IdentityServer.DataProtection.PersistentGrantSerializerDataProtectionDecorator"/>
/// to wrap a serialized persisted-grant payload together with the metadata needed
/// to determine whether the payload has been protected via ASP.NET Core Data Protection.
/// </summary>
public class DataProtectedGrantData
{
    /// <summary>
    /// Schema version of this envelope.
    /// Incremented when the shape of the payload changes.
    /// </summary>
    public int PersistentGrantDataContainerVersion { get; set; } = 1;
    
    /// <summary>
    /// <see langword="true"/> when <see cref="Payload"/> has been protected via
    /// <c>IDataProtector.Protect</c> and must be unprotected before deserialization;
    /// <see langword="false"/> when the payload is the raw serialized grant.
    /// </summary>
    public bool DataProtected { get; set; }
    
    /// <summary>
    /// <see langword="true"/> when <see cref="Payload"/> has been protected via
    /// <c>IDataProtector.Protect</c> and must be unprotected before deserialization;
    /// <see langword="false"/> when the payload is the raw serialized grant.
    /// </summary>
    public string Payload { get; set; }
}