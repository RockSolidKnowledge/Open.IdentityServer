using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Logic for token revocation
/// </summary>
public interface ITokenRevocationService
{
    /// <summary>
    /// Revokes a grant family starting with an auth code. Will revoke any revocable token linked to provided auth code.
    /// </summary>
    /// <param name="authorizationCode">auth code for which to revoke all token</param>
    public Task RevokeGrantFamily(AuthorizationCode authorizationCode);
}