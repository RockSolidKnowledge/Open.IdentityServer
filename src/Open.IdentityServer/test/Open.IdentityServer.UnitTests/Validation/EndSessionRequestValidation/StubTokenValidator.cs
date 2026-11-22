// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.Validation.EndSessionRequestValidation;

public class StubTokenValidator : ITokenValidator
{
    public TokenValidationResult AccessTokenValidationResult { get; set; } = new TokenValidationResult();
    public TokenValidationResult IdentityTokenValidationResult { get; set; } = new TokenValidationResult();

    public Task<TokenValidationResult> ValidateAccessTokenAsync(AccessTokenValidationContext validationContext)
    {
        return Task.FromResult(AccessTokenValidationResult);
    }

    public Task<TokenValidationResult> ValidateIdentityTokenAsync(IdentityTokenValidationContext validationContext)
    {
        return Task.FromResult(IdentityTokenValidationResult);
    }

    public Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client)
    {
        throw new System.NotImplementedException();
    }
}