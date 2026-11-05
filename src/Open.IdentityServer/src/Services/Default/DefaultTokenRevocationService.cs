using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Services.Default;

/// <summary>
/// Default token revocation service
/// </summary>
public class DefaultTokenRevocationService(
    IAuthorizationCodeStore authorizationCodeStore,
    IReferenceTokenStore referenceTokenStore,
    IRefreshTokenStore refreshTokenStore,
    IPersistedGrantStore persistedGrantStore): ITokenRevocationService
{
    /// <inheritdoc />
    public async Task RevokeGrantFamily(string code, AuthorizationCode authorizationCode)
    {
        List<string> removalOperations = [code];

        if (!string.IsNullOrWhiteSpace(authorizationCode.ChildGrant))
        {
            var currentGrant = authorizationCode.ChildGrant;

            do
            {
                var refreshToken = await refreshTokenStore.GetRefreshTokenAsync(currentGrant);

                if (refreshToken != null)
                {
                    foreach (var (key, token) in refreshToken.AccessTokens)
                    {
                        if (token.AccessTokenType == AccessTokenType.Reference)
                        {
                            removalOperations.Add(key);
                        }
                    }
                }
                
                removalOperations.Add(currentGrant);

                currentGrant = refreshToken?.ChildGrant;
                
                //TODO: protect against infinite loop
            } while (!string.IsNullOrWhiteSpace(currentGrant));
        }

        await persistedGrantStore.RemoveAsync(removalOperations.ToArray());
    }
}