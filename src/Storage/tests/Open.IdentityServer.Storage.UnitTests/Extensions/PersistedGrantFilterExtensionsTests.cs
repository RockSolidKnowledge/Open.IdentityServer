// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using AwesomeAssertions;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.Storage.UnitTests.Extensions;

public class PersistedGrantFilterExtensionsTests
{
    [Fact]
    public void Validate_WhenEmpty_ShouldThrowArgumentException()
    {
        PersistedGrantFilter filter = new PersistedGrantFilter();

        Action act = filter.Validate;

        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Validate_WhenClientIdsContainsOnlyInvalidValues_ShouldThrowArgumentException()
    {
        PersistedGrantFilter filter = new PersistedGrantFilter
        {
            ClientIds = [null, "", " "],
        };

        Action act = filter.Validate;

        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Validate_WhenTypesContainsOnlyInvalidValues_ShouldThrowArgumentException()
    {
        PersistedGrantFilter filter = new PersistedGrantFilter
        {
            Types = [null, "", " "],
        };

        Action act = filter.Validate;

        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Validate_WhenSubjectProvidedOnly_ShouldReturn()
    {
        PersistedGrantFilter filter = new PersistedGrantFilter
        {
            SubjectId = "subject",
        };

        Action act = filter.Validate;

        act.Should().NotThrow();
    }
    
    [Fact]
    public void Validate_WhenSessionProvidedOnly_ShouldReturn()
    {
        PersistedGrantFilter filter = new PersistedGrantFilter
        {
            SessionId = "subject",
        };

        Action act = filter.Validate;

        act.Should().NotThrow();
    }
    
    [Fact]
    public void Validate_WhenTypesProvidedOnly_ShouldReturn()
    {
        PersistedGrantFilter filter = new PersistedGrantFilter
        {
            Types = ["type-a", "type-b"],
        };

        Action act = filter.Validate;

        act.Should().NotThrow();
    }
    
    [Fact]
    public void Validate_WhenClientsProvidedOnly_ShouldReturn()
    {
        PersistedGrantFilter filter = new PersistedGrantFilter
        {
            ClientIds = ["cli-a", "cli-b"],
        };

        Action act = filter.Validate;

        act.Should().NotThrow();
    }
}