using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Open.IdentityServer.Models;

public class UserSession
{
    public string SubjectId { get; set; } = default!;

    public string SessionId { get; set; } = default!;

    public string DisplayName { get; set; }

    public DateTime Created { get; set; }

    public DateTime Renewed { get; set; }

    public DateTime? Expires { get; set; }

    public IReadOnlyCollection<string> ClientIds { get; set; } = default!;

    public AuthenticationTicket AuthenticationTicket { get; set; } = default!;
}