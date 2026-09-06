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

    [Fact]
    public void CanMapToExtendedClientModel()
    {
        var entity = new Entities.Client
        {
            ClientName = "foo",
            Description = "bar",
            Created = DateTime.UtcNow.AddDays(-100),
            Updated = DateTime.UtcNow.AddDays(-50),
            LastAccessed = DateTime.UtcNow.AddDays(-25),
            Properties = [new Entities.ClientProperty { Key = "x", Value = "xx" }, new Entities.ClientProperty { Key = "y", Value = "yy" }]
        };

        var model = entity.ToExtendedModel();

        Assert.NotNull(model);
        model.Created.Should().Be(entity.Created);
        model.Updated.Should().Be(entity.Updated);
        model.LastAccessed.Should().Be(entity.LastAccessed);
        model.X.Should().Be(entity.Properties.Single(p => p.Key == "x").Value);
    }

    [Fact]
    public void ToEntity_maps_all_properties()
    {
        new MappingVerifier<Client, Entities.Client>()
            .ExcludeDestinationProperties(
                // Database-assigned or entity-managed fields not sourced from the model
                nameof(Entities.Client.Id),
                nameof(Entities.Client.Created),
                nameof(Entities.Client.Updated),
                nameof(Entities.Client.LastAccessed),
                nameof(Entities.Client.NonEditable),
                // Compatibility properties intentionally not mapped
                nameof(Entities.Client.CibaLifetime),
                nameof(Entities.Client.PollingInterval),
                nameof(Entities.Client.CoordinateLifetimeWithUserSession),
                nameof(Entities.Client.InitiateLoginUri),
                nameof(Entities.Client.DPoPClockSkew),
                nameof(Entities.Client.DPoPValidationMode),
                nameof(Entities.Client.RequireDPoP),
                nameof(Entities.Client.PushedAuthorizationLifetime),
                nameof(Entities.Client.RequirePushedAuthorization))
            .Verify(model => model.ToEntity());
    }

    [Fact]
    public void ToModel_maps_all_properties()
    {
        new MappingVerifier<Entities.Client, Client>()
            .ExcludeDestinationProperties(
                // Compatibility properties intentionally not mapped
                nameof(Client.CibaLifetime),
                nameof(Client.PollingInterval),
                nameof(Client.CoordinateLifetimeWithUserSession),
                nameof(Client.InitiateLoginUri),
                nameof(Client.DPoPClockSkew),
                nameof(Client.DPoPValidationMode),
                nameof(Client.RequireDPoP),
                nameof(Client.PushedAuthorizationLifetime),
                nameof(Client.RequirePushedAuthorization))
            .Verify(entity => entity.ToModel());
    }
}

internal static class ExtendedClientMappingExtensions
{
    extension(Entities.Client clientEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.Client"/> to convert into an instance of <see cref="ExtendedClient"/>

        /// </summary>
        /// <returns>mapped instance of <see cref="ExtendedClient"/></returns>
        public ExtendedClient ToExtendedModel()
        {
            var model = clientEntity.ToModel<ExtendedClient>();
            model.Created = clientEntity.Created;
            model.Updated = clientEntity.Updated;
            model.LastAccessed = clientEntity.LastAccessed;

            return model;
        }

    }
}

internal class ExtendedClient : Client
{
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? LastAccessed { get; internal set; }

    public string? X
    {
        get => Properties.ContainsKey("x") ? Properties["x"] : null;
        set => Properties["x"] = value;
    }
}