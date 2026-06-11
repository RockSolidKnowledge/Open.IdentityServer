using System.Threading.Tasks;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.ResponseHandling;

/// <summary>
/// 
/// </summary>
public interface IPushedAuthorizationResponseGenerator
{
    /// <summary>
    /// Creates a response to the pushed authorization request, generating the Unique URI for the request.
    /// </summary>
    /// <param name="request">The validated authorization request</param>
    /// <returns>A response that can be returned to the client</returns>
    Task<PushedAuthorizationResponse> CreateResponseAsync(ValidatedAuthorizeRequest request);
}