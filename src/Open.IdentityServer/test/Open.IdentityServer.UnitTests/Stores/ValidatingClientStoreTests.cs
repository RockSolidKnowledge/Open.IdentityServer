// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Common;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Events;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores;

public class ValidatingClientStoreTests
{
    private const string Category = "Stores - ValidatingClientStore";

    private readonly Mock<IClientStore> _inner = new();
    private readonly Mock<IClientConfigurationValidator> _validator = new();
    private readonly TestEventService _events = new();
    private readonly Mock<ITelemetryService> _telemetry = new();

    private ValidatingClientStore<IClientStore> CreateSubject()
    {
        return new ValidatingClientStore<IClientStore>(
            _inner.Object,
            _validator.Object,
            _events,
            _telemetry.Object,
            TestLogger.Create<ValidatingClientStore<IClientStore>>());
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task find_client_should_return_null_when_inner_store_returns_null()
    {
        var subject = CreateSubject();

        _inner.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync((Client)null);

        var result = await subject.FindClientByIdAsync("client");

        result.Should().BeNull();
        _validator.Verify(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()), Times.Never);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task find_client_should_validate_the_client_loaded_from_inner_store()
    {
        var subject = CreateSubject();
        var client = new Client { ClientId = "expected-client" };
        ClientConfigurationValidationContext captured = null;

        _inner.Setup(x => x.FindClientByIdAsync("expected-client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()))
            .Callback<ClientConfigurationValidationContext>(ctx => captured = ctx)
            .Returns(Task.CompletedTask);

        await subject.FindClientByIdAsync("expected-client");

        captured.Should().NotBeNull();
        captured.Client.Should().BeSameAs(client);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task find_client_should_return_client_when_configuration_is_valid()
    {
        var subject = CreateSubject();
        var client = new Client { ClientId = "client" };

        _inner.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()))
            .Returns(Task.CompletedTask);

        var result = await subject.FindClientByIdAsync("client");

        result.Should().NotBeNull();
        result.Should().BeSameAs(client);
        _validator.Verify(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()), Times.Once);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task find_client_should_return_null_and_raise_event_when_configuration_is_invalid()
    {
        var subject = CreateSubject();
        var client = new Client { ClientId = "client" };

        _inner.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()))
            .Callback<ClientConfigurationValidationContext>(ctx =>
            {
                ctx.SetError("invalid client configuration");
            })
            .Returns(Task.CompletedTask);

        var result = await subject.FindClientByIdAsync("client");

        result.Should().BeNull();
        _validator.Verify(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()), Times.Once);
        _events.AssertEventWasRaised<InvalidClientConfigurationEvent>();
    }

    [Fact]
    public async Task FindClientById_OnValidClient_ShouldRaiseTelemetrySignal()
    {
        TelemetryTag[] expectedTags = new[]
        {
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "client"),
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountClientConfigValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once());
        
        var subject = CreateSubject();
        var client = new Client { ClientId = "client" };

        _inner.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()))
            .Returns(Task.CompletedTask);

        var result = await subject.FindClientByIdAsync("client");
        
        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    public async Task FindClientById_OnInvalidClient_ShouldRaiseTelemetrySignal()
    {
        TelemetryTag[] expectedTags = new[]
        {
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "client"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, "invalid client configuration"),
        };
        TelemetryTag[] actualTags = null;
        _telemetry.Setup(t => t.CountClientConfigValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once());
        
        var subject = CreateSubject();
        var client = new Client { ClientId = "client" };

        _inner.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<ClientConfigurationValidationContext>()))
            .Callback<ClientConfigurationValidationContext>(ctx =>
            {
                ctx.SetError("invalid client configuration");
            })
            .Returns(Task.CompletedTask);

        var result = await subject.FindClientByIdAsync("client");

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }
}