// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Test;
using Microsoft.Extensions.Logging;

namespace IdentityServerHost.Extensions;

public class HostProfileService : TestUserProfileService
{
    public HostProfileService(TestUserStore users, ILogger<TestUserProfileService> logger) : base(users, logger)
    {
    }

    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        await base.GetProfileDataAsync(context);

        var transaction = context.RequestedResources.ParsedScopes.FirstOrDefault(x => x.ParsedName == "transaction");
        if (transaction?.ParsedParameter != null)
        {
            context.IssuedClaims.Add(new Claim("transaction_id", transaction.ParsedParameter));
        }
    }
}