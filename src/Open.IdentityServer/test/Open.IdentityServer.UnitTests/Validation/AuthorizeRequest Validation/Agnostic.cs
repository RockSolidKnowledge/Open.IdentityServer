// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Agnostic
{
    [Fact]
    public async Task ValidateAsync_WhenCalled_ShouldCreateTelemetryTrace()
    {
        Mock<ITelemetryService> telemetry = new();
        
        var parameters = new NameValueCollection();

        var validator = Factory.CreateAuthorizeRequestValidator(telemetry: telemetry.Object);
        await validator.ValidateAsync(parameters);

        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Validation, validator, "ValidateAsync"));
    }
}