// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.Extensions;

internal static class ClaimsPrincipleExtension
{
    extension(ClaimsPrincipal claimsPrincipal)
    {
        public ClaimsPrincipalLite ToSerializableObj()
        {
            return new ClaimsPrincipalLite
            {
                AuthenticationType = claimsPrincipal.Identity!.AuthenticationType!,
                Claims = claimsPrincipal.Claims.ToSerializableObj(),
            };
        }
    }
    
    extension(ClaimsPrincipalLite claimsPrincipalLite)
    {
        public ClaimsPrincipal ToClaimsPrincipal()
        {
            return new ClaimsPrincipal(new ClaimsIdentity(claimsPrincipalLite.Claims.ToClaims(), claimsPrincipalLite.AuthenticationType));
        }
    }
}