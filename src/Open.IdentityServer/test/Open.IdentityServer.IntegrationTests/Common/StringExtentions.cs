// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.IntegrationTests.Common;

public static class StringExtensions
{
    public static List<KeyValuePair<string, string>> ParseFragment(this string fragment)
    {
        return fragment.TrimStart('#')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .Select(p => new KeyValuePair<string, string>(
                Uri.UnescapeDataString(p[0]),
                p.Length > 1 ? Uri.UnescapeDataString(p[1]) : string.Empty))
            .ToList();
    }
}