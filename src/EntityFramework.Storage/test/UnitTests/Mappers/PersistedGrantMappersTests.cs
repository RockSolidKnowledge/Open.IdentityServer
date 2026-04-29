// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

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