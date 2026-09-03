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
using Open.IdentityServer.Configuration.DependencyInjection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class DefaultServerSideSessionRefreshTokenServiceTests
{
    private IRefreshTokenService decoratedService = Mock.Of<IRefreshTokenService>();
    private IUserSessionEventsService userSessionEventsService = Mock.Of<IUserSessionEventsService>();
    private ITelemetryService telemetry = Mock.Of<ITelemetryService>();

    private DefaultServerSideSessionRefreshTokenService CreateSut() =>
        new(new Decorator<IRefreshTokenService>(decoratedService), userSessionEventsService, telemetry);

    [Fact]
    public async Task ValidateRefreshTokenAsync_WhenDecoratedResultIsInvalid_ShouldNotValidateUserSession()
    {
        const string fakeToken = "fake_token";
        Client fakeClient = new Client();

        var fakeResult = new TokenValidationResult
        {
            IsError = true,
            Error = "fake_error"
        };

        Mock.Get(decoratedService)
            .Setup(x => x.ValidateRefreshTokenAsync(fakeToken, fakeClient))
            .ReturnsAsync(fakeResult);
        
        DefaultServerSideSessionRefreshTokenService sut = CreateSut();
        
        TokenValidationResult actual = await sut.ValidateRefreshTokenAsync(fakeToken, fakeClient);
        
        actual.Should().Be(fakeResult);
        
        Mock.Get(userSessionEventsService)
            .Verify(x => x.ValidateSession(It.IsAny<ValidateUserSessionEventContext>()), Times.Never);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WhenDecoratedResultIsValid_AndValidateUserSessionUnsuccessful_ShouldReturnErrorResult()
    {
        const string fakeToken = "fake_token";
        const string fakeSubject = "fakeSubject";
        const string fakeSession = "fakeSession";
        Client fakeClient = new Client();

        var fakeResult = new TokenValidationResult
        {
            RefreshToken = new RefreshToken { Subject = new IdentityServerUser(fakeSubject).CreatePrincipal(), SessionId = fakeSession },
            Client = fakeClient, IsError = false,
        };

        Mock.Get(decoratedService)
            .Setup(x => x.ValidateRefreshTokenAsync(fakeToken, fakeClient))
            .ReturnsAsync(fakeResult);

        Mock.Get(userSessionEventsService)
            .Setup(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)))
            .ReturnsAsync(false);
        
        DefaultServerSideSessionRefreshTokenService sut = CreateSut();
        
        TokenValidationResult actual = await sut.ValidateRefreshTokenAsync(fakeToken, fakeClient);

        actual.IsError.Should().BeTrue();
        actual.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        
        Mock.Get(userSessionEventsService)
            .Verify(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)));
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WhenDecoratedResultIsValid_AndValidateUserSessionSuccessful_ShouldReturnDecoratedServiceResult()
    {
        const string fakeToken = "fake_token";
        const string fakeSubject = "fakeSubject";
        const string fakeSession = "fakeSession";
        Client fakeClient = new Client();

        var fakeResult = new TokenValidationResult
        {
            RefreshToken = new RefreshToken { Subject = new IdentityServerUser(fakeSubject).CreatePrincipal(), SessionId = fakeSession },
            Client = fakeClient, IsError = false,
        };
        Mock.Get(decoratedService)
            .Setup(x => x.ValidateRefreshTokenAsync(fakeToken, fakeClient))
            .ReturnsAsync(fakeResult);

        Mock.Get(userSessionEventsService)
            .Setup(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)))
            .ReturnsAsync(true);
        
        DefaultServerSideSessionRefreshTokenService sut = CreateSut();
        
        TokenValidationResult actual = await sut.ValidateRefreshTokenAsync(fakeToken, fakeClient);
        
        actual.Should().Be(fakeResult);
        
        Mock.Get(userSessionEventsService)
            .Verify(x => x.ValidateSession(It.Is<ValidateUserSessionEventContext>(ctx =>
                ctx.SessionId == fakeSession && ctx.SubjectId == fakeSubject && ctx.Client == fakeClient)));
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_ShouldJustUseDecoratedService()
    {
        RefreshTokenCreationRequest fakeRequest = new RefreshTokenCreationRequest
        {
            Subject = null, AccessToken = null, Client = null, AuthorisedScopes = null, 
            AuthorisedResourceIndicators = null, RequestedResourceIndicator = null
        };
        string fakeToken = "fake_refresh_token";
        
        DefaultServerSideSessionRefreshTokenService sut = CreateSut();
        
        Mock.Get(decoratedService)
            .Setup(x => x.CreateRefreshTokenAsync(fakeRequest))
            .ReturnsAsync(fakeToken);
        
        string actual = await sut.CreateRefreshTokenAsync(fakeRequest);
        
        Mock.Get(decoratedService)
            .Verify(x => x.CreateRefreshTokenAsync(fakeRequest));
        
        actual.Should().Be(fakeToken);
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_ShouldJustUseDecoratedService()
    {
        string fakeHandle = "fakeHandle";
        RefreshToken fakeRefreshToken = new RefreshToken();
        Client fakeClient = new Client();
        string fakeToken = "fake_refresh_token";
        
        DefaultServerSideSessionRefreshTokenService sut = CreateSut();
        
        Mock.Get(decoratedService)
            .Setup(x => x.UpdateRefreshTokenAsync(fakeHandle, fakeRefreshToken, fakeClient))
            .ReturnsAsync(fakeToken);
        
        string actual = await sut.UpdateRefreshTokenAsync(fakeHandle, fakeRefreshToken, fakeClient);
        
        Mock.Get(decoratedService)
            .Verify(x => x.UpdateRefreshTokenAsync(fakeHandle, fakeRefreshToken, fakeClient));
        
        actual.Should().Be(fakeToken);
    }

    [Fact]
    public async Task PublicMethods_WithCustomisedLogic_WhenCalled_ShouldTelemetryTrace()
    {
        string fakeHandle = "fakeHandle";
        Client fakeClient = new Client();
        string fakeToken = "fake_refresh_token";

        List<(Func<DefaultServerSideSessionRefreshTokenService, Task> actMethod, string traceMethodName)> methods
            = [
                (store => store.ValidateRefreshTokenAsync(fakeToken, fakeClient), "ValidateRefreshTokenAsync"),
            ];

        DefaultServerSideSessionRefreshTokenService sut = CreateSut();

        foreach ((Func<DefaultServerSideSessionRefreshTokenService, Task> actMethod, string traceMethodName) method in methods)
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
        // typeof(DefaultServerSideSessionRefreshTokenService).GetMethods()
        //     .Where(m => m is { IsPublic: true, IsStatic: false, IsSpecialName: false })
        //     .Where(m => m.DeclaringType == typeof(DefaultServerSideSessionRefreshTokenService))
        //     .Select(m => m.Name)
        //     .Distinct()
        //     .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }
}