// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.UnitTests.Common
{
    internal class StubClock : TimeProvider
    {
        public Func<DateTime> UtcNowFunc = () => DateTime.UtcNow;

        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(UtcNowFunc());
        }
    }
}
