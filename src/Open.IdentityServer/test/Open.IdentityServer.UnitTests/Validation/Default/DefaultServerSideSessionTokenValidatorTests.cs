// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class DefaultServerSideSessionTokenValidatorTests
{
    private ITokenValidator decoratedService = Mock.Of<ITokenValidator>();
    private IUserSessionEventsService userSessionEventsService = Mock.Of<IUserSessionEventsService>();
    private ITelemetryService telemetry = Mock.Of<ITelemetryService>();

    private DefaultServerSideSessionTokenValidator CreateSut() =>
        new(decoratedService, userSessionEventsService, telemetry);


    [Fact]
    public async Task ValidateAccessTokenAsync_WhenDecoratedResultIsInvalid_ShouldNotValidateUserSession()
    {
        const string fakeToken = "fake_token";

        var fakeResult = new TokenValidationResult { Claims = [], IsError = true, Error = "fake_error" };

        Mock.Get(decoratedService)
            .Setup(x => x.ValidateAccessTokenAsync(fakeToken, It.IsAny<string>()))
            .ReturnsAsync(fakeResult);

        DefaultServerSideSessionTokenValidator sut = CreateSut();

        TokenValidationResult? actual = await sut.ValidateAccessTokenAsync(fakeToken);

        actual.Should().NotBeNull();
        actual.Should().Be(fakeResult);

        Mock.Get(userSessionEventsService)
            .Verify(x => x.ValidateSession(It.IsAny<ValidateUserSessionEventContext>()), Times.Never);
    }

    [Fact]
    public async Task
        ValidateAccessTokenAsync_WhenTokenDoesntContainsSubjectSessionClaims_ShouldNotValidateUserSession()
    {
        const string fakeToken = "fake_token";

        var fakeResult = new TokenValidationResult { Claims = [], IsError = false, };

        Mock.Get(decoratedService)
            .Setup(x => x.ValidateAccessTokenAsync(fakeToken, It.IsAny<string>()))
            .ReturnsAsync(fakeResult);

        DefaultServerSideSessionTokenValidator sut = CreateSut();

        TokenValidationResult? actual = await sut.ValidateAccessTokenAsync(fakeToken);

        actual.Should().NotBeNull();
        actual.Should().Be(fakeResult);

        Mock.Get(userSessionEventsService)
            .Verify(x => x.ValidateSession(It.IsAny<ValidateUserSessionEventContext>()), Times.Never);
    }

    [Fact]
    public async Task
        ValidateAccessTokenAsync_WhenTokenContainsSubjectSessionClaims_AndSessionInValid_ShouldReturnInavlidSessionError()
    {
        const string fakeToken = "fake_token";
        const string fakeSession = "fakeSession";
        const string fakeSubject = "fakeSubject";
        Client fakeClient = new Client();

        var fakeResult = new TokenValidationResult
        {
            Claims =
            [
                new Claim(JwtClaimTypes.SessionId, fakeSession),
                new Claim(JwtClaimTypes.Subject, fakeSubject),
            ],
            Client = fakeClient, IsError = false,
        };

        Mock.Get(decoratedService)
            .Setup(x => x.ValidateAccessTokenAsync(fakeToken, It.IsAny<string>()))
            .ReturnsAsync(fakeResult);

        Mock.Get(userSessionEventsService)
            .Setup(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)))
            .ReturnsAsync(false);

        DefaultServerSideSessionTokenValidator sut = CreateSut();

        TokenValidationResult? actual = await sut.ValidateAccessTokenAsync(fakeToken);

        actual.Should().NotBeNull();
        actual.IsError.Should().BeTrue();
        actual.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);

        Mock.Get(userSessionEventsService)
            .Verify(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)));
    }

    [Fact]
    public async Task
        ValidateAccessTokenAsync_WhenTokenContainsSubjectSessionClaims_AndSessionValid_ShouldReturnDecoratedServiceResult()
    {
        const string fakeToken = "fake_token";
        const string fakeSession = "fakeSession";
        const string fakeSubject = "fakeSubject";
        Client fakeClient = new Client();

        var fakeResult = new TokenValidationResult
        {
            Claims =
            [
                new Claim(JwtClaimTypes.SessionId, fakeSession),
                new Claim(JwtClaimTypes.Subject, fakeSubject),
            ], 
            Client = fakeClient, IsError = false,
        };

        Mock.Get(decoratedService)
            .Setup(x => x.ValidateAccessTokenAsync(fakeToken, It.IsAny<string>()))
            .ReturnsAsync(fakeResult);

        Mock.Get(userSessionEventsService)
            .Setup(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)))
            .ReturnsAsync(true);

        DefaultServerSideSessionTokenValidator sut = CreateSut();

        TokenValidationResult? actual = await sut.ValidateAccessTokenAsync(fakeToken);

        actual.Should().NotBeNull();
        actual.Should().Be(fakeResult);

        Mock.Get(userSessionEventsService)
            .Verify(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)));
    }

    [Fact]
    public async Task ValidateIdentityTokenAsync_ShouldJustUseDecoratedService()
    {
        string fakeToken = "fake_refresh_token";
        string fakeClientId = "";
        bool fakeValidateLifetime = false;

        TokenValidationResult fakeResult = new TokenValidationResult();

        DefaultServerSideSessionTokenValidator sut = CreateSut();

        Mock.Get(decoratedService)
            .Setup(x => x.ValidateIdentityTokenAsync(fakeToken, fakeClientId, fakeValidateLifetime))
            .ReturnsAsync(fakeResult);

        TokenValidationResult actual =
            await sut.ValidateIdentityTokenAsync(fakeToken, fakeClientId, fakeValidateLifetime);

        Mock.Get(decoratedService)
            .Verify(x => x.ValidateIdentityTokenAsync(fakeToken, fakeClientId, fakeValidateLifetime));

        actual.Should().Be(fakeResult);
    }

    [Fact]
    public async Task PublicMethods_WithCustomisedLogic_WhenCalled_ShouldTelemetryTrace()
    {
        string fakeToken = "fake_refresh_token";

        List<(Func<DefaultServerSideSessionTokenValidator, Task> actMethod, string traceMethodName)> methods
            =
            [
                (store => store.ValidateAccessTokenAsync(fakeToken), "ValidateAccessTokenAsync"),
            ];

        DefaultServerSideSessionTokenValidator sut = CreateSut();

        foreach ((Func<DefaultServerSideSessionTokenValidator, Task> actMethod, string traceMethodName) method in
                 methods)
        {
            ITrace trace = Mock.Of<ITrace>();
            Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);

            await method.actMethod(sut);

            Mock.Get(telemetry)
                .Verify(t => t.Trace(
                    TelemetryConstants.TraceCategories.Validation, sut, method.traceMethodName), Times.Once);
            Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
        }

        // Assert all methods covered
        // typeof(DefaultServerSideSessionTokenValidator).GetMethods()
        //     .Where(m => m is { IsPublic: true, IsStatic: false, IsSpecialName: false })
        //     .Where(m => m.DeclaringType == typeof(DefaultServerSideSessionTokenValidator))
        //     .Select(m => m.Name)
        //     .Distinct()
        //     .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }
}