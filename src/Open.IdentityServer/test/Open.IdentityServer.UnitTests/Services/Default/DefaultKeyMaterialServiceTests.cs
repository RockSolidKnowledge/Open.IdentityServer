// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultKeyMaterialServiceTests
{
    private Mock<ISigningCredentialStore> _signingCredentials;
    private Mock<IValidationKeysStore> _validationKeys;
    private Mock<ITelemetryService> _telemetry;

    public DefaultKeyMaterialServiceTests()
    {
        _signingCredentials = new Mock<ISigningCredentialStore>();
        _validationKeys = new Mock<IValidationKeysStore>();
        _telemetry = new Mock<ITelemetryService>();
    }

    private DefaultKeyMaterialService CreateSubject()
    {
        return new DefaultKeyMaterialService(
            [_validationKeys.Object],
            [_signingCredentials.Object],
            _telemetry.Object
        );
    }

    [Fact]
    public async Task GetValidationKeysAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.GetValidationKeysAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetValidationKeysAsync"));
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.GetSigningCredentialsAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetSigningCredentialsAsync"));
    }

    [Fact]
    public async Task GetAllSigningCredentialsASync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var subject = CreateSubject();
        
        await subject.GetAllSigningCredentialsAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetAllSigningCredentialsAsync"));
    }
}