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
    private readonly IPersistedGrantStore _store = Mock.Of<IPersistedGrantStore>();
    private readonly IPersistentGrantSerializer _serializer = new PersistentGrantSerializer();
    private readonly IHandleGenerationService _handleGenerationService = Mock.Of<IHandleGenerationService>();
    private readonly ITelemetryService _telemetry =  Mock.Of<ITelemetryService>();
    private readonly ITrace _trace =  Mock.Of<ITrace>();
    private readonly ILogger<DefaultUserConsentStore> _logger = NullLogger<DefaultUserConsentStore>.Instance;
    
    private DefaultUserConsentStore CreateSut()
    {
        return new DefaultUserConsentStore(_store, _serializer, _handleGenerationService, _telemetry, _logger);
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
        
        Mock.Get(_store)
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

        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHexHashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync((PersistedGrant) null);

        Mock.Get(_store)
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

        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHexHashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync((PersistedGrant) null);

        Mock.Get(_store)
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
    
    [Fact]
    public async Task StoreUserConsentAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
        const string baseHandle = "test_base_handle";
        var expectedHandle = baseHandle + DefaultAuthorizationCodeStore.HexEncodingSuffix;

        Mock.Get(_handleGenerationService)
            .Setup(x => x.GenerateAsync())
            .ReturnsAsync(baseHandle);

        Mock.Get(_store)
            .Setup(x => x.StoreAsync(It.IsAny<PersistedGrant>()))
            .Returns(Task.CompletedTask);

        var consent = new Consent();
        var sut = CreateSut();

        await sut.StoreUserConsentAsync(consent);

        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "StoreUserConsentAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetUserConsentAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
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
        
        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHexHashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        await sut.GetUserConsentAsync(fakeConsent.SubjectId, fakeConsent.ClientId);

        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "GetUserConsentAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task RemoveUserConsentAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
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
        
        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHexHashedKey(fakeConsent.ClientId, fakeConsent.SubjectId)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        await sut.RemoveUserConsentAsync(fakeConsent.SubjectId, fakeConsent.ClientId);
        
        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "RemoveUserConsentAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }
}