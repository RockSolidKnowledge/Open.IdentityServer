using System;
using System.Text.Json;
using IdentityServer4.Configuration;
using IdentityServer4.Storage.Stores.DataProtection;
using IdentityServer4.Stores.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace IdentityServer4.DataProtection;

/// <summary>
/// Decorator for IPersistentGrantSerializer that protects to serialized persisted grant data 
/// </summary>
/// <seealso cref="IdentityServer4.Stores.Serialization.IPersistentGrantSerializer" />
public class PersistentGrantSerializerDataProtectionDecorator(
    IPersistentGrantSerializer decoratedSerializer,
    IDataProtectionProvider dataProtectionProvider,
    IOptions<IdentityServerOptions> options): IPersistentGrantSerializer
{
    private const string ProtectorPurpose = "PersistedGrant-DataProtection";
    private IDataProtector dataProtector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    private IdentityServerOptions options = options.Value;

    private JsonSerializerOptions serializerOptions = new();

    /// <summary>
    /// Serializes the specified value. And protects the data using Data Protection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public string Serialize<T>(T value)
    {
        string data = decoratedSerializer.Serialize(value);
        
        var wrappedData = new DataProtectedGrantData
        {
            DataProtected = options.PersistentGrants.DataProtectData,
            Payload = options.PersistentGrants.DataProtectData ? dataProtector.Protect(data) : data,
        };

        return JsonSerializer.Serialize(wrappedData, serializerOptions);
    }

    /// <summary>
    /// Deserializes the specified string. And unprotects the data using Data Protection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">The json.</param>
    /// <returns></returns>
    public T Deserialize<T>(string json)
    {
        var wrappedData = JsonSerializer.Deserialize<DataProtectedGrantData>(json, serializerOptions);

        var data = wrappedData.DataProtected ? dataProtector.Unprotect(wrappedData.Payload) : wrappedData.Payload;

        return decoratedSerializer.Deserialize<T>(data);
    }
}