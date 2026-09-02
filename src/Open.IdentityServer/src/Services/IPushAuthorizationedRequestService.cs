using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;

namespace Open.IdentityServer.Services;
#nullable enable

/// <summary>
/// Represents a PAR response
/// </summary>
/// <param name="Key">The Identifier for the response</param>
/// <param name="ExpiresIn">The time for which the response is valid</param>
public record PushedAuthorization(Uri Key, TimeSpan ExpiresIn);

/// <summary>
/// Manages the creation and storage of a PAR request, along with the ability
/// to obtain the original parameters, to perform an AuthCode flow
/// </summary>
public interface IPushedAuthorizationRequestService
{
    /// <summary>
    /// Create a PAR response bound to the supplied parameters
    /// </summary>
    /// <param name="client">The client making the request</param>
    /// <param name="parameters">the parameters to store, and to be used for a subsequence AuthCode flow</param>
    /// <returns>An expiring response, used to obtain the parameters during an AuthCode flow</returns>
    Task<PushedAuthorization> CreateAsync(Client client,NameValueCollection parameters);
    
    /// <summary>
    /// Returns a NameValue collection associated with the key assuming it has not expired
    /// </summary>
    /// <param name="key">The Key returned as a part of a CreateResponse</param>
    /// <returns>The parameters associated with the key, or null if the response has expired or was never created </returns>
    Task<NameValueCollection?> ConsumeAsync(string key);
}