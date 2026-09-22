// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Models;
using System;
using System.Linq;
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
    public void CanMapToExtendedIdentityResourceModel()
    {
        var entity = new Entities.IdentityResource
        {
            Name = "foo",
            DisplayName = "foo",
            Description = "bar",
            Created = DateTime.UtcNow.AddDays(-100),
            Updated = DateTime.UtcNow.AddDays(-50),
            Properties = [new Entities.IdentityResourceProperty { Key = "x", Value = "xx" }, new Entities.IdentityResourceProperty { Key = "y", Value = "yy" }]
        };

        var model = entity.ToExtendedModel();

        Assert.NotNull(model);
        model.Created.Should().Be(entity.Created);
        model.Updated.Should().Be(entity.Updated);
        model.X.Should().Be(entity.Properties.Single(p => p.Key == "x").Value);
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

internal static class ExtendedIdentityResourceMappingExtensions
{
    extension(Entities.IdentityResource identityResourceEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.IdentityResource"/> to convert into an instance of <see cref="ExtendedIdentityResource"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="ExtendedIdentityResource"/></returns>
        public ExtendedIdentityResource ToExtendedModel()
        {
            var model = identityResourceEntity.ToModel<ExtendedIdentityResource>();
            model.Created = identityResourceEntity.Created;
            model.Updated = identityResourceEntity.Updated;

            return model;
        }

    }
}

internal class ExtendedIdentityResource : IdentityResource
{
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }

    public string? X
    {
        get => Properties.ContainsKey("x") ? Properties["x"] : null;
        set => Properties["x"] = value;
    }
}