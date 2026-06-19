// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Events;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.Default;

public class IntrospectionResponseGeneratorTests
{
    private const string Category = "ResponseHandling";

    private readonly TestEventService _events = new();
    private readonly Mock<ITelemetryService> _telemetry = new();

    private IntrospectionResponseGenerator CreateSubject()
    {
        return new IntrospectionResponseGenerator(
            _events,
            _telemetry.Object,
            TestLogger.Create<IntrospectionResponseGenerator>());
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_inactive_response_and_raise_success_event_for_inactive_token()
    {
        var subject = CreateSubject();
        var api = new ApiResource("api1")
        {
            Scopes = { "scope1" }
        };

        var validationResult = new IntrospectionRequestValidationResult
        {
            IsActive = false,
            Api = api,
            Claims = new List<Claim>()
        };

        var result = await subject.ProcessAsync(validationResult);

        result.Should().NotBeNull();
        result.Should().ContainKey("active");
        result["active"].Should().Be(false);

        _events.AssertEventWasRaised<TokenIntrospectionSuccessEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_emit_telemetry_signal_for_inactive_token()
    {
        _telemetry.Setup(t => t.CountTokenIntrospection("api1", false))
            .Verifiable(Times.Once);
        
        var subject = CreateSubject();
        var api = new ApiResource("api1")
        {
            Scopes = { "scope1" }
        };

        var validationResult = new IntrospectionRequestValidationResult
        {
            IsActive = false,
            Api = api,
            Claims = new List<Claim>()
        };

        await subject.ProcessAsync(validationResult);
        
        _telemetry.Verify();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_inactive_response_and_raise_failure_event_when_expected_scopes_are_missing()
    {
        var subject = CreateSubject();
        var api = new ApiResource("api1")
        {
            Scopes = { "scope1" }
        };

        var validationResult = new IntrospectionRequestValidationResult
        {
            IsActive = true,
            Api = api,
            Token = "token",
            Claims = new List<Claim>
            {
                new(JwtClaimTypes.Scope, "different-scope"),
                new("sub", "123")
            }
        };

        var result = await subject.ProcessAsync(validationResult);

        result.Should().NotBeNull();
        result.Should().ContainKey("active");
        result["active"].Should().Be(false);

        _events.AssertEventWasRaised<TokenIntrospectionFailureEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_emit_telemetry_signal_when_expected_scopes_are_missing()
    {
        _telemetry.Setup(t => t.CountTokenIntrospection("api1", null, "Expected scopes are missing"))
            .Verifiable(Times.Once);

        var subject = CreateSubject();
        var api = new ApiResource("api1")
        {
            Scopes = { "scope1" }
        };

        var validationResult = new IntrospectionRequestValidationResult
        {
            IsActive = true,
            Api = api,
            Token = "token",
            Claims = new List<Claim>
            {
                new(JwtClaimTypes.Scope, "different-scope"),
                new("sub", "123")
            }
        };

        await subject.ProcessAsync(validationResult);
        
        _telemetry.Verify();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_return_active_response_and_raise_success_event_when_expected_scopes_are_present()
    {
        var subject = CreateSubject();
        var api = new ApiResource("api1")
        {
            Scopes = { "scope1", "scope2" }
        };

        var validationResult = new IntrospectionRequestValidationResult
        {
            IsActive = true,
            Api = api,
            Claims = new List<Claim>
            {
                new("sub", "123"),
                new("client_id", "client"),
                new(JwtClaimTypes.Scope, "scope1"),
                new(JwtClaimTypes.Scope, "other-scope")
            }
        };

        var result = await subject.ProcessAsync(validationResult);

        result.Should().NotBeNull();
        result.Should().ContainKey("active");
        result["active"].Should().Be(true);
        result.Should().ContainKey("sub");
        result["sub"].Should().Be("123");
        result.Should().ContainKey("client_id");
        result["client_id"].Should().Be("client");
        result.Should().ContainKey("scope");
        result["scope"].Should().Be("scope1");

        _events.AssertEventWasRaised<TokenIntrospectionSuccessEvent>();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task process_should_emit_telemetry_signal_when_expected_scopes_are_present()
    {
        _telemetry.Setup(t => t.CountTokenIntrospection( "api1", true, null))
            .Verifiable(Times.Once);

        var subject = CreateSubject();
        var api = new ApiResource("api1")
        {
            Scopes = { "scope1", "scope2" }
        };

        var validationResult = new IntrospectionRequestValidationResult
        {
            IsActive = true,
            Api = api,
            Claims = new List<Claim>
            {
                new("sub", "123"),
                new("client_id", "client"),
                new(JwtClaimTypes.Scope, "scope1"),
                new(JwtClaimTypes.Scope, "other-scope")
            }
        };

        await subject.ProcessAsync(validationResult);
        
        _telemetry.Verify();
    }

    [Fact]
    public async Task process_should_initiate_telemetry_trace()
    {
        var subject = CreateSubject();
        var api = new ApiResource("api1")
        {
            Scopes = { "scope1", "scope2" }
        };

        var validationResult = new IntrospectionRequestValidationResult
        {
            IsActive = true,
            Api = api,
            Claims = new List<Claim>
            {
                new("sub", "123"),
                new("client_id", "client"),
                new(JwtClaimTypes.Scope, "scope1"),
                new(JwtClaimTypes.Scope, "other-scope")
            }
        };

        await subject.ProcessAsync(validationResult);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic,
            subject,
            "ProcessAsync"));
    }
}