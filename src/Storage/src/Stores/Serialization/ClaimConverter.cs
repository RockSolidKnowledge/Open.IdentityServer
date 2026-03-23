// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable 1591

namespace IdentityServer4.Stores.Serialization
{
    public class ClaimConverter: JsonConverter<Claim>
    {
        public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var source = JsonSerializer.Deserialize<ClaimLite>(ref reader, options);
            var target = new Claim(source.Type, source.Value, source.ValueType);
            return target;
        }

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
}