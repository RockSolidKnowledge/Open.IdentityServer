// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Open.IdentityServer.Models;

/// <summary>
/// Extension methods for hashing strings
/// </summary>
public static class HashExtensions
{
    /// <summary>
    /// Creates a SHA256 hash of the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="hexEncode">Specifies if it should hex encode the hash, defaults to false and uses Base64</param>
    /// <returns>A hash</returns>
    public static string Sha256(this string input, bool hexEncode = false)
    {
        if (input.IsMissing()) return string.Empty;
            
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);

        if (hexEncode)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }

        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Creates a SHA256 hash of the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A hash.</returns>
    public static byte[] Sha256(this byte[] input)
    {
        if (input == null)
        {
            return null;
        }
            
        return SHA256.HashData(input);
    }

    /// <summary>
    /// Creates a SHA512 hash of the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A hash</returns>
    public static string Sha512(this string input)
    {
        if (input.IsMissing()) return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA512.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}