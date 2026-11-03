using System.Collections.Generic;
using Open.IdentityServer.Models;

namespace IdentityServer.Models;

public class SessionsListViewModel
{
    public IReadOnlyCollection<UserSession> Sessions { get; init; } = [];
    public IReadOnlyCollection<string> Usernames { get; init; } = [];
    public string SelectedUsername { get; init; } = string.Empty;
}
