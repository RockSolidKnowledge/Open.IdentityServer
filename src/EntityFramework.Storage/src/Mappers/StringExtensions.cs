// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Open.IdentityServer.EntityFramework.Mappers;

internal static class StringExtensions
{
    extension(ICollection<string>? stringCollection) {
        internal string ToSeparatedString(char separator = ',')
        {
            if (stringCollection == null || stringCollection.Count == 0)
            {
                return string.Empty;
            }
            return stringCollection.Aggregate((x, y) => $"{x}{separator}{y}");
        }
    }
    
    extension(string? srcString) {
        internal ICollection<string> ToCollectionUsingSepator(char separator = ',')
        {
            var list = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(srcString))
            {
                srcString = srcString.Trim();
                foreach (var item in srcString.Split([separator], StringSplitOptions.RemoveEmptyEntries).Distinct())
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}