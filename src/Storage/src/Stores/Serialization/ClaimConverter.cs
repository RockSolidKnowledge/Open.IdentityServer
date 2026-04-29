// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// JSON converter for serializing and deserializing <see cref="Claim"/> instances.
/// </summary>
public class ClaimConverter: JsonConverter<Claim>
{
    /// <summary>
    /// Reads and converts JSON to a <see cref="Claim"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized <see cref="Claim"/>.</returns>
    public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var source = JsonSerializer.Deserialize<ClaimLite>(ref reader, options);
        var target = new Claim(source.Type, source.Value, source.ValueType);
        return target;
    }

    /// <summary>
    /// Writes a <see cref="Claim"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    /// <param name="value">The <see cref="Claim"/> to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, Claim value, JsonSerializerOptions options)
    {
        var source = value;

        var target = new ClaimLite
        {
            Type = source.Type,
            Value = source.Value,
            ValueType = source.ValueType
        };

        JsonSerializer.Serialize(writer, target, options);
    }
}