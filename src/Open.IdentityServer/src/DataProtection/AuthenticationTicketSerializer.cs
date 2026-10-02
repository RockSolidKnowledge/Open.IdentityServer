#nullable enable

using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Serializer class for <see cref="AuthenticationTicket"/>, data is serialized/deserialized and protected/unprotected using data protection
/// </summary>
public class AuthenticationTicketSerializer(
    IDataProtectionProvider dataProtectionProvider,
    ILogger<AuthenticationTicketSerializer> logger): IAuthenticationTicketSerializer
{
    private readonly IDataProtector dataProtector =
        dataProtectionProvider.CreateProtector(DataProtectionConstants.ServerSideTicketStorePurpose);

    /// <summary>
    /// <see cref="JsonSerializerOptions"/> to be used for storing server side sessions
    /// </summary>
    public static readonly JsonSerializerOptions JsonSettings = new()
    {
        IncludeFields = true,
    };

    /// <inheritdoc />
    public string Serialize(AuthenticationTicket ticket)
    {
        string serializedTicket = JsonSerializer.Serialize(ticket.ToSerializableObj());

        return JsonSerializer.Serialize(new DataProtectedSessionData
        {
            Payload = dataProtector.Protect(serializedTicket),
        }, JsonSettings);
    }

    /// <inheritdoc />
    public AuthenticationTicket? Deserialize(string data)
    {
        DataProtectedSessionData? dataProtectedSessionData;

        try
        {
            dataProtectedSessionData = JsonSerializer.Deserialize<DataProtectedSessionData>(data, JsonSettings);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "failed deserialising auth ticket data");
            return null;
        }
        
        if (dataProtectedSessionData is not { Version: 1 })
        {
            logger.LogError("failed deserialising auth ticket data, incorrect version '{VersionOrNull}'", dataProtectedSessionData?.Version);
            return null;
        }
            
        string unprotectedData = dataProtector.Unprotect(dataProtectedSessionData.Payload);

        SerializedAuthenticationTicket? serializedAuthTicket =
            JsonSerializer.Deserialize<SerializedAuthenticationTicket>(unprotectedData);

        return serializedAuthTicket?.ToAuthTicket();
    }
}