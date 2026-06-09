// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Stores.Serialization;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.Default;

public class DefaultReferenceTokenStoreTests
{
    private readonly IPersistedGrantStore _store = Mock.Of<IPersistedGrantStore>();
    private readonly IPersistentGrantSerializer _serializer = new PersistentGrantSerializer();
    private readonly IHandleGenerationService _handleGenerationService = Mock.Of<IHandleGenerationService>();
    private readonly ITelemetryService _telemetry = Mock.Of<ITelemetryService>();
    private readonly ITrace _trace = Mock.Of<ITrace>();

    private DefaultReferenceTokenStore CreateSut() =>
        new DefaultReferenceTokenStore(
            _store,
            _serializer,
            _handleGenerationService,
            _telemetry,
            TestLogger.Create<DefaultReferenceTokenStore>()
        );

    private static Token CreateToken() => new()
    {
        ClientId = "test-client",
        Claims = new[]
        {
            new Claim("sub", "user-123"),
            new Claim("sid", "session-abc"),
        },
        Description = "test description",
        CreationTime = DateTime.UtcNow,
        Lifetime = 3600,
    };

    private static string GetHashedKey(string handle) =>
        $"{handle}:{IdentityServerConstants.PersistedGrantTypes.ReferenceToken}"
            .Sha256(handle.EndsWith(DefaultReferenceTokenStore.HexEncodingSuffix));

    [Fact]
    public async Task StoreReferenceTokenAsync_WhenCalled_ShouldPersistGrantAndReturnHandle()
    {
        const string baseHandle = "test_base_handle";
        var expectedHandle = baseHandle + DefaultReferenceTokenStore.HexEncodingSuffix;

        Mock.Get(_handleGenerationService)
            .Setup(x => x.GenerateAsync())
            .ReturnsAsync(baseHandle);

        Mock.Get(_store)
            .Setup(x => x.StoreAsync(It.IsAny<PersistedGrant>()))
            .Returns(Task.CompletedTask);

        var token = CreateToken();
        var sut = CreateSut();

        var handle = await sut.StoreReferenceTokenAsync(token);

        handle.Should().Be(expectedHandle);
        Mock.Get(_store).Verify(x => x.StoreAsync(It.Is<PersistedGrant>(g =>
            g.Key == GetHashedKey(expectedHandle) &&
            g.Type == IdentityServerConstants.PersistedGrantTypes.ReferenceToken &&
            g.ClientId == token.ClientId &&
            g.SessionId == token.SessionId
        )), Times.Once);
    }

    [Fact]
    public async Task GetReferenceTokenAsync_WhenGrantExists_ShouldReturnDeserializedToken()
    {
        const string handle = "test_handle-1";
        var token = CreateToken();

        var grant = new PersistedGrant
        {
            Key = GetHashedKey(handle),
            Type = IdentityServerConstants.PersistedGrantTypes.ReferenceToken,
            ClientId = token.ClientId,
            Data = _serializer.Serialize(token),
        };

        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHashedKey(handle)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        var result = await sut.GetReferenceTokenAsync(handle);

        result.Should().NotBeNull();
        result.ClientId.Should().Be(token.ClientId);
        result.SessionId.Should().Be(token.SessionId);
        result.Lifetime.Should().Be(token.Lifetime);
    }

    [Fact]
    public async Task GetReferenceTokenAsync_WhenGrantNotFound_ShouldReturnNull()
    {
        const string handle = "missing_handle-1";

        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHashedKey(handle)))
            .ReturnsAsync((PersistedGrant)null);

        var sut = CreateSut();

        var result = await sut.GetReferenceTokenAsync(handle);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveReferenceTokenAsync_WhenCalled_ShouldRemoveGrantByHashedKey()
    {
        const string handle = "test_handle-1";

        Mock.Get(_store)
            .Setup(x => x.RemoveAsync(GetHashedKey(handle)))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.RemoveReferenceTokenAsync(handle);

        Mock.Get(_store).Verify(x => x.RemoveAsync(GetHashedKey(handle)), Times.Once);
    }

    [Fact]
    public async Task RemoveReferenceTokensAsync_WhenCalled_ShouldRemoveAllGrantsBySubjectAndClient()
    {
        const string subjectId = "user-123";
        const string clientId = "test-client";

        Mock.Get(_store)
            .Setup(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.RemoveReferenceTokensAsync(subjectId, clientId);

        Mock.Get(_store).Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f =>
            f.SubjectId == subjectId &&
            f.ClientId == clientId &&
            f.Type == IdentityServerConstants.PersistedGrantTypes.ReferenceToken
        )), Times.Once);
    }

    [Fact]
    public async Task StoreReferenceTokenAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
        const string baseHandle = "test_base_handle";

        Mock.Get(_handleGenerationService)
            .Setup(x => x.GenerateAsync())
            .ReturnsAsync(baseHandle);

        Mock.Get(_store)
            .Setup(x => x.StoreAsync(It.IsAny<PersistedGrant>()))
            .Returns(Task.CompletedTask);

        var token = CreateToken();
        var sut = CreateSut();

        await sut.StoreReferenceTokenAsync(token);
        
        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut,"StoreReferenceTokenAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task GetReferenceTokenAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
        const string handle = "test_handle-1";
        var token = CreateToken();

        var grant = new PersistedGrant
        {
            Key = GetHashedKey(handle),
            Type = IdentityServerConstants.PersistedGrantTypes.ReferenceToken,
            ClientId = token.ClientId,
            Data = _serializer.Serialize(token),
        };

        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHashedKey(handle)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        await sut.GetReferenceTokenAsync(handle);
        
        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut,"GetReferenceTokenAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RemoveReferenceTokenAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
        const string handle = "test_handle-1";

        Mock.Get(_store)
            .Setup(x => x.RemoveAsync(GetHashedKey(handle)))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.RemoveReferenceTokenAsync(handle);
        
        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "RemoveReferenceTokenAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RemoveReferenceTokensAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
        const string subjectId = "user-123";
        const string clientId = "test-client";

        Mock.Get(_store)
            .Setup(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.RemoveReferenceTokensAsync(subjectId, clientId);
        
        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "RemoveReferenceTokensAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }
}