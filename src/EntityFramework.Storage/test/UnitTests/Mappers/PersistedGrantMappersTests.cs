// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AwesomeAssertions;
using OpenIdentityServer.EntityFramework.Mappers;
using OpenIdentityServer.Models;
using Xunit;

namespace OpenIdentityServer.EntityFramework.UnitTests.Mappers;

public class PersistedGrantMappersTests
{
    [Fact]
    public void CanMap()
    {
        var model = new PersistedGrant
        {
            ConsumedTime = new System.DateTime(2020, 02, 03, 4, 5, 6)
        };
            
        var mappedEntity = model.ToEntity();
        mappedEntity.Should().NotBeNull();
        mappedEntity.ConsumedTime.Should().NotBeNull();
        mappedEntity.ConsumedTime.Value.Should().Be(new System.DateTime(2020, 02, 03, 4, 5, 6));
            
        var mappedModel = mappedEntity.ToModel();
        mappedModel.Should().NotBeNull();
        mappedModel.ConsumedTime.Should().NotBeNull();
        mappedModel.ConsumedTime.Value.Should().Be(new System.DateTime(2020, 02, 03, 4, 5, 6));
    }
}