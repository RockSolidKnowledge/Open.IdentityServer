using System.Linq;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Extensions.Mapping;

/// <summary>
/// <see cref="AuthenticationTicketFilterResult"/> mapping extension methods
/// </summary>
public static class AuthenticationTicketFilterResultMappingExtensions
{
    extension(AuthenticationTicketFilterResult result)
    {
        /// <summary>
        /// Maps object to an instance of the <see cref="UserSession"/> model
        /// </summary>
        /// <returns>new <see cref="UserSession"/> object</returns>
        public UserSession ToUserSession()
        {
            string? issuer = null;
        
            result.AuthTicket?.Properties.Items.TryGetValue(JwtClaimTypes.Issuer, out issuer);
        
            return new UserSession
            {
                SubjectId = result.Session.SubjectId,
                SessionId = result.Session.SessionId,
                DisplayName = result.Session.DisplayName,
                Created = result.Session.Created,
                Renewed = result.Session.Renewed,
                Expires = result.Session.Expires,
                Issuer = issuer,
                ClientIds = result.AuthTicket?.Properties.GetClientList().ToList() ?? [],
                AuthenticationTicket = result.AuthTicket,
            };
        }
    }
}