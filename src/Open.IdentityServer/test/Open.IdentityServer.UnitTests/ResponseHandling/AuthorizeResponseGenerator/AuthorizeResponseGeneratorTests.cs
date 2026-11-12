// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.AuthorizeResponseGenerator;

public abstract class AuthorizeResponseGeneratorTests
{
    protected FakeTimeProvider clock = new FakeTimeProvider();
    protected ITokenService tokenService = Mock.Of<ITokenService>();
    protected IKeyMaterialService keyMaterialService = Mock.Of<IKeyMaterialService>();
    protected IAuthorizationCodeStore authorizationCodeStore = Mock.Of<IAuthorizationCodeStore>();
    protected ILogger<Open.IdentityServer.ResponseHandling.AuthorizeResponseGenerator> logger = NullLogger<Open.IdentityServer.ResponseHandling.AuthorizeResponseGenerator>.Instance;
    protected IEventService events = Mock.Of<IEventService>();
    protected ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    protected ITrace trace = Mock.Of<ITrace>();
    
    protected Open.IdentityServer.ResponseHandling.AuthorizeResponseGenerator CreateSut() => 
        new(clock, tokenService, keyMaterialService, authorizationCodeStore, logger, events, telemetry);
    
    protected static ValidatedAuthorizeRequest CreateValidatedAuthorizeRequest(string grantType) => new()
    {
        GrantType = grantType,
        Client = new Client
        {
            ClientId = "client",
            AuthorizationCodeLifetime = 300,
            AllowedIdentityTokenSigningAlgorithms = []
        },
        Subject = new ClaimsPrincipal(new ClaimsIdentity()),
        ValidatedResources = new ResourceValidationResult(),
        SessionId = "session",
        RedirectUri = "https://example.com/callback",
        CodeChallenge = "challenge",
        CodeChallengeMethod = "S256",
        Raw = new NameValueCollection()
    };

    protected static ValidatedAuthorizeRequest CreateHybridRequest(string responseType) => new()
    {
        GrantType = GrantType.Hybrid,
        ResponseType = responseType,
        Client = new Client
        {
            ClientId = "client",
            AuthorizationCodeLifetime = 300,
            AllowedIdentityTokenSigningAlgorithms = []
        },
        Subject = new ClaimsPrincipal(new ClaimsIdentity()),
        ValidatedResources = new ResourceValidationResult(),
        SessionId = "session",
        RedirectUri = "https://example.com/callback",
        CodeChallenge = "challenge",
        CodeChallengeMethod = "S256",
        Raw = new NameValueCollection()
    };

    protected static ValidatedAuthorizeRequest CreateImplicitRequest(string responseType) => new()
    {
        GrantType = GrantType.Implicit,
        ResponseType = responseType,
        Client = new Client
        {
            ClientId = "client",
            AllowedIdentityTokenSigningAlgorithms = []
        },
        Subject = new ClaimsPrincipal(new ClaimsIdentity()),
        ValidatedResources = new ResourceValidationResult(),
        Raw = new NameValueCollection()
    };
}