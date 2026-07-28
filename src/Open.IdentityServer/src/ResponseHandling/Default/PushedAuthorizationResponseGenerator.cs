using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Services;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.ResponseHandling.Default;

#nullable enable

/// <summary>
/// Default implementation of the pushed authorization response generator
/// </summary>
/// <param name="store">The store used to save the pushed authorization request for later retrieval</param>
/// <param name="handleGenerationService">The service used to generate the ID for the stored state</param>
/// <param name="logger">The logger to record errors and debug inforation</param>
public class PushedAuthorizationResponseGenerator(IPushedAuthorizationRequestStore store, 
                                                  IHandleGenerationService handleGenerationService,
                                                  ILogger<PushedAuthorizationResponseGenerator> logger) : IPushedAuthorizationResponseGenerator
{
    /// <summary>
    /// Standard prefix for the generated URI for a Pushed Authorization Request
    /// </summary>
    public static readonly string PushedAuthorizationRequestPrefix = "urn:ietf:params:oauth:request_uri:";
    
    /// <summary>
    /// Default lifetime for a Pushed Authorization Request
    /// </summary>
    public static readonly int DefaultRequestLifetimeInSeconds = 60;
    
    /// <summary>
    /// Generates the Pushed Authorization Request response
    /// </summary>
    /// <param name="request">The request for which to generate a response</param>
    /// <returns>The generated response</returns>
    public async Task<PushedAuthorizationResponse?> CreateResponseAsync(ValidatedAuthorizeRequest request)
    {
        PushedAuthorizationStoredInformation storeInfo = MapRequestInformation(request);
        
        string generatedUniquePart = await handleGenerationService.GenerateAsync();
        
        string id = PushedAuthorizationRequestPrefix + generatedUniquePart;

        try
        {
            await store.StorePushedAuthorizationRequestAsync(id, storeInfo);

            return new PushedAuthorizationResponse(new Uri(id), DefaultRequestLifetimeInSeconds);
        }
        catch (Exception e)
        {
            return null;
        }
       
    }

    private PushedAuthorizationStoredInformation MapRequestInformation(ValidatedAuthorizeRequest request)
    {
        return new PushedAuthorizationStoredInformation
        {
            AccessTokenLifetime = request.AccessTokenLifetime,
            ClientId = request.ClientId,
            ClientSecretVerified = request.Secret != null, // If the secret is passed then it will already have been validated
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            RedirectUri = request.RedirectUri,
            RequestedScopes = request.RequestedScopes,
            Subject = request.Subject,
            Confirmation = request.Confirmation,
            Description = request.Description,
            DisplayMode = request.DisplayMode,
            GrantType = request.GrantType,
            IsApiResourceRequest = request.IsApiResourceRequest,
            IsOpenIdRequest = request.IsOpenIdRequest,
            LoginHint = request.LoginHint,
            MaxAge = request.MaxAge,
            Nonce = request.Nonce,
            ResponseMode = request.ResponseMode,
            ResponseType = request.ResponseType,
            State = request.State,
            UiLocales = request.UiLocales,
            SessionId = request.SessionId,
            WasConsentShown = request.WasConsentShown,
            ValidatedResources = request.ValidatedResources.Resources,
            RequestedResourceIndicators = request.RequestedResourceIndicators,
            AuthenticationContextReferenceClasses = request.AuthenticationContextReferenceClasses,
            PromptModes = request.PromptModes,
            RequestObjectValues = request.RequestObjectValues,
        };
    }
}