// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Events;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class TokenRequestValidatorTests
{
    private static NameValueCollection PasswordGrantParameters(
        string userName = "bob", string password = "bob", string scope = "resource") =>
        new()
        {
            [OidcConstants.TokenRequest.GrantType] = OidcConstants.GrantTypes.Password,
            [OidcConstants.TokenRequest.UserName] = userName,
            [OidcConstants.TokenRequest.Password] = password,
            [OidcConstants.TokenRequest.Scope] = scope
        };

    private static IClientStore Clients { get; } = Factory.CreateClientStore();

    // Raising internal events
    
    [Fact]
    public async Task PasswordGrant_WhenCredentialsValidAndUserActive_RaisesUserLoginSuccessEvent()
    {
        var events = new TestEventService();
        var subject = Factory.CreateTokenRequestValidator(events: events);
        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        // bob / bob succeeds in TestResourceOwnerPasswordValidator (username == password)
        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(userName: "bob", password: "bob"),
            client.ToValidationResult());

        result.IsError.Should().BeFalse();

        var evt = events.AssertEventWasRaised<UserLoginSuccessEvent>();
        evt.Username.Should().Be("bob");
        evt.SubjectId.Should().Be("bob");   // GrantValidationResult uses userName as subjectId
        evt.ClientId.Should().Be("roclient");
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsUnsupportedGrantType_RaisesUserLoginFailureEvent()
    {
        var events = new TestEventService();
        var subject = Factory.CreateTokenRequestValidator(
            resourceOwnerValidator: new NotSupportedResourceOwnerPasswordValidator(TestLogger.Create<NotSupportedResourceOwnerPasswordValidator>()),
            events: events);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);

        var evt = events.AssertEventWasRaised<UserLoginFailureEvent>();
        evt.Username.Should().Be("bob");
        evt.Message.Should().Be("password grant type not supported");
        evt.ClientId.Should().Be("roclient");
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsErrorWithNoDescription_RaisesUserLoginFailureEventWithDefaultDescription()
    {
        var events = new TestEventService();
        var subject = Factory.CreateTokenRequestValidator(
            resourceOwnerValidator: new TestResourceOwnerPasswordValidator(TokenRequestErrors.InvalidGrant),
            events: events);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.ErrorDescription.Should().Be("invalid_username_or_password");

        var evt = events.AssertEventWasRaised<UserLoginFailureEvent>();
        evt.Username.Should().Be("bob");
        evt.Message.Should().Be("invalid_username_or_password");
        evt.ClientId.Should().Be("roclient");
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsErrorWithDescription_RaisesUserLoginFailureEventWithThatDescription()
    {
        var events = new TestEventService();
        var subject = Factory.CreateTokenRequestValidator(
            resourceOwnerValidator: new TestResourceOwnerPasswordValidator(
                TokenRequestErrors.InvalidGrant, "account_locked"),
            events: events);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.ErrorDescription.Should().Be("account_locked");

        var evt = events.AssertEventWasRaised<UserLoginFailureEvent>();
        evt.Username.Should().Be("bob");
        evt.Message.Should().Be("account_locked");
        evt.ClientId.Should().Be("roclient");
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsNullSubject_RaisesUserLoginFailureEvent()
    {
        var events = new TestEventService();
        var subject = Factory.CreateTokenRequestValidator(events: events);
        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        // Mismatched credentials: TestResourceOwnerPasswordValidator leaves
        // context.Result unset (default GrantValidationResult has null Subject
        // and IsError == false), triggering the null-subject guard.
        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(userName: "bob", password: "wrong"),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();

        var evt = events.AssertEventWasRaised<UserLoginFailureEvent>();
        evt.Username.Should().Be("bob");
        evt.Message.Should().Be("invalid_username_or_password");
        evt.ClientId.Should().Be("roclient");
    }
    
    [Fact]
    public async Task PasswordGrant_WhenUserIsInactive_RaisesUserLoginFailureEvent()
    {
        var events = new TestEventService();
        var subject  = Factory.CreateTokenRequestValidator(
            profile: new TestProfileService(shouldBeActive: false),
            events: events);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        // bob / bob succeeds credential validation but profile says inactive
        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(userName: "bob", password: "bob"),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);

        var evt = events.AssertEventWasRaised<UserLoginFailureEvent>();
        evt.Username.Should().Be("bob");
        evt.Message.Should().Be("user is inactive");
        evt.ClientId.Should().Be("roclient");
    }
    
    // Emitting telemetry signals
    
    [Fact]
    public async Task PasswordGrant_WhenCredentialsValidAndUserActive_EmitsTelemetrySignal()
    {
        var telemetry = new Mock<ITelemetryService>();
        TelemetryTag[] expectedTags = [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "roclient"),
        ];
        TelemetryTag[] actualTags = null;
        telemetry.Setup(t => t.CountResourceOwnerAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);

        var subject = Factory.CreateTokenRequestValidator(telemetry: telemetry.Object);
        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        // bob / bob succeeds in TestResourceOwnerPasswordValidator (username == password)
        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(userName: "bob", password: "bob"),
            client.ToValidationResult());

        result.IsError.Should().BeFalse();

        telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsUnsupportedGrantType_EmitsTelemetrySignal()
    {
        var telemetry = new Mock<ITelemetryService>();
        TelemetryTag[] expectedTags = [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "roclient"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, "password grant type not supported")
        ];
        TelemetryTag[] actualTags = null;
        telemetry.Setup(t => t.CountResourceOwnerAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);

        var subject = Factory.CreateTokenRequestValidator(
            resourceOwnerValidator: new NotSupportedResourceOwnerPasswordValidator(TestLogger.Create<NotSupportedResourceOwnerPasswordValidator>()),
            telemetry: telemetry.Object);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();

        telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsErrorWithNoDescription_EmitsTelemetrySignal()
    {
        var telemetry = new Mock<ITelemetryService>();
        TelemetryTag[] expectedTags = [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "roclient"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, "invalid_username_or_password")
        ];
        TelemetryTag[] actualTags = null;
        telemetry.Setup(t => t.CountResourceOwnerAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);

        var subject = Factory.CreateTokenRequestValidator(
            resourceOwnerValidator: new TestResourceOwnerPasswordValidator(TokenRequestErrors.InvalidGrant),
            telemetry: telemetry.Object);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();

        telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsErrorWithDescription_EmitsTelemetrySignal()
    {
        var telemetry = new Mock<ITelemetryService>();
        TelemetryTag[] expectedTags = [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "roclient"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, "account_locked")
        ];
        TelemetryTag[] actualTags = null;
        telemetry.Setup(t => t.CountResourceOwnerAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);

        var subject = Factory.CreateTokenRequestValidator(
            resourceOwnerValidator: new TestResourceOwnerPasswordValidator(
                TokenRequestErrors.InvalidGrant, "account_locked"),
            telemetry: telemetry.Object);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();

        telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    public async Task PasswordGrant_WhenValidatorReturnsNullSubject_EmitsTelemetrySignal()
    {
        var telemetry = new Mock<ITelemetryService>();
        TelemetryTag[] expectedTags = [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "roclient"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, "invalid_username_or_password")
        ];
        TelemetryTag[] actualTags = null;
        telemetry.Setup(t => t.CountResourceOwnerAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);

        var subject = Factory.CreateTokenRequestValidator(telemetry: telemetry.Object);
        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        // Mismatched credentials: TestResourceOwnerPasswordValidator leaves
        // context.Result unset (default GrantValidationResult has null Subject
        // and IsError == false), triggering the null-subject guard.
        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(userName: "bob", password: "wrong"),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();

        telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }
    
    [Fact]
    public async Task PasswordGrant_WhenUserIsInactive_EmitsTelemetrySignal()
    {
        var telemetry = new Mock<ITelemetryService>();
        TelemetryTag[] expectedTags = [
            new TelemetryTag(TelemetryConstants.TagConstants.Client, "roclient"),
            new TelemetryTag(TelemetryConstants.TagConstants.Error, "user is inactive")
        ];
        TelemetryTag[] actualTags = null;
        telemetry.Setup(t => t.CountResourceOwnerAuthentication(It.IsAny<TelemetryTag[]>()))
            .Callback<TelemetryTag[]>(tags => actualTags = tags)
            .Verifiable(Times.Once);
        
        var subject  = Factory.CreateTokenRequestValidator(
            profile: new TestProfileService(shouldBeActive: false),
            telemetry: telemetry.Object);

        var client = await Clients.FindEnabledClientByIdAsync("roclient");

        // bob / bob succeeds credential validation but profile says inactive
        var result = await subject.ValidateRequestAsync(
            PasswordGrantParameters(userName: "bob", password: "bob"),
            client.ToValidationResult());

        result.IsError.Should().BeTrue();

        telemetry.Verify();
        actualTags.Should().BeEquivalentTo(expectedTags);
    }
}