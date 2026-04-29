// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common;

class MockKeyMaterialService : IKeyMaterialService
{
    public List<SigningCredentials> SigningCredentials = new List<SigningCredentials>();
    public List<SecurityKeyInfo> ValidationKeys = new List<SecurityKeyInfo>();

    public Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
    {
        return Task.FromResult(SigningCredentials.AsEnumerable());
    }

    public Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null)
    {
        return Task.FromResult(SigningCredentials.FirstOrDefault());
    }

    public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        return Task.FromResult(ValidationKeys.AsEnumerable());
    }
}