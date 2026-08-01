// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace IdentityServer.IntegrationTests.Utility;

internal static class InternalStringExtensions
{
    [DebuggerStepThrough]
    public static bool IsMissing([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    [DebuggerStepThrough]
    public static bool IsPresent([NotNullWhen(true)] this string? value)
    {
        return !(value.IsMissing());
    }

    [DebuggerStepThrough]
    public static string EnsureTrailingSlash(this string url)
    {
        if (!url.EndsWith("/"))
        {
            return url + "/";
        }

        return url;
    }

    [DebuggerStepThrough]
    public static string RemoveTrailingSlash(this string url)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        
        if (url.EndsWith("/"))
        {
            url = url.Substring(0, url.Length - 1);
        }

        return url;
    }
}
