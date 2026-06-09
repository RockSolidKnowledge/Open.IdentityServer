// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Services;
using Xunit;

namespace Open.IdentityServer.UnitTests.Hosting;

public class IdentityServerMiddlewareTests
{
    private readonly IdentityServerMiddleware _subject;
    
    private readonly RequestDelegate _next;
    private bool nextCalled;
    
    private readonly Mock<ILogger<IdentityServerMiddleware>> _loggerMock;
    private readonly Mock<IEndpointRouter> _router;
    private readonly Mock<IUserSession> _userSession;
    private readonly Mock<IEventService> _eventService;
    private readonly Mock<IBackChannelLogoutService> _backChannelLogoutService;
    private readonly Mock<ITelemetryService> _telemetryService;
    private readonly DefaultHttpContext _context;

    public IdentityServerMiddlewareTests()
    {
        _next = context => { 
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        _loggerMock = new Mock<ILogger<IdentityServerMiddleware>>();
        _router = new Mock<IEndpointRouter>();
        _userSession = new Mock<IUserSession>();
        _eventService = new Mock<IEventService>();
        _backChannelLogoutService = new Mock<IBackChannelLogoutService>();
        _telemetryService = new Mock<ITelemetryService>();
        
        _userSession.Setup(x => x.EnsureSessionIdCookieAsync()).Returns(Task.CompletedTask);
        _router.Setup(x => x.Find(It.IsAny<HttpContext>())).Returns((IEndpointHandler)null);
        
        _subject = new IdentityServerMiddleware(_next, _loggerMock.Object);
        
        _context = new DefaultHttpContext();
    }
    
    private async Task InvokeSubjectMiddleware()
    {
        await _subject.Invoke(_context, _router.Object, _userSession.Object, 
            _eventService.Object, _backChannelLogoutService.Object, _telemetryService?.Object);
    }

    [Fact]
    public async Task Invoke_WithNoMatchingEndpoint_ShouldCallNext()
    {
        _router.Setup(x => x.Find(_context)).Returns((IEndpointHandler)null);
        
        await InvokeSubjectMiddleware();
        
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_ShouldEnsureSessionIdCookie()
    {
        await InvokeSubjectMiddleware();
        
        _userSession.Verify(x => x.EnsureSessionIdCookieAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Invoke_WhenRouterLocatesEndpoint_ShouldProcessEndpoint()
    {
        var endpointMock = new Mock<IEndpointHandler>();
        var endpointResult = new Mock<IEndpointResult>();
    
        endpointMock
            .Setup(x => x.ProcessAsync(_context))
            .ReturnsAsync(endpointResult.Object);
    
        _router.Setup(x => x.Find(_context)).Returns(endpointMock.Object);
        
        await InvokeSubjectMiddleware();
        
        endpointMock.Verify(x => x.ProcessAsync(_context), Times.Once);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_WhenProcessedEndpointProducesResult_ShouldExecuteResult()
    {
        var endpointMock = new Mock<IEndpointHandler>();
        var resultMock = new Mock<IEndpointResult>();
    
        endpointMock
            .Setup(x => x.ProcessAsync(_context))
            .ReturnsAsync(resultMock.Object);
    
        _router.Setup(x => x.Find(_context)).Returns(endpointMock.Object);
        
        await InvokeSubjectMiddleware();
        
        resultMock.Verify(x => x.ExecuteAsync(_context), Times.Once);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_OnUnhandledException_ShouldCountTelemetryEvent()
    {
        TelemetryTag[] passedTags = null;
        TelemetryTag[] expectedTags = new[]
        {
            new TelemetryTag(TelemetryConstants.TagConstants.Error, "System.Exception"),
        };
        
        _router.Setup(x => x.Find(_context)).Throws(new Exception("Test exception"));
        _telemetryService.Setup(x => x.CountInternalError(It.IsAny<TelemetryTag[]>()))
            .Callback((TelemetryTag[] tags) => passedTags = tags);
        
        try
        {
            await InvokeSubjectMiddleware();
        }
        catch (Exception e)
        {
            // intentionally swallowed
        }
        
        _telemetryService.Verify(x => x.CountInternalError(It.IsAny<TelemetryTag[]>()), Times.Once);
        
        passedTags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    public async Task Invoke_WhenRouterLocatesEndpoint_ShouldTrackActiveRequest()
    {
        var endpointMock = new Mock<IEndpointHandler>();
        var endpointResult = new Mock<IEndpointResult>();
    
        endpointMock
            .Setup(x => x.ProcessAsync(_context))
            .ReturnsAsync(endpointResult.Object);
    
        _router.Setup(x => x.Find(_context)).Returns(endpointMock.Object);

        var activeRequestDisposable = new Mock<IDisposable>();
        using var telemetryScope = activeRequestDisposable.Object;
        _telemetryService.Setup(x => x.BeginActiveRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(telemetryScope);
        
        await InvokeSubjectMiddleware();
        
        _telemetryService.Verify(x => x.BeginActiveRequest(
            endpointMock.Object.GetType().FullName, 
            _context.Request.Path.ToString()), 
            Times.Once);
        activeRequestDisposable.Verify(x => x.Dispose(), Times.Once);
    }
}