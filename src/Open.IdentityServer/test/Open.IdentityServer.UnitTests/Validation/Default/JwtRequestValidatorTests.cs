// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class JwtRequestValidatorTests
{
    private Mock<IHttpContextAccessor> _contextAccessor = new();
    private IdentityServerOptions _options = new IdentityServerOptions();
    private Mock<ITelemetryService> _telemetry = new();

    private JwtRequestValidator CreateSubject()
    {
        return new JwtRequestValidator(
            _contextAccessor.Object,
            _options,
            _telemetry.Object,
            TestLogger.Create<JwtRequestValidator>());
    }

    [Fact]
    public async Task ValidateAsync_WhenCalled_ShouldCreateTelemetryTrace()
    {
        Client client = new Client();
        string jwtString = "asdg";
        
        var subject = CreateSubject();
        
        await subject.ValidateAsync(client, jwtString);

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Validation, subject, "ValidateAsync"));
    }
}