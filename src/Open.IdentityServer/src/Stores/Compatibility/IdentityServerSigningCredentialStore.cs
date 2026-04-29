using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.DataProtection;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Duende compatibility signing key store
/// </summary>
/// <param name="identityServerKeyStore/>
public class IdentityServerSigningCredentialStore(
    IIdentityServerKeyStore identityServerKeyStore,
    DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter): ISigningCredentialStore
{
    /// <summary>
    /// Gets the key to be used for signing from the key store, will be the newest key 
    /// </summary>
    /// <returns>a key to be used for signing</returns>
    public async Task<SigningCredentials> GetSigningCredentialsAsync()
    {
        return identityServerKeyStore.GetKeys()
            .Where(x => x.Use == "signing")
            .Select(dataProtectedIdentityServerKeyMaterialConverter.Convert)
            .OrderByDescending(x => x.Created)
            .Select(x => x.Credentials)
            .First();
    }
}