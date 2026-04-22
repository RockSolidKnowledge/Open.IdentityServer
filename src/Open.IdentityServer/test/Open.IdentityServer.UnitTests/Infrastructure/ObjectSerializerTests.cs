// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using AwesomeAssertions;
using Open.IdentityServer.Models;
using Xunit;

namespace IdentityServer.UnitTests.Infrastructure;

public class ObjectSerializerTests
{
    public ObjectSerializerTests()
    {
    }

    [Fact]
    public void Can_be_deserialize_message()
    {
        Action a = () => Open.IdentityServer.ObjectSerializer.FromString<Message<ErrorMessage>>("{\"created\":0, \"data\": {\"error\": \"error\"}}");
        a.Should().NotThrow();
    }
}