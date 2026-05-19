// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;


namespace Open.IdentityServer.Extensions;

/// <summary>
/// Extension methods for converting string collections to <see cref="NameValueCollection"/>.
/// </summary>
public static class IReadableStringCollectionExtensions
{
    /// <summary>
    /// Converts an enumerable of key/value pairs with <see cref="StringValues"/> to a <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="collection">The collection of key/value pairs to convert.</param>
    /// <returns>A <see cref="NameValueCollection"/> containing all key/value pairs from the source collection.</returns>
    [DebuggerStepThrough]
    public static NameValueCollection AsNameValueCollection(this IEnumerable<KeyValuePair<string, StringValues>> collection)
    {
        var nv = new NameValueCollection();

        foreach (var field in collection)
        {
            nv.AddStringValues(field);
        }

        return nv;
    }

    /// <summary>
    /// Converts a dictionary with <see cref="StringValues"/> to a <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="collection">The dictionary to convert.</param>
    /// <returns>A <see cref="NameValueCollection"/> containing all key/value pairs from the source dictionary.</returns>
    [DebuggerStepThrough]
    public static NameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
    {
        var nv = new NameValueCollection();

        foreach (var field in collection)
        {
            nv.AddStringValues(field);
        }

        return nv;
    }

    private static void AddStringValues(this NameValueCollection collection,  KeyValuePair<string, StringValues> field)
    {
        foreach (var value in field.Value)
        {
            collection.Add(field.Key, value);
        }
    }
}