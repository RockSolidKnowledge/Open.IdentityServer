using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Configuration.DependencyInjection;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;

#nullable enable
namespace Open.IdentityServer.Validation;

internal class AuthorizeUsingPushedAuthorizationRequestValidator(
    Decorator<IAuthorizeRequestValidator> toDecorate,
    IdentityServerOptions options,
    IPushedAuthorizationRequestStore store)
    : IAuthorizeRequestValidator
{
    public async Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal? subject = null)
    {
        string[]? requestUris = parameters.GetValues(OidcConstants.AuthorizeRequest.RequestUri);
        
        if (requestUris == null || 
            requestUris[0].StartsWith(IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix) == false)
        {
            return await ValidateNonParRequest(parameters, subject);
        }

        if (requestUris.Length > 1)
        {
            return new AuthorizeRequestValidationResult(new ValidatedAuthorizeRequest(), "Too many request Uris",
                "Only one request uri is allowed");
        }
        
        PushedAuthorizationMemento? request = await store.ConsumePushedAuthorizationRequestAsync(requestUris[0]);
        if (request == null)
        {
            return new AuthorizeRequestValidationResult(OidcConstants.AuthorizeErrors.InvalidRequest);
        }

        if (request.Parameters.Get(OidcConstants.AuthorizeRequest.ClientId) !=
            parameters.Get(OidcConstants.AuthorizeRequest.ClientId))
        {
            return new AuthorizeRequestValidationResult(OidcConstants.AuthorizeErrors.InvalidRequest);
        }
        
        AuthorizeRequestValidationResult result = await toDecorate.Instance.ValidateAsync(request.Parameters, subject);
        
        return result;
    }
    

    private async Task<AuthorizeRequestValidationResult> ValidateNonParRequest(NameValueCollection parameters, ClaimsPrincipal? subject)
    {
        AuthorizeRequestValidationResult result = await toDecorate.Instance.ValidateAsync(parameters, subject);
        if (result.ValidatedRequest?.Client?.RequirePushedAuthorization == true || options.PushedAuthorization.Required)
        {
            return new AuthorizeRequestValidationResult(result.ValidatedRequest, "PAR required",
                "Client is configured for PAR only");
        }

        return result;
    }
}