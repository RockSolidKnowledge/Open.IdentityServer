using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Extension to <see cref="ITicketStore"/> to add more methods of retrival
/// </summary>
public interface IServerSessionTicketStore: ITicketStore
{
    /// <summary>
    /// Filters auth tickets stored server side using the provided filters
    /// </summary>
    /// <param name="subjectId">subject id filter to apply</param>
    /// <param name="sessionId">session id filter to apply</param>
    /// <returns>collection of auth ticket matching filter</returns>
    Task<IEnumerable<AuthenticationTicketFilterResult>> FilterServerAuthenticationTickets(string subjectId, string sessionId);
}