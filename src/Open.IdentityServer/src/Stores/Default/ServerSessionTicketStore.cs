using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Stores;

/// <summary>
/// 
/// </summary>
/// <param name="dataProtectionProvider"></param>
/// <param name="logger"></param>
public class ServerSessionTicketStore(
    IDataProtectionProvider dataProtectionProvider,
    ILogger<ServerSessionTicketStore> logger): ITicketStore
{
    private IDataProtector dataProtector = dataProtectionProvider?.CreateProtector(DataProtectionConstants.ServerSideTicketStorePurpose);

    /// <inheritdoc />
    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key)
    {
        throw new System.NotImplementedException();
    }
}