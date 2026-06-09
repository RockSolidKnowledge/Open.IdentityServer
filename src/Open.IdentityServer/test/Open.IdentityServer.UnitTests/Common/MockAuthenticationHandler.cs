// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Open.IdentityServer.UnitTests.Common;

internal class MockAuthenticationHandler : IAuthenticationHandler
{
    public AuthenticateResult Result { get; set; } = AuthenticateResult.NoResult();

    public Task<AuthenticateResult> AuthenticateAsync()
    {
        return Task.FromResult(Result);
    }

    public Task ChallengeAsync(AuthenticationProperties properties)
    {
        return Task.CompletedTask;
    }

    public Task ForbidAsync(AuthenticationProperties properties)
    {
        return Task.CompletedTask;
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        return Task.CompletedTask;
    }
}