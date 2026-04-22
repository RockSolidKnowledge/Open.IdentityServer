using Microsoft.AspNetCore.Authentication;
using System;

namespace IdentityServer.UnitTests.Common;

class MockSystemClock : TimeProvider
{
    public DateTimeOffset Now { get; set; }

    public override DateTimeOffset GetUtcNow()
    {
        return Now;
    }
}