// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Reflection;

namespace Open.IdentityServer.EntityFramework.IntegrationTests;

public class LocalTimeZoneInfoMocker: IDisposable
{
    public LocalTimeZoneInfoMocker(TimeZoneInfo mockTimeZoneInfo)
    {
        var info = typeof(TimeZoneInfo).GetField("s_cachedData", BindingFlags.NonPublic | BindingFlags.Static);
        var cachedData = info?.GetValue(null);
        var field = cachedData?.GetType().GetField("_localTimeZone",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Instance);
        field?.SetValue(cachedData, mockTimeZoneInfo);
    }

    public void Dispose()
    {
        TimeZoneInfo.ClearCachedData();
    }
}