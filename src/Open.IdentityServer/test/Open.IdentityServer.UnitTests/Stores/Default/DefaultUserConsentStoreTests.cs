// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Stores.Serialization;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.Default;

public class DefaultUserConsentStoreTests
{
    private readonly IPersistedGrantStore store = Mock.Of<IPersistedGrantStore>();
    private readonly IPersistentGrantSerializer serializer = new PersistentGrantSerializer();
    private readonly IHandleGenerationService handleGenerationService = Mock.Of<IHandleGenerationService>();
    private readonly ILogger<DefaultUserConsentStore> logger = NullLogger<DefaultUserConsentStore>.Instance;
    
    private DefaultUserConsentStore CreateSut()
    {
        return new DefaultUserConsentStore(store, serializer, handleGenerationService, logger);
    }

    [Fact]
    public async Task GetUserConsentAsync_WhenHexEncodedKeyReturnsValue_ShouldReturnConsent()
    {
        var fakeConsent = new Consent
        {
            SubjectId = Guid.NewGuid().ToString(),
            ClientId = "fake.client",
        };

        var grant = new PersistedGrant
        {
            SubjectId = fakeConsent.SubjectId,
            ClientId = fakeConsent.ClientId,
            Data = JsonSerializer.Serialize(fakeConsent),
            Type = IdentityServerConstants.PersistedGrantTypes.UserConsent,
        };
        
        Mock.Get(store)
            .Setup(x => x.GetAsync(GetHexHashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        var actual = await sut.GetUserConsentAsync(fakeConsent.SubjectId, fakeConsent.ClientId);

        actual.Should().BeEquivalentTo(fakeConsent);
    }

    [Fact]
    public async Task GetUserConsentAsync_WhenHexEncodedKeyReturnsNull_ShouldTryAgainWithBase64()
    {
        var fakeConsent = new Consent
        {
            SubjectId = Guid.NewGuid().ToString(),
            ClientId = "fake.client",
        };

        var grant = new PersistedGrant
        {
            SubjectId = fakeConsent.SubjectId,
            ClientId = fakeConsent.ClientId,
            Data = JsonSerializer.Serialize(fakeConsent),
            Type = IdentityServerConstants.PersistedGrantTypes.UserConsent,
        };

        Mock.Get(store)
            .Setup(x => x.GetAsync(GetHexHashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync((PersistedGrant) null);

        Mock.Get(store)
            .Setup(x => x.GetAsync(GetBase64HashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        var actual = await sut.GetUserConsentAsync(fakeConsent.SubjectId, fakeConsent.ClientId);

        actual.Should().BeEquivalentTo(fakeConsent);
    }

    [Fact]
    public async Task GetUserConsentAsync_WhenHexEncodedKeyAndBase64ReturnsNull_ShouldReturnNull()
    {
        var fakeConsent = new Consent
        {
            SubjectId = Guid.NewGuid().ToString(),
            ClientId = "fake.client",
        };

        Mock.Get(store)
            .Setup(x => x.GetAsync(GetHexHashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync((PersistedGrant) null);

        Mock.Get(store)
            .Setup(x => x.GetAsync(GetBase64HashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync((PersistedGrant) null);

        var sut = CreateSut();

        var actual = await sut.GetUserConsentAsync(fakeConsent.SubjectId, fakeConsent.ClientId);

        actual.Should().BeNull();
    }

    private string GetHexHashedKey(string clientId, string subjectId) =>
        $"{clientId}|{subjectId}{DefaultUserConsentStore.HexEncodingSuffix}:{IdentityServerConstants.PersistedGrantTypes.UserConsent}"
            .Sha256(true);

    private string GetBase64HashedKey(string clientId, string subjectId) =>
        $"{clientId}|{subjectId}:{IdentityServerConstants.PersistedGrantTypes.UserConsent}"
            .Sha256();
}