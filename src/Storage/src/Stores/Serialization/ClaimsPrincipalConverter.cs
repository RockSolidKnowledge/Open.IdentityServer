// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Open.IdentityModel;


namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// JSON converter for serializing and deserializing <see cref="ClaimsPrincipal"/> instances.
/// </summary>
public class ClaimsPrincipalConverter: JsonConverter<ClaimsPrincipal>
{
    /// <summary>
    /// Reads and converts JSON to a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized <see cref="ClaimsPrincipal"/>, or <c>null</c> if the JSON is null.</returns>
    public override ClaimsPrincipal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var source = JsonSerializer.Deserialize<ClaimsPrincipalLite>(ref reader, options);
        if (source == null) return null;

        var claims = source.Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType));
        var id = new ClaimsIdentity(claims, source.AuthenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
        var target = new ClaimsPrincipal(id);
        return target;
    }

    /// <summary>
    /// Writes a <see cref="ClaimsPrincipal"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    /// <param name="value">The <see cref="ClaimsPrincipal"/> to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, ClaimsPrincipal value, JsonSerializerOptions options)
    {
        var source = value;

        var target = new ClaimsPrincipalLite
        {
            AuthenticationType = source.Identity.AuthenticationType,
            Claims = source.Claims.Select(x => new ClaimLite { Type = x.Type, Value = x.Value, ValueType = x.ValueType }).ToArray()
        };
        JsonSerializer.Serialize(writer, target, options);
    }
}