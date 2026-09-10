// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Validation;
#nullable enable

internal class AuthorizeRequestValidatorFactory
    (
        IAuthorizeRequestValidator authOnlyValidator,
        IdentityServerOptions options,
        IPushedAuthorizationRequestService parService
    ) : IAuthorizeRequestValidatorFactory
{
    
    public IAuthorizeRequestValidator Create()
    {
        return new AuthorizeUsingPushedAuthorizationRequestValidator(authOnlyValidator, options, parService);
    }
}