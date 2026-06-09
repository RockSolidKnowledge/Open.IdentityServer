// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Events;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Secrets;

public class ApiSecretValidation
{
    private const string Category = "API Secret Validation";

    private readonly Mock<IResourceStore> _resourceStore;
    private readonly Mock<ISecretsListParser> _parser;
    private readonly Mock<ISecretsListValidator> _validator;
    private readonly Mock<ITelemetryService> _telemetry;
    private readonly TestEventService _events;
    private readonly ApiSecretValidator _subject;

    public ApiSecretValidation()
    {
        _resourceStore = new Mock<IResourceStore>();
        _parser = new Mock<ISecretsListParser>();
        _validator = new Mock<ISecretsListValidator>();
        _telemetry = new Mock<ITelemetryService>();
        _events = new TestEventService();

        _subject = new ApiSecretValidator(
            _resourceStore.Object,
            _parser.Object,
            _validator.Object,
            _events,
            _telemetry.Object,
            TestLogger.Create<ApiSecretValidator>());
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task no_secret_found_should_fail()
    {
        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((ParsedSecret)null);

        var result = await _subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        result.Resource.Should().BeNull();
        _events.AssertEventWasRaised<ApiAuthenticationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task no_secret_found_should_emit_telemetry_count()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new("client", "unknown"),
            new("error", "No API id or secret found")
        };
        TelemetryTag[] invokedTags = null;
        _telemetry.Setup(x => x.CountApiSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => invokedTags = tags)
            .Verifiable(Times.Once);
        
        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((ParsedSecret)null);

        await _subject.ValidateAsync(new DefaultHttpContext());
        
        _telemetry.Verify();
        invokedTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task unknown_api_resource_should_fail()
    {
        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "unknown_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource>());

        var result = await _subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        result.Resource.Should().BeNull();
        _events.AssertEventWasRaised<ApiAuthenticationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task unknown_api_resource_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new("client", "unknown_api"),
            new("error", "Unknown API resource")
        };
        TelemetryTag[] invokedTags = null;
        _telemetry.Setup(x => x.CountApiSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => invokedTags = tags)
            .Verifiable(Times.Once);
        
        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "unknown_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource>());

        await _subject.ValidateAsync(new DefaultHttpContext());
        
        _telemetry.Verify();
        invokedTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task multiple_api_resources_should_fail()
    {
        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "api_id", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource>()
            {
                new ApiResource("api_id"),
                new ApiResource("api_id"),
            });

        var result = await _subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        result.Resource.Should().BeNull();
        _events.AssertEventWasRaised<ApiAuthenticationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task multiple_api_resources_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new("client", "api_id"),
            new("error", "Invalid API resource")
        };
        TelemetryTag[] invokedTags = null;
        _telemetry.Setup(x => x.CountApiSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => invokedTags = tags)
            .Verifiable(Times.Once);
        
        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "api_id", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource>()
            {
                new ApiResource("api_id"),
                new ApiResource("api_id"),
            });

        await _subject.ValidateAsync(new DefaultHttpContext());
        
        _telemetry.Verify();
        invokedTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task disabled_api_resource_should_fail()
    {
        var api = new ApiResource("my_api") { Enabled = false };

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "my_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource> { api });

        var result = await _subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        result.Resource.Should().BeNull();
        _events.AssertEventWasRaised<ApiAuthenticationFailureEvent>();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task disabled_api_resource_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new("client", "my_api"),
            new("error", "API resource not enabled")
        };
        TelemetryTag[] invokedTags = null;
        _telemetry.Setup(x => x.CountApiSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => invokedTags = tags)
            .Verifiable(Times.Once);
        
        var api = new ApiResource("my_api") { Enabled = false };

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "my_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource> { api });

        await _subject.ValidateAsync(new DefaultHttpContext());
        
        _telemetry.Verify();
        invokedTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task invalid_secret_should_fail()
    {
        var api = new ApiResource("my_api");

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "my_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource> { api });

        _validator.Setup(x => x.ValidateAsync(It.IsAny<IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = false });

        var result = await _subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeTrue();
        result.Resource.Should().BeNull();
        _events.AssertEventWasRaised<ApiAuthenticationFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task invalid_secret_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new("client", "my_api"),
            new("error", "Invalid API secret")
        };
        TelemetryTag[] invokedTags = null;
        _telemetry.Setup(x => x.CountApiSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => invokedTags = tags)
            .Verifiable(Times.Once);
        
        var api = new ApiResource("my_api");

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "my_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource> { api });

        _validator.Setup(x => x.ValidateAsync(It.IsAny<IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = false });

        await _subject.ValidateAsync(new DefaultHttpContext());
        
        _telemetry.Verify();
        invokedTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task valid_secret_should_succeed()
    {
        var api = new ApiResource("my_api");

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "my_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource> { api });

        _validator.Setup(x => x.ValidateAsync(It.IsAny<IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = true });

        var result = await _subject.ValidateAsync(new DefaultHttpContext());

        result.IsError.Should().BeFalse();
        result.Resource.Should().NotBeNull();
        result.Resource.Name.Should().Be("my_api");
        _events.AssertEventWasRaised<ApiAuthenticationSuccessEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task valid_secret_should_emit_telemetry_signal()
    {
        TelemetryTag[] expectedTags = new TelemetryTag[]
        {
            new("client", "my_api"),
            new(TelemetryConstants.TagConstants.AuthMethod, "SharedSecret")
        };
        TelemetryTag[] invokedTags = null;
        _telemetry.Setup(x => x.CountApiSecretValidation(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => invokedTags = tags)
            .Verifiable(Times.Once);
        
        var api = new ApiResource("my_api");

        _parser.Setup(x => x.ParseAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ParsedSecret { Id = "my_api", Type = "SharedSecret" });

        _resourceStore.Setup(x => x.FindApiResourcesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<ApiResource> { api });

        _validator.Setup(x => x.ValidateAsync(It.IsAny<IEnumerable<Secret>>(), It.IsAny<ParsedSecret>()))
            .ReturnsAsync(new SecretValidationResult { Success = true });

        await _subject.ValidateAsync(new DefaultHttpContext());
        
        _telemetry.Verify();
        invokedTags.Should().BeEquivalentTo(expectedTags);
    }
}