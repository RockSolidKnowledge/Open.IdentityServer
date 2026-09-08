// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

public class ScopesMappersTests
{
    [Fact]
    public void CanMapScope()
    {
        var model = new ApiScope();
        var mappedEntity = model.ToEntity();
        var mappedModel = mappedEntity.ToModel();

        Assert.NotNull(mappedModel);
        Assert.NotNull(mappedEntity);
    }

    [Fact]
    public void CanMapToExtendedScopeModel()
    {
        var entity = new Entities.ApiScope
        {
            Name = "foo",
            DisplayName = "foo",
            Description = "bar",
            Created = DateTime.UtcNow.AddDays(-100),
            Updated = DateTime.UtcNow.AddDays(-50),
            LastAccessed = DateTime.UtcNow.AddDays(-10),
            UserClaims = [ new Entities.ApiScopeClaim { Type = "c1" }, new Entities.ApiScopeClaim { Type = "c2" } ],
            Properties = [ new Entities.ApiScopeProperty { Key = "x", Value = "xx" }, new Entities.ApiScopeProperty { Key = "y", Value = "yy" } ]
        };

        var model = entity.ToExtendedModel();

        Assert.NotNull(model);
        model.Created.Should().Be(entity.Created);
        model.Updated.Should().Be(entity.Updated);
        model.LastAccessed.Should().Be(entity.LastAccessed);
        model.X.Should().Be(entity.Properties.Single(p => p.Key == "x").Value);
    }

    [Fact]
    public void Properties_Map()
    {
        var model = new ApiScope()
        {
            Description = "description",
            DisplayName = "displayname",
            Name = "foo",
            UserClaims = { "c1", "c2" },
            Properties = {
                { "x", "xx" },
                { "y", "yy" },
            },
            Enabled = false
        };


        var mappedEntity = model.ToEntity();
        mappedEntity.Description.Should().Be("description");
        mappedEntity.DisplayName.Should().Be("displayname");
        mappedEntity.Name.Should().Be("foo");

        mappedEntity.UserClaims.Count.Should().Be(2);
        mappedEntity.UserClaims.Select(x => x.Type).Should().BeEquivalentTo(new[] { "c1", "c2" });
        mappedEntity.Properties.Count.Should().Be(2);
        mappedEntity.Properties.Should().Contain(x => x.Key == "x" && x.Value == "xx");
        mappedEntity.Properties.Should().Contain(x => x.Key == "y" && x.Value == "yy");


        var mappedModel = mappedEntity.ToModel();

        mappedModel.Description.Should().Be("description");
        mappedModel.DisplayName.Should().Be("displayname");
        mappedModel.Enabled.Should().BeFalse();
        mappedModel.Name.Should().Be("foo");
        mappedModel.UserClaims.Count.Should().Be(2);
        mappedModel.UserClaims.Should().BeEquivalentTo(new[] { "c1", "c2" });
        mappedModel.Properties.Count.Should().Be(2);
        mappedModel.Properties["x"].Should().Be("xx");
        mappedModel.Properties["y"].Should().Be("yy");
    }

    [Fact]
    public void ToEntity_maps_all_properties()
    {
        new MappingVerifier<ApiScope, Entities.ApiScope>()
            .ExcludeDestinationProperties(
                // Database-assigned or entity-managed fields not sourced from the model
                nameof(Entities.ApiScope.Id),
                nameof(Entities.ApiScope.Created),
                nameof(Entities.ApiScope.Updated),
                nameof(Entities.ApiScope.LastAccessed),
                nameof(Entities.ApiScope.NonEditable))
            .Verify(model => model.ToEntity());
    }

    [Fact]
    public void ToModel_maps_all_properties()
    {
        new MappingVerifier<Entities.ApiScope, ApiScope>()
            .Verify(entity => entity.ToModel());
    }
}

internal static class ExtendedScopeMappingExtensions
{
    extension(Entities.ApiScope apiScopeEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.ApiScope"/> to convert into an instance of <see cref="ExtendedApiScope"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="ExtendedApiScope"/></returns>
        public ExtendedApiScope ToExtendedModel()
        {
            var model = apiScopeEntity.ToModel<ExtendedApiScope>();
            model.Created = apiScopeEntity.Created;
            model.Updated = apiScopeEntity.Updated;
            model.LastAccessed = apiScopeEntity.LastAccessed;

            return model;
        }

    }
}

internal class ExtendedApiScope : ApiScope
{
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? LastAccessed { get; set; }

    public string? X
    {
        get => Properties.ContainsKey("x") ? Properties["x"] : null;
        set => Properties["x"] = value;
    }
}