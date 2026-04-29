// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.Mappers;
using Xunit;
using Client = Open.IdentityServer.Models.Client;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

public class ClientMappersTests
{
    [Fact]
    public void Can_Map()
    {
        var model = new Client();
        var mappedEntity = model.ToEntity();
        var mappedModel = mappedEntity.ToModel();

        Assert.NotNull(mappedModel);
        Assert.NotNull(mappedEntity);
    }

    [Fact]
    public void Properties_Map()
    {
        var model = new Client()
        {
            Properties =
            {
                {"foo1", "bar1"},
                {"foo2", "bar2"},
            }
        };


        var mappedEntity = model.ToEntity();

        mappedEntity.Properties.Count.Should().Be(2);
        var foo1 = mappedEntity.Properties.FirstOrDefault(x => x.Key == "foo1");
        foo1.Should().NotBeNull();
        foo1.Value.Should().Be("bar1");
        var foo2 = mappedEntity.Properties.FirstOrDefault(x => x.Key == "foo2");
        foo2.Should().NotBeNull();
        foo2.Value.Should().Be("bar2");



        var mappedModel = mappedEntity.ToModel();

        mappedModel.Properties.Count.Should().Be(2);
        mappedModel.Properties.ContainsKey("foo1").Should().BeTrue();
        mappedModel.Properties.ContainsKey("foo2").Should().BeTrue();
        mappedModel.Properties["foo1"].Should().Be("bar1");
        mappedModel.Properties["foo2"].Should().Be("bar2");
    }

    [Fact]
    public void duplicates_properties_in_db_map()
    {
        var entity = new Open.IdentityServer.EntityFramework.Entities.Client
        {
            Properties = new System.Collections.Generic.List<Entities.ClientProperty>()
            {
                new Entities.ClientProperty{Key = "foo1", Value = "bar1"},
                new Entities.ClientProperty{Key = "foo1", Value = "bar2"},
            }
        };

        Action modelAction = () => entity.ToModel();
        modelAction.Should().Throw<Exception>();
    }

    [Fact]
    public void missing_values_should_use_defaults()
    {
        var entity = new Open.IdentityServer.EntityFramework.Entities.Client
        {
            ClientSecrets = new System.Collections.Generic.List<Entities.ClientSecret>
            {
                new Entities.ClientSecret
                {
                }
            }
        };

        var def = new Client
        {
            ClientSecrets = { new Models.Secret("foo") }
        };

        var model = entity.ToModel();
        model.ProtocolType.Should().Be(def.ProtocolType);
        model.ClientSecrets.First().Type.Should().Be(def.ClientSecrets.First().Type);
    }
}