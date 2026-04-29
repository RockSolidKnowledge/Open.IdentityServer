// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Custom JSON converter for <see cref="DataProtectedGrantData"/> that performs case-insensitive property matching
/// and returns null when the version property is not present in the JSON.
/// </summary>
public class DataProtectedGrantDataConverter: JsonConverter<DataProtectedGrantData>
{
    /// <summary>
    /// Reads and converts JSON to a <see cref="DataProtectedGrantData"/> instance.
    /// Returns null if the <see cref="DataProtectedGrantData.PersistentGrantDataContainerVersion"/> property is not present.
    /// </summary>
    /// <param name="reader">The reader to read JSON from.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A <see cref="DataProtectedGrantData"/> instance, or null if the version property is missing.</returns>
    public override DataProtectedGrantData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int? version = null;
        bool? dataProtected = null;
        string? payload = null;
        
        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            
            var property = reader.GetString();
            reader.Read();
                
            if (property == null) continue;

            if (property.Equals(nameof(DataProtectedGrantData.PersistentGrantDataContainerVersion), StringComparison.OrdinalIgnoreCase))
            {
                version = reader.GetInt32();
            }
            if (property.Equals(nameof(DataProtectedGrantData.DataProtected), StringComparison.OrdinalIgnoreCase))
            {
                dataProtected = reader.GetBoolean();
            }
            if (property.Equals(nameof(DataProtectedGrantData.Payload), StringComparison.OrdinalIgnoreCase))
            {
                payload = reader.GetString();
            }
        }
        
        if (!version.HasValue)
        {
            return null;
        }

        return new DataProtectedGrantData
        {
            PersistentGrantDataContainerVersion = version.Value,
            DataProtected = dataProtected ?? false,
            Payload = payload ?? string.Empty,
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DataProtectedGrantData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}