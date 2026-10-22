using System.Linq;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Extensions.Mapping;

/// <summary>
/// <see cref="AuthenticationTicketFilterResult"/> mapping extension methods
/// </summary>
public static class AuthenticationTicketFilterResultMappingExtensions
{
    extension(ServerSideSession result)
    {
        /// <summary>
        /// Maps object to an instance of the <see cref="UserSession"/> model
        /// </summary>
        /// <returns>new <see cref="UserSession"/> object</returns>
        public UserSession ToUserSession()
        {
            string issuer = null;
        
            result.AuthenticationTicket?.Properties.Items.TryGetValue(JwtClaimTypes.Issuer, out issuer);
        
            return new UserSession
            {
                SubjectId = result.SubjectId,
                SessionId = result.SessionId,
                DisplayName = result.DisplayName,
                Created = result.Created,
                Renewed = result.Renewed,
                Expires = result.Expires,
                Issuer = issuer,
                ClientIds = result.AuthenticationTicket?.Properties.GetClientList().ToList() ?? [],
                AuthenticationTicket = result.AuthenticationTicket,
            };
        }
    }
}