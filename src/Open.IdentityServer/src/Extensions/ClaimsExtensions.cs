// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.Extensions;

internal static class ClaimsExtensions
{
    public static Dictionary<string, object> ToClaimsDictionary(this IEnumerable<Claim> claims)
    {
        var d = new Dictionary<string, object>();

        if (claims == null)
        {
            return d;
        }

        var distinctClaims = claims.Distinct(new ClaimComparer());

        foreach (var claim in distinctClaims)
        {
            if (!d.ContainsKey(claim.Type))
            {
                d.Add(claim.Type, GetValue(claim));
            }
            else
            {
                var value = d[claim.Type];

                if (value is List<object> list)
                {
                    list.Add(GetValue(claim));
                }
                else
                {
                    d.Remove(claim.Type);
                    d.Add(claim.Type, new List<object> { value, GetValue(claim) });
                }
            }
        }

        return d;
    }

    private static object GetValue(Claim claim)
    {
        if (claim.ValueType == ClaimValueTypes.Integer ||
            claim.ValueType == ClaimValueTypes.Integer32)
        {
            if (Int32.TryParse(claim.Value, out int value))
            {
                return value;
            }
        }

        if (claim.ValueType == ClaimValueTypes.Integer64)
        {
            if (Int64.TryParse(claim.Value, out long value))
            {
                return value;
            }
        }

        if (claim.ValueType == ClaimValueTypes.Boolean)
        {
            if (bool.TryParse(claim.Value, out bool value))
            {
                return value;
            }
        }

        if (claim.ValueType == IdentityServerConstants.ClaimValueTypes.Json)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(claim.Value);
            }
            catch { }
        }

        return claim.Value;
    }
    
    public static ClaimLite[] ToSerializableObj(this IEnumerable<Claim> claims)
    {
        return claims.Select(x => new ClaimLite
        {
            Type = x.Type, Value = x.Value, ValueType = x.ValueType, Issuer = x.Issuer,
        }).ToArray();
    }
    
    public static Claim[] ToClaims(this ClaimLite[] claims)
    {
        return claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer)).ToArray();
    }
}