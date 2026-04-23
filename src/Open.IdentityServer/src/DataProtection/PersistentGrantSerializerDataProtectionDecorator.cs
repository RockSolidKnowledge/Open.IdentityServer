#nullable enable

using System;
using System.Text.Json;
using Open.IdentityServer.Storage.Stores.DataProtection;
using Open.IdentityServer.Stores.Serialization;
using Microsoft.AspNetCore.DataProtection;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Decorator for IPersistentGrantSerializer that protects to serialized persisted grant data 
/// </summary>
/// <seealso cref="IdentityServer4.Stores.Serialization.IPersistentGrantSerializer" />
public class PersistentGrantSerializerDataProtectionDecorator(
    IPersistentGrantSerializer decoratedSerializer,
    IDataProtectionProvider dataProtectionProvider): IPersistentGrantSerializer
{
    private IDataProtector dataProtector = dataProtectionProvider.CreateProtector(nameof(PersistentGrantSerializer));

    private JsonSerializerOptions serializerOptions = new()
    {
        Converters =
        {
            new DataProtectedGrantDataConverter(),
        }
    };

    /// <summary>
    /// Serializes the specified value. And protects the data using Data Protection
    /// </summary>
    /// <typeparam name="TGrant">type of grant to be serialized</typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public string Serialize<TGrant>(TGrant value)
    {
        string data = decoratedSerializer.Serialize(value);
        
        var wrappedData = new DataProtectedGrantData
        {
            DataProtected = true,
            Payload = dataProtector.Protect(data),
        };

        return JsonSerializer.Serialize(wrappedData, serializerOptions);
    }

    /// <summary>
    /// Deserializes the specified string. And unprotects the data using Data Protection
    /// </summary>
    /// <typeparam name="TGrant">type of grant to be deserialized</typeparam>
    /// <param name="json">The json.</param>
    /// <returns></returns>
    public TGrant? Deserialize<TGrant>(string json)
    {
        DataProtectedGrantData? wrappedData = JsonSerializer.Deserialize<DataProtectedGrantData>(json, serializerOptions);

        if (wrappedData == null)
        {
            return decoratedSerializer.Deserialize<TGrant>(json);
        }
        
        var data = wrappedData.Payload;

        if (wrappedData.DataProtected)
        {
            try
            {
                data = dataProtector.Unprotect(data);
            }
            catch (Exception ex)
            {
                throw new DataProtectionException(ex, "Failed to unprotect grant data, this could be because data protection is incorrectly configured");
            }
        }

        return decoratedSerializer.Deserialize<TGrant>(data);
    }
}