// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Models;

/// <summary>
/// Container for the session model and deserialized auth ticket from the <see cref="IServerSessionTicketStore"/> FilterServerAuthenticationTickets method
/// </summary>
public class AuthenticationTicketFilterResult
{
    /// <summary>
    /// Session model returned from filtering
    /// </summary>
    public IdentityServerServerSideSessions Session { get; set; } = null!;
    
    /// <summary>
    /// AuthenticationTicket deserialized from the data property on the session entity
    /// </summary>
    public AuthenticationTicket? AuthTicket { get; set; }

    /// <summary>
    /// Maps object to an instance of the <see cref="UserSession"/> model
    /// </summary>
    /// <returns>new <see cref="UserSession"/> object</returns>
    public UserSession ToUserSession()
    {
        return new UserSession
        {
            SubjectId = Session.SubjectId,
            SessionId = Session.SessionId,
            DisplayName = Session.DisplayName,
            Created = Session.Created,
            Renewed = Session.Renewed,
            Expires = Session.Expires,
            ClientIds = AuthTicket?.Properties.GetClientList().ToList() ?? [],
            AuthenticationTicket = AuthTicket,
        };
    }
}