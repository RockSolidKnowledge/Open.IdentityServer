using System;
using System.Text.Json;
using IdentityServer4.Configuration;
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
    private static readonly JsonSerializerOptions _settings;

    /// <summary>
    /// Serializes the specified value. And protects the data using Data Protection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public string Serialize<T>(T value)
    {
        var serialisedData = decoratedSerializer.Serialize(value);
        
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deserializes the specified string. And unprotects the data using Data Protection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">The json.</param>
    /// <returns></returns>
    public T Deserialize<T>(string json)
    {
        var deserialisedData = decoratedSerializer.Deserialize<T>(json);

        throw new NotImplementedException();
    }
}