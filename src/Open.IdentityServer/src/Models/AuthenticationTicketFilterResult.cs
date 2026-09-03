// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using Microsoft.AspNetCore.Authentication;
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
}