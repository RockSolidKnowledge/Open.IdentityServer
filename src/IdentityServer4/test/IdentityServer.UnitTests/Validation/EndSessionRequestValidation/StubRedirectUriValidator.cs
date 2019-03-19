// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.UnitTests.Validation.EndSessionRequestValidation
{
    public class StubRedirectUriValidator : IRedirectUriValidator
    {
        public bool IsRedirectUriValid { get; set; }
        public bool IsPostLogoutRedirectUriValid { get; set; }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(IsPostLogoutRedirectUriValid);
        }

        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(IsRedirectUriValid);
        }
    }
}
