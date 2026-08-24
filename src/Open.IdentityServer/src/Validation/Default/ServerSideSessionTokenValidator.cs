using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Validation;

/// <summary>
/// 
/// </summary>
/// <param name="decoratedService"></param>
/// <param name="ticketStore"></param>
public class DefaultServerSideSessionRefreshTokenService(
    IRefreshTokenService decoratedService,
    ITicketStore ticketStore): IRefreshTokenService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public async Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client)
    {
        var validatedRefreshToken = await decoratedService.ValidateRefreshTokenAsync(token, client);
        
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task<string> CreateRefreshTokenAsync(RefreshTokenCreationRequest request) => 
        decoratedService.CreateRefreshTokenAsync(request);

    /// <inheritdoc />
    public Task<string> UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, Client client) =>
        decoratedService.UpdateRefreshTokenAsync(handle, refreshToken, client);
}