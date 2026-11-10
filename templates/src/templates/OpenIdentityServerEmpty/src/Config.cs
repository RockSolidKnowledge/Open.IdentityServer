using Open.IdentityServer.Models;

namespace OpenIdentityServerEmpty;

public static class Config
{
    public static IEnumerable<Client> Clients => [];

    public static IEnumerable<ApiScope> ApiScopes => [];
}