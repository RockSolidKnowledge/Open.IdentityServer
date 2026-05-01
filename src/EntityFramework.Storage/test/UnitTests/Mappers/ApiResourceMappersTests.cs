// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.EntityFramework.Mappers;
using Xunit;
using ApiResource = Open.IdentityServer.Models.ApiResource;
using Secret = Open.IdentityServer.Models.Secret;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

public class ApiResourceMappersTests
{
    [Fact]
    public void Can_Map()
    {
        var model = new ApiResource();
        var mappedEntity = model.ToEntity();
        var mappedModel = mappedEntity.ToModel();

        Assert.NotNull(mappedModel);
        Assert.NotNull(mappedEntity);
    }

    [Fact]
    public void EntitiesApiResourceToModel_ShouldMapEntityToModelObject()
    {
        var entity = new Entities.ApiResource
        {
            Description = "description",
            ShowInDiscoveryDocument = true,
            UserClaims = [
                new ApiResourceClaim { Type = "role" },
                new ApiResourceClaim { Type = "email" },
                
            ],
            Properties = [
                new ApiResourceProperty { Key = "propA", Value = "Property Value", }
            ],
            Secrets = [
                new ApiResourceSecret
                {
                    Value = "FakeSecret",
                },
            ],
            DisplayName = "displayname",
            Name = "foo",
            Scopes =
            [
                new ApiResourceScope { Scope = "foo1" },
                new ApiResourceScope { Scope = "foo2" }
            ],
            AllowedAccessTokenSigningAlgorithms = null,
            Enabled = false,
            RequireResourceIndicator = true,
        };
        
        var mappedModel = entity.ToModel();

        mappedModel.Scopes.Should().BeEquivalentTo("foo1", "foo2");
        mappedModel.UserClaims.Should().BeEquivalentTo("role", "email");
        mappedModel.Properties.Should().ContainKey("propA")
            .WhoseValue.Should().Be("Property Value");
        mappedModel.Description.Should().Be("description");
        mappedModel.DisplayName.Should().Be("displayname");
        mappedModel.Enabled.Should().BeFalse();
        mappedModel.Name.Should().Be("foo");
        mappedModel.RequireResourceIndicator.Should().BeTrue();
    }

    [Fact]
    public void ModelsApiResourceToEntity_ShouldMapModelToEntityObject()
    {
        var model = new ApiResource
        {
            Description = "description",
            ShowInDiscoveryDocument = true,
            UserClaims = ["role", "email"],
            Properties = new Dictionary<string, string>
            {
                ["propA"] = "Property Value",
            },
            ApiSecrets = [
                new Secret("FakSecret"), 
            ],
            DisplayName = "displayname",
            Name = "foo",
            Scopes =
            {
                "foo1",
                "foo2"
            },
            AllowedAccessTokenSigningAlgorithms = null,
            Enabled = false,
            RequireResourceIndicator = true,
        };

        var mappedEntity = model.ToEntity();

        mappedEntity.Scopes.Count.Should().Be(2);
        var foo1 = mappedEntity.Scopes.FirstOrDefault(x => x.Scope == "foo1");
        foo1.Should().NotBeNull();
        var foo2 = mappedEntity.Scopes.FirstOrDefault(x => x.Scope == "foo2");
        foo2.Should().NotBeNull();


        mappedEntity.UserClaims.Should().ContainEquivalentOf(new ApiResourceClaim { Type = "role" });
        mappedEntity.UserClaims.Should().ContainEquivalentOf(new ApiResourceClaim { Type = "email" });

        mappedEntity.Properties.Should().ContainEquivalentOf(new ApiResourceProperty { Key = "propA", Value = "Property Value" });
        
        mappedEntity.Description.Should().Be("description");
        mappedEntity.DisplayName.Should().Be("displayname");
        mappedEntity.Enabled.Should().BeFalse();
        mappedEntity.Name.Should().Be("foo");
        mappedEntity.RequireResourceIndicator.Should().BeTrue();
    }

    [Fact]
    public void EntitiesApiResourceToModel_MissingValues_ShouldUseDefaults()
    {
        var entity = new Open.IdentityServer.EntityFramework.Entities.ApiResource
        {
            Secrets =
            [
                new Entities.ApiResourceSecret(),
            ]
        };

        var def = new ApiResource
        {
            ApiSecrets = { new Models.Secret("foo") }
        };

        var model = entity.ToModel();
        model.ApiSecrets.First().Type.Should().Be(def.ApiSecrets.First().Type);
    }
}