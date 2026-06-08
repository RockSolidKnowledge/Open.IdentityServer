using System;
using System.Reflection;

namespace Open.IdentityServer.UnitTests;

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