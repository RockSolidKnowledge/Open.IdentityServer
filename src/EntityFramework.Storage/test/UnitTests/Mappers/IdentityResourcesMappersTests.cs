// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

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

    [Fact]
    public void ToEntity_maps_all_properties()
    {
        new MappingVerifier<IdentityResource, Entities.IdentityResource>()
            .ExcludeDestinationProperties(
                // Database-assigned or entity-managed fields not sourced from the model
                nameof(Entities.IdentityResource.Id),
                nameof(Entities.IdentityResource.Created),
                nameof(Entities.IdentityResource.Updated),
                nameof(Entities.IdentityResource.NonEditable))
            .Verify(model => model.ToEntity());
    }

    [Fact]
    public void ToModel_maps_all_properties()
    {
        new MappingVerifier<Entities.IdentityResource, IdentityResource>()
            .Verify(entity => entity.ToModel());
    }
}