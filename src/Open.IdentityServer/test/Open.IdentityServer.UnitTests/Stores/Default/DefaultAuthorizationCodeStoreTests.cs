// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Stores.Serialization;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.Default;

public class DefaultAuthorizationCodeStoreTests
{
    private readonly IPersistedGrantStore _store = Mock.Of<IPersistedGrantStore>();
    private readonly IPersistentGrantSerializer _serializer = new PersistentGrantSerializer();
    private readonly IHandleGenerationService _handleGenerationService = Mock.Of<IHandleGenerationService>();
    private readonly ITelemetryService _telemetry = Mock.Of<ITelemetryService>();
    private readonly ITrace _trace = Mock.Of<ITrace>();

    private readonly ILogger<DefaultAuthorizationCodeStore>
        _logger = TestLogger.Create<DefaultAuthorizationCodeStore>();

    private DefaultAuthorizationCodeStore CreateSut() =>
        new DefaultAuthorizationCodeStore(_store, _serializer, _handleGenerationService, _telemetry, _logger);
    
    [Fact]
    public async Task StoreAuthorizationCodeAsync_WhenCalled_ShouldPersistGrantAndReturnHandle()
    {
        const string baseHandle = "test_base_handle";
        var expectedHandle = baseHandle + DefaultAuthorizationCodeStore.HexEncodingSuffix;

        Mock.Get(_handleGenerationService)
            .Setup(x => x.GenerateAsync())
            .ReturnsAsync(baseHandle);

        Mock.Get(_store)
            .Setup(x => x.StoreAsync(It.IsAny<PersistedGrant>()))
            .Returns(Task.CompletedTask);

        var code = CreateAuthorizationCode();
        var sut = CreateSut();

        var handle = await sut.StoreAuthorizationCodeAsync(code);

        handle.Should().Be(expectedHandle);
        Mock.Get(_store).Verify(x => x.StoreAsync(It.Is<PersistedGrant>(g =>
            g.Key == GetHashedKey(expectedHandle) &&
            g.Type == IdentityServerConstants.PersistedGrantTypes.AuthorizationCode &&
            g.ClientId == code.ClientId &&
            g.SessionId == code.SessionId
        )), Times.Once);
    }
    
    [Fact]
    public async Task GetAuthorizationCodeAsync_WhenGrantExists_ShouldReturnDeserializedAuthorizationCode()
    {
        const string handle = "test_handle-1";
        var code = CreateAuthorizationCode();

        var grant = new PersistedGrant
        {
            Key = GetHashedKey(handle),
            Type = IdentityServerConstants.PersistedGrantTypes.AuthorizationCode,
            ClientId = code.ClientId,
            Data = _serializer.Serialize(code),
        };

        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHashedKey(handle)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        var result = await sut.GetAuthorizationCodeAsync(handle);

        result.Should().NotBeNull();
        result.ClientId.Should().Be(code.ClientId);
        result.SessionId.Should().Be(code.SessionId);
        result.Lifetime.Should().Be(code.Lifetime);
    }
    
    [Fact]
    public async Task RemoveAuthorizationCodeAsync_WhenCalled_ShouldRemoveGrantByHashedKey()
    {
        const string handle = "test_handle-1";

        Mock.Get(_store)
            .Setup(x => x.RemoveAsync(GetHashedKey(handle)))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.RemoveAuthorizationCodeAsync(handle);

        Mock.Get(_store).Verify(x => x.RemoveAsync(GetHashedKey(handle)), Times.Once);
    }

    private static AuthorizationCode CreateAuthorizationCode() => new()
    {
        ClientId = "test-client",
        Subject = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user-123") }, "test")),
        SessionId = "session-abc",
        Description = "test description",
        CreationTime = DateTime.UtcNow,
        Lifetime = 300,
    };

    private static string GetHashedKey(string handle) =>
        $"{handle}:{IdentityServerConstants.PersistedGrantTypes.AuthorizationCode}"
            .Sha256(handle.EndsWith(DefaultAuthorizationCodeStore.HexEncodingSuffix));
    
    [Fact]
    public async Task StoreAuthorizationCodeAsync_WhenCalled_ShouldTelemetryTrace()
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

        var code = CreateAuthorizationCode();
        var sut = CreateSut();

        await sut.StoreAuthorizationCodeAsync(code);

        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "StoreAuthorizationCodeAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetAuthorizationCodeAsync_WhenGrantExists_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        
        const string handle = "test_handle-1";
        var code = CreateAuthorizationCode();

        var grant = new PersistedGrant
        {
            Key = GetHashedKey(handle),
            Type = IdentityServerConstants.PersistedGrantTypes.AuthorizationCode,
            ClientId = code.ClientId,
            Data = _serializer.Serialize(code),
        };

        Mock.Get(_store)
            .Setup(x => x.GetAsync(GetHashedKey(handle)))
            .ReturnsAsync(grant);

        var sut = CreateSut();

        await sut.GetAuthorizationCodeAsync(handle);

        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "GetAuthorizationCodeAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task RemoveAuthorizationCodeAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace);
        const string handle = "test_handle-1";

        Mock.Get(_store)
            .Setup(x => x.RemoveAsync(GetHashedKey(handle)))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.RemoveAuthorizationCodeAsync(handle);
        
        Mock.Get(_telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, "RemoveAuthorizationCodeAsync"));
        Mock.Get(_trace).Verify(t => t.Dispose(), Times.Once);
    }
}