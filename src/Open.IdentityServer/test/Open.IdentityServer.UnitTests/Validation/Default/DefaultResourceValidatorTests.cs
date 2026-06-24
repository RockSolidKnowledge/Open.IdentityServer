// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class DefaultResourceValidatorTests
{
    private readonly Mock<IResourceStore> _resourceStore = new();
    private readonly Mock<IScopeParser> _scopeParser = new();
    private readonly Mock<ITelemetryService> _telemetry = new();
    
    private DefaultResourceValidator CreateSubject()
    {
        return new DefaultResourceValidator(
            _resourceStore.Object,
            _scopeParser.Object,
            _telemetry.Object,
            TestLogger.Create<DefaultResourceValidator>());
    }

    [Fact]
    public async Task ValidateRequestedResourcesAsync_WhenCalled_ShouldTelemetryTrace()
    {
        var request = new ResourceValidationRequest
        {
            Scopes = []
        };

        _scopeParser
            .Setup(x => x.ParseScopeValues(It.IsAny<IEnumerable<string>>()))
            .Returns(new ParsedScopesResult
            {
                ParsedScopes = [],
                Errors = []
            });

        _resourceStore
            .Setup(x => x.FindIdentityResourcesByScopeNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new IdentityResource()
                {
                    Name = "test"
                }
            ]);
        _resourceStore
            .Setup(x => x.FindApiResourcesByScopeNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new ApiResource()
                {
                    Name = "test2"
                }
            ]);
        _resourceStore
            .Setup(x => x.FindApiScopesByNameAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new ApiScope()
                {
                    Name = "test3"
                }
            ]);

        var subject = CreateSubject();

        await subject.ValidateRequestedResourcesAsync(request);

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Validation, subject, "ValidateRequestedResourcesAsync"));
    }
}