// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Text.Json;
using Open.IdentityServer.Stores.Serialization;
using Microsoft.AspNetCore.DataProtection;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Decorator for IPersistentGrantSerializer that protects to serialized persisted grant data 
/// </summary>
/// <seealso cref="Open.IdentityServer.Stores.Serialization.IPersistentGrantSerializer" />
/// <param name="decoratedSerializer">The inner serializer used to perform the actual grant serialization and deserialization.</param>
/// <param name="dataProtectionProvider">The data protection provider used to create the protector for encrypting and decrypting grant payloads.</param>
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
    /// <returns>A JSON string containing the data-protected grant payload.</returns>
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
    /// <returns>The deserialized grant of type <typeparamref name="TGrant"/>, or <see langword="null"/> when the JSON cannot be deserialized.</returns>
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