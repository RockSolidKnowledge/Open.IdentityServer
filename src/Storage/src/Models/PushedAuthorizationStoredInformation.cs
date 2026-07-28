using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Storage.Models;

/// <summary>
/// Used to serialize the data necessary to store a pushed authorization request.
/// </summary>
public class PushedAuthorizationStoredInformation
{
    /// <summary>
    /// Gets or sets whether the secret used to authenticate the client has been correctly verified.
    /// False if no secret was supplied
    /// </summary>
    /// <value>
    /// The parsed secret.
    /// </value>
    public bool ClientSecretVerified { get; set; }

    /// <summary>
    /// Gets or sets the effective access token lifetime for the current request.
    /// This value is initally read from the client configuration but can be modified in the request pipeline
    /// </summary>
    public int AccessTokenLifetime { get; set; }

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The subject.
    /// </value>
    public ClaimsPrincipal Subject { get; set; }

    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    /// <value>
    /// The session identifier.
    /// </value>
    public string SessionId { get; set; }
    
    /// <summary>
    /// Gets or sets the validated resources for the request.
    /// </summary>
    /// <value>
    /// The validated resources.
    /// </value>
    public Resources ValidatedResources { get; set; }

    /// <summary>
    /// Gets or sets the value of the confirmation method (will become the cnf claim). Must be a JSON object.
    /// </summary>
    /// <value>
    /// The confirmation.
    /// </value>
    public string Confirmation { get; set; }

    /// <summary>
    /// Gets or sets the client ID that should be used for the current request (this is useful for token exchange scenarios)
    /// </summary>
    /// <value>
    /// The client ID
    /// </value>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the type of the response.
    /// </summary>
    /// <value>
    /// The type of the response.
    /// </value>
    public string ResponseType { get; set; }

    /// <summary>
    /// Gets or sets the response mode.
    /// </summary>
    /// <value>
    /// The response mode.
    /// </value>
    public string ResponseMode { get; set; }

    /// <summary>
    /// Gets or sets the grant type.
    /// </summary>
    /// <value>
    /// The grant type.
    /// </value>
    public string GrantType { get; set; }

    /// <summary>
    /// Gets or sets the redirect URI.
    /// </summary>
    /// <value>
    /// The redirect URI.
    /// </value>
    public string RedirectUri { get; set; }

    /// <summary>
    /// Gets or sets the requested scopes.
    /// </summary>
    /// <value>
    /// The requested scopes.
    /// </value>
    // todo: consider replacing with extension method to access Raw collection; would neeed to be done wholesale for all props.
    public List<string> RequestedScopes { get; set; }

    /// <summary>
    /// Gets or sets the requested resource indicators
    /// </summary>
    /// <value>
    /// The request resource indicators
    /// </value>
    public List<string> RequestedResourceIndicators { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether consent was shown.
    /// </summary>
    /// <value>
    ///   <c>true</c> if consent was shown; otherwise, <c>false</c>.
    /// </value>
    public bool WasConsentShown { get; set; }

    /// <summary>
    /// Gets the description the user assigned to the device being authorized.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    /// <value>
    /// The state.
    /// </value>
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the UI locales.
    /// </summary>
    /// <value>
    /// The UI locales.
    /// </value>
    public string UiLocales { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request was an OpenID Connect request.
    /// </summary>
    /// <value>
    /// <c>true</c> if the request was an OpenID Connect request; otherwise, <c>false</c>.
    /// </value>
    public bool IsOpenIdRequest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is API resource request.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is API resource request; otherwise, <c>false</c>.
    /// </value>
    public bool IsApiResourceRequest { get; set; }

    /// <summary>
    /// Gets or sets the nonce.
    /// </summary>
    /// <value>
    /// The nonce.
    /// </value>
    public string Nonce { get; set; }

    /// <summary>
    /// Gets or sets the authentication context reference classes.
    /// </summary>
    /// <value>
    /// The authentication context reference classes.
    /// </value>
    public List<string> AuthenticationContextReferenceClasses { get; set; }

    /// <summary>
    /// Gets or sets the display mode.
    /// </summary>
    /// <value>
    /// The display mode.
    /// </value>
    public string DisplayMode { get; set; }

    /// <summary>
    /// Gets or sets the collection of prompt modes.
    /// </summary>
    /// <value>
    /// The collection of prompt modes.
    /// </value>
    public IEnumerable<string> PromptModes { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the maximum age.
    /// </summary>
    /// <value>
    /// The maximum age.
    /// </value>
    public int? MaxAge { get; set; }

    /// <summary>
    /// Gets or sets the login hint.
    /// </summary>
    /// <value>
    /// The login hint.
    /// </value>
    public string LoginHint { get; set; }

    /// <summary>
    /// Gets or sets the code challenge
    /// </summary>
    /// <value>
    /// The code challenge
    /// </value>
    public string CodeChallenge { get; set; }

    /// <summary>
    /// Gets or sets the code challenge method
    /// </summary>
    /// <value>
    /// The code challenge method
    /// </value>
    public string CodeChallengeMethod { get; set; }

    /// <summary>
    /// Gets or sets the validated contents of the request object (if present)
    /// </summary>
    /// <value>
    /// The request object values
    /// </value>
    public Dictionary<string, string> RequestObjectValues { get; set; } = new ();

}