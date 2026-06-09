// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.UnitTests.ResponseHandling.AuthorizeResponseGenerator;

public abstract class AuthorizeResponseGeneratorTests
{
    protected FakeTimeProvider clock = new FakeTimeProvider();
    protected ITokenService tokenService = Mock.Of<ITokenService>();
    protected IKeyMaterialService keyMaterialService = Mock.Of<IKeyMaterialService>();
    protected IAuthorizationCodeStore authorizationCodeStore = Mock.Of<IAuthorizationCodeStore>();
    protected ILogger<Open.IdentityServer.ResponseHandling.AuthorizeResponseGenerator> logger = NullLogger<Open.IdentityServer.ResponseHandling.AuthorizeResponseGenerator>.Instance;
    protected IEventService events = Mock.Of<IEventService>();
    
    protected Open.IdentityServer.ResponseHandling.AuthorizeResponseGenerator CreateSut() => 
        new(clock, tokenService, keyMaterialService, authorizationCodeStore, logger, events);
}