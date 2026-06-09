// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Services.Default;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling;

public class DeviceAuthorizationResponseGeneratorTests
{
    private readonly List<IdentityResource> _identityResources =
        [new IdentityResources.OpenId(), new IdentityResources.Profile()];
    private readonly List<ApiResource> _apiResources = [new("resource") { Scopes = { "api1" } }];
    private readonly List<ApiScope> _scopes = [new("api1")];

    private readonly FakeUserCodeGenerator _fakeUserCodeGenerator = new();
    private readonly IDeviceFlowCodeService _deviceFlowCodeService = new DefaultDeviceFlowCodeService(new InMemoryDeviceFlowStore(), new StubHandleGenerationService(), new NopTelemetryService());
    private readonly IdentityServerOptions _options = new();
    private readonly StubClock _clock = new();
    private readonly Mock<ITelemetryService> _telemetry = new();
    private readonly Mock<ITrace> _trace = new();
        
    private readonly DeviceAuthorizationResponseGenerator _generator;
    private readonly DeviceAuthorizationRequestValidationResult _testResult;
    private const string TestBaseUrl = "http://localhost:5000/";

    public DeviceAuthorizationResponseGeneratorTests()
    {
        _testResult = new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest
        {
            Client = new Client {ClientId = Guid.NewGuid().ToString()},
            IsOpenIdRequest = true,
            ValidatedResources = new ResourceValidationResult()
        });

        _generator = new DeviceAuthorizationResponseGenerator(
            _options,
            new DefaultUserCodeService([new NumericUserCodeGenerator(), _fakeUserCodeGenerator]),
            _deviceFlowCodeService,
            _clock,
            _telemetry.Object,
            new NullLogger<DeviceAuthorizationResponseGenerator>());
    }
    

    [Fact]
    public async Task ProcessAsync_WhenCalled_ShouldTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var creationTime = DateTime.UtcNow;
        _clock.UtcNowFunc = () => creationTime;

        _testResult.ValidatedRequest.Client.UserCodeType = FakeUserCodeGenerator.UserCodeTypeValue;
        await _deviceFlowCodeService.StoreDeviceAuthorizationAsync(FakeUserCodeGenerator.TestCollisionUserCode, new DeviceCode());

        await _generator.ProcessAsync(_testResult, TestBaseUrl);

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic, _generator, "ProcessAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_when_valiationresult_null_exect_exception()
    {
        Func<Task> act = () => _generator.ProcessAsync(null, TestBaseUrl);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessAsync_when_valiationresult_client_null_exect_exception()
    {
        var validationResult = new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest());
        Func <Task> act = () => _generator.ProcessAsync(validationResult, TestBaseUrl);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessAsync_when_baseurl_null_exect_exception()
    {
        Func<Task> act = () => _generator.ProcessAsync(_testResult, null);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ProcessAsync_when_user_code_collision_expect_retry()
    {
        var creationTime = DateTime.UtcNow;
        _clock.UtcNowFunc = () => creationTime;

        _testResult.ValidatedRequest.Client.UserCodeType = FakeUserCodeGenerator.UserCodeTypeValue;
        await _deviceFlowCodeService.StoreDeviceAuthorizationAsync(FakeUserCodeGenerator.TestCollisionUserCode, new DeviceCode());

        var response = await _generator.ProcessAsync(_testResult, TestBaseUrl);

        response.UserCode.Should().Be(FakeUserCodeGenerator.TestUniqueUserCode);
    }

    [Fact]
    public async Task ProcessAsync_when_user_code_collision_retry_limit_reached_expect_error()
    {
        var creationTime = DateTime.UtcNow;
        _clock.UtcNowFunc = () => creationTime;

        _fakeUserCodeGenerator.RetryLimit = 1;
        _testResult.ValidatedRequest.Client.UserCodeType = FakeUserCodeGenerator.UserCodeTypeValue;
        await _deviceFlowCodeService.StoreDeviceAuthorizationAsync(FakeUserCodeGenerator.TestCollisionUserCode, new DeviceCode());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _generator.ProcessAsync(_testResult, TestBaseUrl));
    }

    [Fact]
    public async Task ProcessAsync_when_generated_expect_user_code_stored()
    {
        var creationTime = DateTime.UtcNow;
        _clock.UtcNowFunc = () => creationTime;

        _testResult.ValidatedRequest.RequestedScopes = new List<string> { "openid", "api1" };
        _testResult.ValidatedRequest.ValidatedResources = new ResourceValidationResult(new Resources(
            _identityResources.Where(x=>x.Name == "openid"), 
            _apiResources.Where(x=>x.Name == "resource"), 
            _scopes.Where(x=>x.Name == "api1")));

        var response = await _generator.ProcessAsync(_testResult, TestBaseUrl);

        response.UserCode.Should().NotBeNullOrWhiteSpace();

        var userCode = await _deviceFlowCodeService.FindByUserCodeAsync(response.UserCode);
        userCode.Should().NotBeNull();
        userCode.ClientId.Should().Be(_testResult.ValidatedRequest.Client.ClientId);
        userCode.Lifetime.Should().Be(_testResult.ValidatedRequest.Client.DeviceCodeLifetime);
        userCode.CreationTime.Should().Be(creationTime);
        userCode.Subject.Should().BeNull();
        userCode.AuthorizedScopes.Should().BeNull();

        userCode.RequestedScopes.Should().Contain(_testResult.ValidatedRequest.RequestedScopes);
    }

    [Fact]
    public async Task ProcessAsync_when_generated_expect_device_code_stored()
    {
        var creationTime = DateTime.UtcNow;
        _clock.UtcNowFunc = () => creationTime;

        var response = await _generator.ProcessAsync(_testResult, TestBaseUrl);

        response.DeviceCode.Should().NotBeNullOrWhiteSpace();
        response.Interval.Should().Be(_options.DeviceFlow.Interval);
            
        var deviceCode = await _deviceFlowCodeService.FindByDeviceCodeAsync(response.DeviceCode);
        deviceCode.Should().NotBeNull();
        deviceCode.ClientId.Should().Be(_testResult.ValidatedRequest.Client.ClientId);
        deviceCode.IsOpenId.Should().Be(_testResult.ValidatedRequest.IsOpenIdRequest);
        deviceCode.Lifetime.Should().Be(_testResult.ValidatedRequest.Client.DeviceCodeLifetime);
        deviceCode.CreationTime.Should().Be(creationTime);
        deviceCode.Subject.Should().BeNull();
        deviceCode.AuthorizedScopes.Should().BeNull();
            
        response.DeviceCodeLifetime.Should().Be(deviceCode.Lifetime);
    }

    [Fact]
    public async Task ProcessAsync_when_DeviceVerificationUrl_is_relative_uri_expect_correct_VerificationUris()
    {
        const string baseUrl = "http://localhost:5000/";
        _options.UserInteraction.DeviceVerificationUrl = "/device";
        _options.UserInteraction.DeviceVerificationUserCodeParameter = "userCode";

        var response = await _generator.ProcessAsync(_testResult, baseUrl);

        response.VerificationUri.Should().Be("http://localhost:5000/device");
        response.VerificationUriComplete.Should().StartWith("http://localhost:5000/device?userCode=");
    }

    [Fact]
    public async Task ProcessAsync_when_DeviceVerificationUrl_is_absolute_uri_expect_correct_VerificationUris()
    {
        const string baseUrl = "http://localhost:5000/";
        _options.UserInteraction.DeviceVerificationUrl = "http://short/device";
        _options.UserInteraction.DeviceVerificationUserCodeParameter = "userCode";

        var response = await _generator.ProcessAsync(_testResult, baseUrl);

        response.VerificationUri.Should().Be("http://short/device");
        response.VerificationUriComplete.Should().StartWith("http://short/device?userCode=");
    }
}

internal class FakeUserCodeGenerator : IUserCodeGenerator
{
    public const string UserCodeTypeValue = "Collider";
    public const string TestUniqueUserCode = "123";
    public const string TestCollisionUserCode = "321";
    private int tryCount;


    public string UserCodeType => UserCodeTypeValue;

    public int RetryLimit { get; set; } = 2;

    public Task<string> GenerateAsync()
    {
        if (tryCount == 0)
        {
            tryCount++;
            return Task.FromResult(TestCollisionUserCode);
        }

        tryCount++;
        return Task.FromResult(TestUniqueUserCode);
    }
}