// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Open.IdentityServer.UnitTests.Common;

class MockClaimsService : IClaimsService
{
    public List<Claim> IdentityTokenClaims { get; set; } = new List<Claim>();
    public List<Claim> AccessTokenClaims { get; set; } = new List<Claim>();

    public Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, bool includeAllIdentityClaims, ValidatedRequest request)
    {
        return Task.FromResult(IdentityTokenClaims.AsEnumerable());
    }

    public Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, ValidatedRequest request)
    {
        return Task.FromResult(AccessTokenClaims.AsEnumerable());
    }
}