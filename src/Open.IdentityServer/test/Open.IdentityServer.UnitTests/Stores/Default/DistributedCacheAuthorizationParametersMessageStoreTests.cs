// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.UnitTests.Common;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores.Default;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.Default;

public class DistributedCacheAuthorizationParametersMessageStoreTests
{
    private MockDistributedCache _mockCache = new();

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntryFromCache()
    {
        var subject = new DistributedCacheAuthorizationParametersMessageStore(_mockCache, new DefaultHandleGenerationService());

        _mockCache.Entries.Clear();

        var id = await subject.WriteAsync(new Message<IDictionary<string, string[]>>(new Dictionary<string, string[]>()));
        _mockCache.Entries.Count.Should().Be(1);

        await subject.DeleteAsync(id);
        _mockCache.Entries.Count.Should().Be(0);
    }
}
