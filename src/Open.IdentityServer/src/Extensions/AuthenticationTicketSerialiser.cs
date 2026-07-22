// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.Extensions;

internal static class AuthenticationTicketExtensions
{
    extension(AuthenticationTicket authenticationTicket)
    {
        public SerializedAuthenticationTicket ToSerializableObj()
        {
            return new SerializedAuthenticationTicket
            {
                Scheme = authenticationTicket.AuthenticationScheme,
                User = authenticationTicket.Principal.ToSerializableObj(),
                Items = authenticationTicket.Properties.Items,
            };
        }
    }

    extension(SerializedAuthenticationTicket serializationAuthTicket)
    {
        public AuthenticationTicket ToAuthTicket()
        {
            return new AuthenticationTicket(
                serializationAuthTicket.User.ToClaimsPrincipal(), 
                new AuthenticationProperties(serializationAuthTicket.Items), 
                serializationAuthTicket.Scheme);
        }
    }
}