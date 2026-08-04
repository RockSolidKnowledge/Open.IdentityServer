// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.AuthorizeInteractionResponseGenerator;

public class AuthorizeInteractionResponseGeneratorTests_Create
{
    private readonly IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator _subject;
    private readonly MockConsentService _mockConsentService = new MockConsentService();
    private readonly StubClock _clock = new StubClock();
    private readonly Mock<ITelemetryService> _telemetry = new Mock<ITelemetryService>();

    public AuthorizeInteractionResponseGeneratorTests_Create()
    {
        _subject = new IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator(
            _clock,
            TestLogger.Create<IdentityServer.ResponseHandling.AuthorizeInteractionResponseGenerator>(),
            _mockConsentService,
            new MockProfileService(),
            _telemetry.Object);
    }

    [Fact]
    public async Task ProcessCreateAsync_PromptModeIsCreate_ReturnsCreateAccountResult()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback",
            PromptModes = [OidcConstants.PromptModes.Create]
        };

        var result = await _subject.ProcessCreateAsync(request);
        result.IsCreateAccount.Should().BeTrue();        
    }

    [Fact]
    public async Task ProcessCreateAsync_PromptModeIsNotCreate_ReturnsEmptyResult()
    {
        var request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            State = "12345",
            RedirectUri = "https://client.com/callback"
        };

        var result = await _subject.ProcessCreateAsync(request);
        result.IsCreateAccount.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessCreateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var act = () => _subject.ProcessCreateAsync(null);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

}
