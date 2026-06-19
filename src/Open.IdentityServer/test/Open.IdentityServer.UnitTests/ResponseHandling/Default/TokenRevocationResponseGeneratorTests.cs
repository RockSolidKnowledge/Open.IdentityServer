// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.Default;

public class TokenRevocationResponseGeneratorTests
{
    private Mock<IReferenceTokenStore> _referenceTokenStore;
    private Mock<IRefreshTokenStore> _refreshTokenStore;
    private Mock<ITelemetryService> _telemetry;
    
    public TokenRevocationResponseGeneratorTests()
    {
        _referenceTokenStore = new Mock<IReferenceTokenStore>();
        _refreshTokenStore = new Mock<IRefreshTokenStore>();
        _telemetry = new Mock<ITelemetryService>();
    }

    private TokenRevocationResponseGenerator CreateSubject()
    {
        return new TokenRevocationResponseGenerator(
            _referenceTokenStore.Object,
            _refreshTokenStore.Object,
            _telemetry.Object,
            TestLogger.Create<TokenRevocationResponseGenerator>());
    }

    [Fact]
    public async Task ProcessAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        var validatedRequest = new TokenRevocationRequestValidationResult();
        
        var subject = CreateSubject();

        await subject.ProcessAsync(validatedRequest);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic,
            subject,
            "ProcessAsync"));
    }
}