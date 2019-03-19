// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using System.Security.Claims;

namespace IdentityServer4.UnitTests.Common
{
    public class MockConsentService : IConsentService
    {
        public bool RequiresConsentResult { get; set; }

        public Task<bool> RequiresConsentAsync(ClaimsPrincipal subject, Client client, IEnumerable<string> scopes)
        {
            return Task.FromResult(RequiresConsentResult);
        }

        public ClaimsPrincipal ConsentSubject { get; set; }
        public Client ConsentClient { get; set; }
        public IEnumerable<string> ConsentScopes { get; set; }

        public Task UpdateConsentAsync(ClaimsPrincipal subject, Client client, IEnumerable<string> scopes)
        {
            ConsentSubject = subject;
            ConsentClient = client;
            ConsentScopes = scopes;

            return Task.CompletedTask;
        }
    }
}
