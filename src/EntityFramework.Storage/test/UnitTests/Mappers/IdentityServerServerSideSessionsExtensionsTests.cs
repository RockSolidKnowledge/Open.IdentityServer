// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Moq;
using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Stores.Serialization;
using Open.IdentityServer.Test.Utilities;
using Xunit;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

public class IdentityServerServerSideSessionsExtensionsTests
{
    private readonly IAuthenticationTicketSerializer authTicketSerializer = Mock.Of<IAuthenticationTicketSerializer>();
    
    [Fact]
    public void EntityIdentityServerServerSideSessions_ToModel_ProducesModelWithCorrectFields()
    {
        ServerSideSession entity = new ServerSideSession
        {
            Id = 1,
            Key = Guid.NewGuid().ToString(),
            Scheme = "FakeScheme",
            SubjectId = "fake-subject",
            SessionId = "fake-session",
            DisplayName = "Fake Session for User",
            Created = new DateTime(2020, 01, 01, 12, 20, 0, DateTimeKind.Utc),
            Renewed = new DateTime(2020, 02, 01, 12, 20, 0, DateTimeKind.Utc),
            Expires = new DateTime(2020, 03, 01, 12, 20, 0, DateTimeKind.Utc),
            Data = "{\"version\":1,\"payload\":\"fake-payload-FakeScheme\"}",
        };
        AuthenticationTicket authTicket = AuthenticationTicketGenerator.GenerateAuthenticationTicket(entity.Scheme, entity.SubjectId, entity.SessionId, entity.DisplayName, entity.Renewed, entity.Expires);

        Mock.Get(authTicketSerializer)
            .Setup(x => x.Deserialize(entity.Data))
            .Returns(authTicket);
        
        Models.ServerSideSession actual = entity.ToModel(authTicketSerializer);

        actual.Should().BeEquivalentTo(entity, cnf => cnf.Excluding(x => x.Id));
    }
    
    [Fact]
    public void ModelIdentityServerServerSideSessions_ToEntity_ProducesEntityWithCorrectFields()
    {
        Models.ServerSideSession model = new Models.ServerSideSession
        {
            Key = Guid.NewGuid().ToString(),
            Scheme = "FakeScheme",
            SubjectId = "fake-subject",
            SessionId = "fake-session",
            DisplayName = "Fake Session for User",
            Created = new DateTime(2020, 01, 01, 12, 20, 0, DateTimeKind.Utc),
            Renewed = new DateTime(2020, 02, 01, 12, 20, 0, DateTimeKind.Utc),
            Expires = new DateTime(2020, 03, 01, 12, 20, 0, DateTimeKind.Utc),
        };
        AuthenticationTicket authTicket = AuthenticationTicketGenerator.GenerateAuthenticationTicket(model.Scheme, model.SubjectId, model.SessionId, model.DisplayName, model.Renewed, model.Expires);
        model.AuthenticationTicket = authTicket;

        const string fakePayload = "{\"version\":1,\"payload\":\"fake-payload-FakeScheme\"}";
        Mock.Get(authTicketSerializer)
            .Setup(x => x.Serialize(model.AuthenticationTicket))
            .Returns(fakePayload);

        ServerSideSession actual = model.ToEntity(authTicketSerializer);
        
        actual.Should().BeEquivalentTo(model, cnf => cnf
            .Excluding(x => x.AuthenticationTicket)
            .Excluding(x => x.Data));
        actual.Data.Should().BeEquivalentTo(fakePayload);
    }
}