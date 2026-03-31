// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using OpenIdentityServer.EntityFramework.Mappers;
using OpenIdentityServer.Models;
using Xunit;

namespace OpenIdentityServer.EntityFramework.UnitTests.Mappers;

public class IdentityResourcesMappersTests
{

    [Fact]
    public void CanMapIdentityResources()
    {
        var model = new IdentityResource();
        var mappedEntity = model.ToEntity();
        var mappedModel = mappedEntity.ToModel();

        Assert.NotNull(mappedModel);
        Assert.NotNull(mappedEntity);
    }
}