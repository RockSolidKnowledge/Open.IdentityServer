// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.EntityFramework.Mappers;
using Xunit;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

public class IdentityServerServerSideSessionsExtensionsTests
{
    [Fact]
    public void EntityIdentityServerServerSideSessions_ToModel_ProducesModelWithCorrectFields()
    {
        IdentityServerServerSideSessions entity = new IdentityServerServerSideSessions
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
            Data = "FAKEPROTECTEDDATA"
        };

        Models.IdentityServerServerSideSessions actual = entity.ToModel();

        entity.Should().BeEquivalentTo(actual);
    }
    
    [Fact]
    public void ModelIdentityServerServerSideSessions_ToEntity_ProducesEntityWithCorrectFields()
    {
        Models.IdentityServerServerSideSessions model = new Models.IdentityServerServerSideSessions
        {
            Key = Guid.NewGuid().ToString(),
            Scheme = "FakeScheme",
            SubjectId = "fake-subject",
            SessionId = "fake-session",
            DisplayName = "Fake Session for User",
            Created = new DateTime(2020, 01, 01, 12, 20, 0, DateTimeKind.Utc),
            Renewed = new DateTime(2020, 02, 01, 12, 20, 0, DateTimeKind.Utc),
            Expires = new DateTime(2020, 03, 01, 12, 20, 0, DateTimeKind.Utc),
            Data = "FAKEPROTECTEDDATA"
        };

        IdentityServerServerSideSessions actual = model.ToEntity();
        
        model.Should().BeEquivalentTo(actual, cnf => cnf.Excluding(x => x.Id));
    }
}