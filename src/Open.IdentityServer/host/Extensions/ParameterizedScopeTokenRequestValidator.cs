// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Open.IdentityServer.Validation;

namespace IdentityServerHost.Extensions;

public class ParameterizedScopeTokenRequestValidator : ICustomTokenRequestValidator
{
    public Task ValidateAsync(CustomTokenRequestValidationContext context)
    {
        var transaction = context.Result.ValidatedRequest.ValidatedResources.ParsedScopes.FirstOrDefault(x => x.ParsedName == "transaction");
        if (transaction?.ParsedParameter != null)
        {
            context.Result.ValidatedRequest.ClientClaims.Add(new Claim(transaction.ParsedName, transaction.ParsedParameter));
        }

        return Task.CompletedTask;
    }
}