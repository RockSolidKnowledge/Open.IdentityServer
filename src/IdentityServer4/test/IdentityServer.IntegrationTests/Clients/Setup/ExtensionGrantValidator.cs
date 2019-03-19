// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class ExtensionGrantValidator : IExtensionGrantValidator
    {
        public Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var credential = context.Request.Raw.Get("custom_credential");

            if (credential != null)
            {
                // valid credential
                context.Result = new GrantValidationResult("818727", "custom");
            }
            else
            {
                // custom error message
                context.Result = new GrantValidationResult(Models.TokenRequestErrors.InvalidGrant, "invalid_custom_credential");
            }

            return Task.CompletedTask;
        }

        public string GrantType =>  "custom";
    }
}