// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Events;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class ClientSecretValidatorTests
{
    private const string Category = "Validation - ClientSecretValidator";

    private readonly Mock<IClientStore> _clients = new();
    private readonly Mock<ISecretsListParser> _parser = new();
    private readonly Mock<ISecretsListValidator> _validator = new();
    private readonly TestEventService _events = new();
    private readonly Mock<ITelemetryService> _telemetry = new();

    private ClientSecretValidator CreateSubject()
    {
        return new ClientSecretValidator(
            _clients.Object,
            _parser.Object,
            _validator.Object,
            _events,
            _telemetry?.Object,
            TestLogger.Create<ClientSecretValidator>());
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenNoSecretFound_ShouldFailAndRaiseEvent()
    {
        var subject = CreateSubject();

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((ParsedSecret)null);

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();

        _events.AssertEventWasRaised<ClientAuthenticationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenNoSecretFound_ShouldRaiseTelemetrySignal()
    {
        TelemetryTag[] actualTags = null;
        TelemetryTag[] expectedTags = [
            new(TelemetryConstants.TagConstants.Client, "unknown"),
            new(TelemetryConstants.TagConstants.Error, "No client id found")
        ];
        _telemetry.Setup(t => t.CountClientSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((ParsedSecret)null);

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();

        _events.AssertEventWasRaised<ClientAuthenticationFailureEvent>();

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClientUnknown_ShouldFailAndRaiseEvent()
    {
        var subject = CreateSubject();

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "unknown-client", Type = "SharedSecret" });

        _clients.Setup(x => x.FindClientByIdAsync("unknown-client"))
            .ReturnsAsync((Client)null);

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        _events.AssertEventWasRaised<ClientAuthenticationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClientUnknown_ShouldRaiseTelemetrySignal()
    {
        TelemetryTag[] actualTags = null;
        TelemetryTag[] expectedTags = [
            new(TelemetryConstants.TagConstants.Client, "unknown-client"),
            new(TelemetryConstants.TagConstants.Error, "Unknown client")
        ];
        _telemetry.Setup(t => t.CountClientSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "unknown-client", Type = "SharedSecret" });

        _clients.Setup(x => x.FindClientByIdAsync("unknown-client"))
            .ReturnsAsync((Client)null);

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        _events.AssertEventWasRaised<ClientAuthenticationFailureEvent>();

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClientSecretInvalid_ShouldFailAndRaiseEvent()
    {
        var subject = CreateSubject();
        var client = new Client { ClientId = "client" };

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "client", Type = "SharedSecret" });

        _clients.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<System.Collections.Generic.IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = false });

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        _events.AssertEventWasRaised<ClientAuthenticationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClientSecretInvalid_ShouldRaiseTelemetrySignal()
    {
        TelemetryTag[] actualTags = null;
        TelemetryTag[] expectedTags = [
            new(TelemetryConstants.TagConstants.Client, "client_id"),
            new(TelemetryConstants.TagConstants.Error, "Invalid client secret")
        ];
        _telemetry.Setup(t => t.CountClientSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);

        var subject = CreateSubject();
        var client = new Client { ClientId = "client_id" };

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "client_id", Type = "SharedSecret" });

        _clients.Setup(x => x.FindClientByIdAsync("client_id"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<System.Collections.Generic.IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = false });

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        _events.AssertEventWasRaised<ClientAuthenticationFailureEvent>();

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClientSecretValid_ShouldPassAndRaiseEvent()
    {
        var subject = CreateSubject();
        var client = new Client { ClientId = "client", Enabled = true };

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "client", Type = "SharedSecret" });

        _clients.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<System.Collections.Generic.IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = true });

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeFalse();

        result.Client.Should().BeSameAs(client);

        _events.AssertEventWasRaised<ClientAuthenticationSuccessEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClientSecretValid_ShouldRaiseTelemetrySignal()
    {
        TelemetryTag[] actualTags = null;
        TelemetryTag[] expectedTags = [
            new(TelemetryConstants.TagConstants.Client, "client"),
            new(TelemetryConstants.TagConstants.AuthMethod, "SharedSecret")
        ];
        _telemetry.Setup(t => t.CountClientSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);

        var subject = CreateSubject();
        var client = new Client { ClientId = "client", Enabled = true };

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "client", Type = "SharedSecret" });

        _clients.Setup(x => x.FindClientByIdAsync("client"))
            .ReturnsAsync(client);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<System.Collections.Generic.IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = true });

        var result = await subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeFalse();

        result.Client.Should().BeSameAs(client);

        _events.AssertEventWasRaised<ClientAuthenticationSuccessEvent>();

        _telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }
}