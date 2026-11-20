// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.Validation.EndSessionRequestValidation;

public class StubRedirectUriValidator : IRedirectUriValidator
{
    public bool IsRedirectUriValid { get; set; }
    public bool IsPostLogoutRedirectUriValid { get; set; }

    public Task<bool> IsPostLogoutRedirectUriValidAsync(RedirectUriValidationContext validationContext)
    {
        return Task.FromResult(IsPostLogoutRedirectUriValid);
    }

    public Task<bool> IsRedirectUriValidAsync(RedirectUriValidationContext validationContext)
    {
        return Task.FromResult(IsRedirectUriValid);
    }
}