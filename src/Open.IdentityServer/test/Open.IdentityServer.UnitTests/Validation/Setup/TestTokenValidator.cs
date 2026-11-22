// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using System.Threading.Tasks;

namespace Open.IdentityServer.UnitTests.Validation.Setup;

class TestTokenValidator : ITokenValidator
{
    private readonly TokenValidationResult _result;

    public TestTokenValidator(TokenValidationResult result)
    {
        _result = result;
    }

    public Task<TokenValidationResult> ValidateAccessTokenAsync(AccessTokenValidationContext validationContext)
    {
        return Task.FromResult(_result);
    }

    public Task<TokenValidationResult> ValidateIdentityTokenAsync(IdentityTokenValidationContext validationContext)
    {
        return Task.FromResult(_result);
    }

    public Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client = null)
    {
        return Task.FromResult(_result);
    }
}