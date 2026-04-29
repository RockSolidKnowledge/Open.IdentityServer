// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;

namespace IdentityServer.UnitTests.Validation.Setup;

public static class ValidationExtensions
{
    public static ClientSecretValidationResult ToValidationResult(this Client client, ParsedSecret secret = null)
    {
        return new ClientSecretValidationResult
        {
            Client = client,
            Secret = secret
        };
    }
}