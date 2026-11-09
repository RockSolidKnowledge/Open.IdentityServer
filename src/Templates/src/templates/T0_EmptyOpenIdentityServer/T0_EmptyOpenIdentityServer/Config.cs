using Open.IdentityServer.Models;

namespace T0_EmptyOpenIdentityServer;

public static class Config
{
    public static IEnumerable<Client> Clients => [];

    public static IEnumerable<ApiScope> ApiScopes => [];
}