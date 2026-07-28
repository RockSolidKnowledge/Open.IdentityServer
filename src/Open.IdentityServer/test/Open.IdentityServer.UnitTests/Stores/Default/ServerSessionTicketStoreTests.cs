// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.EntityFramework.IntegrationTests;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Stores.Serialization;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.Default;

public class ServerSessionTicketStoreTests
{
    private readonly IIdentityServerServerSideSessionStore serverServerSideSessionStore =
        Mock.Of<IIdentityServerServerSideSessionStore>();

    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly MockDataProtector dataProtector = new();
    private readonly FakeTimeProvider fakeTimeProvider = new();
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly MockLogger<ServerSessionTicketStore> logger = new();

    private static readonly DateTime FakeNow = new(2026, 01, 01, 12, 0, 0, DateTimeKind.Utc);

    public ServerSessionTicketStoreTests()
    {
        fakeTimeProvider.SetUtcNow(FakeNow);

        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector(DataProtectionConstants.ServerSideTicketStorePurpose))
            .Returns(dataProtector);
    }

    private ServerSessionTicketStore CreateSut() => new(serverServerSideSessionStore, dataProtectionProvider,
        fakeTimeProvider, telemetry, logger);

    [Fact]
    public async Task StoreAsync_WhenOptionalValuesNotProvided_ShouldUseCorrectDefaults()
    {
        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();

        AuthenticationTicket authenticationTicket = GenerateAuthenticationTicket(authScheme, subjectId, sessionId);

        IdentityServerServerSideSessions? createdSessionModel = null;
        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.CreateSession(It.IsAny<IdentityServerServerSideSessions>()))
            .Callback<IdentityServerServerSideSessions>((session) => { createdSessionModel = session; });

        ServerSessionTicketStore sut = CreateSut();

        string actualKey = await sut.StoreAsync(authenticationTicket);

        createdSessionModel.Should().NotBeNull();
        createdSessionModel.Key.Should().NotBeNullOrWhiteSpace();
        createdSessionModel.Key.Should().Be(actualKey);
        createdSessionModel.Scheme.Should().Be(authScheme);
        createdSessionModel.SessionId.Should().Be(sessionId);
        createdSessionModel.SubjectId.Should().Be(subjectId);
        createdSessionModel.DisplayName.Should().BeNull();
        createdSessionModel.Created.Should().Be(FakeNow);
        createdSessionModel.Renewed.Should().Be(FakeNow);
        createdSessionModel.Expires.Should().BeNull();

        string expectedJson = JsonSerializer.Serialize(authenticationTicket.ToSerializableObj(),
            ServerSessionTicketStore.JsonSettings);
        dataProtector.ValidateProtectedData(createdSessionModel.Data, expectedJson);
    }

    [Fact]
    public async Task StoreAsync_WhenOptionalValuesProvided_ShouldUseThem()
    {
        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();
        const string displayName = "Fake User";
        DateTime issuedUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        DateTime expiresUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);

        AuthenticationTicket authenticationTicket =
            GenerateAuthenticationTicket(authScheme, subjectId, sessionId, displayName, issuedUtc, expiresUtc);

        IdentityServerServerSideSessions? createdSessionModel = null;
        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.CreateSession(It.IsAny<IdentityServerServerSideSessions>()))
            .Callback<IdentityServerServerSideSessions>((session) => { createdSessionModel = session; });

        ServerSessionTicketStore sut = CreateSut();

        string actualKey = await sut.StoreAsync(authenticationTicket);

        createdSessionModel.Should().NotBeNull();
        createdSessionModel.Key.Should().NotBeNullOrWhiteSpace();
        createdSessionModel.Key.Should().Be(actualKey);
        createdSessionModel.Scheme.Should().Be(authScheme);
        createdSessionModel.SessionId.Should().Be(sessionId);
        createdSessionModel.SubjectId.Should().Be(subjectId);
        createdSessionModel.DisplayName.Should().Be(displayName);
        createdSessionModel.Created.Should().Be(issuedUtc);
        createdSessionModel.Renewed.Should().Be(issuedUtc);
        createdSessionModel.Expires.Should().Be(expiresUtc);

        string expectedJson = JsonSerializer.Serialize(authenticationTicket.ToSerializableObj(),
            ServerSessionTicketStore.JsonSettings);
        dataProtector.ValidateProtectedData(createdSessionModel.Data, expectedJson);
    }

    [Fact]
    public async Task RenewAsync_WhenOptionalValuesNotProvided_ShouldUseCorrectDefaults()
    {
        IdentityServerServerSideSessions existingSession = new IdentityServerServerSideSessions
        {
            Key = Guid.NewGuid().ToString(), Scheme = "AuthScheme", SessionId = Guid.NewGuid().ToString(),
            SubjectId = Guid.NewGuid().ToString(), DisplayName = "John Doe",
            Created = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Renewed = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
            Expires = new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc),
            Data = "EXISTING_PROTEXTEDDAAT",
        };

        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();

        AuthenticationTicket authenticationTicket = GenerateAuthenticationTicket(authScheme, subjectId, sessionId);

        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.GetSession(existingSession.Key))
            .ReturnsAsync(existingSession);

        IdentityServerServerSideSessions? createdSessionModel = null;
        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.UpdateSession(It.IsAny<IdentityServerServerSideSessions>()))
            .Callback<IdentityServerServerSideSessions>((session) => { createdSessionModel = session; });

        ServerSessionTicketStore sut = CreateSut();

        await sut.RenewAsync(existingSession.Key, authenticationTicket);

        createdSessionModel.Should().NotBeNull();
        createdSessionModel.Key.Should().NotBeNullOrWhiteSpace();
        createdSessionModel.Key.Should().Be(existingSession.Key);
        createdSessionModel.Scheme.Should().Be(authScheme);
        createdSessionModel.SessionId.Should().Be(sessionId);
        createdSessionModel.SubjectId.Should().Be(subjectId);
        createdSessionModel.DisplayName.Should().BeNull();
        createdSessionModel.Created.Should().Be(existingSession.Created);
        createdSessionModel.Renewed.Should().Be(FakeNow);
        createdSessionModel.Expires.Should().BeNull();

        string expectedJson = JsonSerializer.Serialize(authenticationTicket.ToSerializableObj(),
            ServerSessionTicketStore.JsonSettings);
        dataProtector.ValidateProtectedData(createdSessionModel.Data, expectedJson);
    }

    [Fact]
    public async Task RenewAsync_WhenOptionalValuesProvided_ShouldUseThem()
    {
        IdentityServerServerSideSessions existingSession = new IdentityServerServerSideSessions
        {
            Key = Guid.NewGuid().ToString(), Scheme = "AuthScheme", SessionId = Guid.NewGuid().ToString(),
            SubjectId = Guid.NewGuid().ToString(), DisplayName = "John Doe",
            Created = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Renewed = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
            Expires = new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc),
            Data = "EXISTING_PROTEXTEDDAAT",
        };

        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();
        const string displayName = "Fake User";
        DateTime issuedUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);
        DateTime expiresUtc = new(2026, 02, 19, 12, 0, 0, DateTimeKind.Utc);

        AuthenticationTicket authenticationTicket =
            GenerateAuthenticationTicket(authScheme, subjectId, sessionId, displayName, issuedUtc, expiresUtc);

        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.GetSession(existingSession.Key))
            .ReturnsAsync(existingSession);

        IdentityServerServerSideSessions? updatedSessionModel = null;
        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.UpdateSession(It.IsAny<IdentityServerServerSideSessions>()))
            .Callback<IdentityServerServerSideSessions>((session) => { updatedSessionModel = session; });

        ServerSessionTicketStore sut = CreateSut();

        await sut.RenewAsync(existingSession.Key, authenticationTicket);

        updatedSessionModel.Should().NotBeNull();
        updatedSessionModel.Key.Should().NotBeNullOrWhiteSpace();
        updatedSessionModel.Key.Should().Be(existingSession.Key);
        updatedSessionModel.Scheme.Should().Be(authScheme);
        updatedSessionModel.SessionId.Should().Be(sessionId);
        updatedSessionModel.SubjectId.Should().Be(subjectId);
        updatedSessionModel.DisplayName.Should().Be(displayName);
        updatedSessionModel.Created.Should().Be(existingSession.Created);
        updatedSessionModel.Renewed.Should().Be(issuedUtc);
        updatedSessionModel.Expires.Should().Be(expiresUtc);

        string expectedJson = JsonSerializer.Serialize(authenticationTicket.ToSerializableObj(),
            ServerSessionTicketStore.JsonSettings);
        dataProtector.ValidateProtectedData(updatedSessionModel.Data, expectedJson);
    }
    
    [Fact]
    public async Task RenewAsync_WhenNoExistingSessionWithKey_ShouldCreateNewSession()
    {
        const string authScheme = "FakeAuthScheme";
        string subjectId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();

        AuthenticationTicket authenticationTicket = GenerateAuthenticationTicket(authScheme, subjectId, sessionId);

        IdentityServerServerSideSessions? createdSessionModel = null;
        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.CreateSession(It.IsAny<IdentityServerServerSideSessions>()))
            .Callback<IdentityServerServerSideSessions>((session) => { createdSessionModel = session; });

        ServerSessionTicketStore sut = CreateSut();
        
        const string nonExistent = "NonExistentSession";
        await sut.RenewAsync(nonExistent, authenticationTicket);

        createdSessionModel.Should().NotBeNull();
        createdSessionModel.Key.Should().NotBeNullOrWhiteSpace();
        createdSessionModel.Key.Should().Be(nonExistent);
        createdSessionModel.Scheme.Should().Be(authScheme);
        createdSessionModel.SessionId.Should().Be(sessionId);
        createdSessionModel.SubjectId.Should().Be(subjectId);
        createdSessionModel.DisplayName.Should().BeNull();
        createdSessionModel.Created.Should().Be(FakeNow);
        createdSessionModel.Renewed.Should().Be(FakeNow);
        createdSessionModel.Expires.Should().BeNull();

        string expectedJson = JsonSerializer.Serialize(authenticationTicket.ToSerializableObj(),
            ServerSessionTicketStore.JsonSettings);
        dataProtector.ValidateProtectedData(createdSessionModel.Data, expectedJson);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RetrieveAsync_WhenArgumentNullOrEmpty_ShouldThrowArgumentException(string? key)
    {
        ServerSessionTicketStore sut = CreateSut();

        Func<Task> act = async () => await sut.RetrieveAsync(key);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RetrieveAsync_WhenNoSessionStoredForKey_ShouldReturnNull()
    {
        ServerSessionTicketStore sut = CreateSut();

        AuthenticationTicket? actual = await sut.RetrieveAsync("non-existent-session");

        actual.Should().BeNull();
    }

    [Fact]
    public async Task RetrieveAsync_WhenSessionStoredForKey_ShouldReturnDeserializedAuthTicket()
    {
        IdentityServerServerSideSessions existingSession = new IdentityServerServerSideSessions
        {
            Key = Guid.NewGuid().ToString(), Scheme = "AuthScheme", SessionId = Guid.NewGuid().ToString(),
            SubjectId = Guid.NewGuid().ToString(), DisplayName = "John Doe",
            Created = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Renewed = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
            Expires = new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc),
        };
        SerializedAuthenticationTicket authenticationTicket =
            GenerateSerializedAuthenticationTicket(existingSession.Scheme, existingSession.SubjectId,
                existingSession.SessionId, existingSession.DisplayName, existingSession.Renewed,
                existingSession.Expires);
        existingSession.Data =
            dataProtector.GenerateFakeProtectedData(JsonSerializer.Serialize(authenticationTicket,
                ServerSessionTicketStore.JsonSettings));

        Mock.Get(serverServerSideSessionStore)
            .Setup(x => x.GetSession(existingSession.Key))
            .ReturnsAsync(existingSession);

        ServerSessionTicketStore sut = CreateSut();
        AuthenticationTicket? actual = await sut.RetrieveAsync(existingSession.Key);

        actual.Should().BeOfType<AuthenticationTicket>();
        actual.AuthenticationScheme.Should().Be(existingSession.Scheme);
        actual.Principal.Identity?.AuthenticationType.Should()
            .BeEquivalentTo(authenticationTicket.User.AuthenticationType);
        actual.Properties.Items.Should().BeEquivalentTo(authenticationTicket.Items);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RemoveAsync_WhenArgumentNullOrEmpty_ShouldThrowArgumentException(string? key)
    {
        ServerSessionTicketStore sut = CreateSut();

        Func<Task> act = async () => await sut.RemoveAsync(key!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallServerSideSessionStoreDelete()
    {
        string keyId = Guid.NewGuid().ToString();

        ServerSessionTicketStore sut = CreateSut();
        await sut.RemoveAsync(keyId);

        Mock.Get(serverServerSideSessionStore)
            .Verify(x => x.DeleteSession(keyId));
    }

    [Fact]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace()
    {
        AuthenticationTicket authTicket =
            GenerateAuthenticationTicket("FakeScheme", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        List<(Func<ServerSessionTicketStore, Task> actMethod, string traceMethodName)> methods
            =
            [
                (store => store.StoreAsync(authTicket), "StoreAsync"),
                (store => store.RenewAsync("FAKE_KEY", authTicket), "RenewAsync"),
                (store => store.RetrieveAsync("FAKE_KEY"), "RetrieveAsync"),
                (store => store.RemoveAsync("FAKE_KEY"), "RemoveAsync")
            ];

        var sut = CreateSut();

        foreach (var method in methods)
        {
            var trace = Mock.Of<ITrace>();
            Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);

            await method.actMethod(sut);

            Mock.Get(telemetry)
                .Verify(t => t.Trace(
                    TelemetryConstants.TraceCategories.Stores, sut, method.traceMethodName), Times.Once);
            Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
        }

        // Assert all methods covered
        typeof(ServerSessionTicketStore).GetMethods()
            .Where(m => m is { IsPublic: true, IsStatic: false, IsSpecialName: false })
            .Where(m => m.DeclaringType == typeof(ServerSessionTicketStore))
            .Select(m => m.Name)
            .Distinct()
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }

    private AuthenticationTicket GenerateAuthenticationTicket(string authScheme, string? subjectId, string? sessionId,
        string? displayName = null, DateTimeOffset? issuedUtc = null, DateTimeOffset? expiresUtc = null)
    {
        IdentityServerUser user = new(subjectId);
        AuthenticationProperties properties = new();

        properties.SetSessionId(sessionId);

        user.DisplayName = displayName;
        properties.IssuedUtc = issuedUtc;
        properties.ExpiresUtc = expiresUtc;

        return new AuthenticationTicket(user.CreatePrincipal(), properties, authScheme);
    }

    private SerializedAuthenticationTicket GenerateSerializedAuthenticationTicket(string authScheme, string? subjectId,
        string? sessionId, string? displayName = null, DateTimeOffset? issuedUtc = null,
        DateTimeOffset? expiresUtc = null)
    {
        List<ClaimLite> claims = [];

        if (subjectId != null)
        {
            claims.Add(new ClaimLite { Type = "sub", Value = subjectId, ValueType = "", Issuer = "", });
        }

        if (displayName != null)
        {
            claims.Add(new ClaimLite { Type = "name", Value = displayName, ValueType = "", Issuer = "", });
        }

        var items = new Dictionary<string, string>();

        if (sessionId != null)
        {
            items["session_id"] = sessionId;
        }

        if (issuedUtc != null)
        {
            items[".issued"] = issuedUtc.Value.ToString("R");
        }

        if (expiresUtc != null)
        {
            items[".expires"] = expiresUtc.Value.ToString("R");
        }

        return new SerializedAuthenticationTicket
        {
            Scheme = authScheme,
            User = new ClaimsPrincipalLite
            {
                AuthenticationType = "Open.IdentityServer",
                Claims = claims.ToArray(),
            },
            Items = items,
        };
    }
}