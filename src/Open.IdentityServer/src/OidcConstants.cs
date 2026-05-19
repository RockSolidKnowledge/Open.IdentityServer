// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer;

/// <summary>
/// Constants for OpenID Connect and OAuth 2.0 protocols.
/// </summary>
public static class OidcConstants
{
    /// <summary>
    /// Authorization endpoint request parameters as defined in the OpenID Connect Core specification.
    /// </summary>
    public static class AuthorizeRequest
    {
        /// <summary>Scope of the access request. REQUIRED. OpenID Connect requests MUST contain the "openid" scope value.</summary>
        public const string Scope = "scope";
        /// <summary>Response type which the client uses to obtain the authorization grant. REQUIRED.</summary>
        public const string ResponseType = "response_type";
        /// <summary>Client identifier issued to the client during the registration process. REQUIRED.</summary>
        public const string ClientId = "client_id";
        /// <summary>Redirection endpoint to which the response will be sent. REQUIRED.</summary>
        public const string RedirectUri = "redirect_uri";
        /// <summary>Opaque value used to maintain state between the request and the callback. RECOMMENDED.</summary>
        public const string State = "state";
        /// <summary>Response mode that determines how the response is returned. OPTIONAL.</summary>
        public const string ResponseMode = "response_mode";
        /// <summary>String value used to associate a Client session with an ID Token, and to mitigate replay attacks. OPTIONAL.</summary>
        public const string Nonce = "nonce";
        /// <summary>Display mode that the Authorization Server uses to display the authentication and consent pages. OPTIONAL.</summary>
        public const string Display = "display";
        /// <summary>Space-delimited list of strings that specifies whether the Authorization Server prompts the user. OPTIONAL.</summary>
        public const string Prompt = "prompt";
        /// <summary>Maximum Authentication Age, specifying the maximum allowable elapsed time in seconds since the user last performed active authentication. OPTIONAL.</summary>
        public const string MaxAge = "max_age";
        /// <summary>BCP47 language tag for the End-User's preferred language(s) for the user interface. OPTIONAL.</summary>
        public const string UiLocales = "ui_locales";
        /// <summary>ID Token, previously issued by the Authorization Server, as a hint about the End-User for whom the client is attempting to obtain authorization. OPTIONAL.</summary>
        public const string IdTokenHint = "id_token_hint";
        /// <summary>Hint to the Authorization Server about the login identifier that the End-User might use for authentication. OPTIONAL.</summary>
        public const string LoginHint = "login_hint";
        /// <summary>Requested Authentication Context Class Reference values, a space-separated list. OPTIONAL.</summary>
        public const string AcrValues = "acr_values";
        /// <summary>Code challenge value created by the client for PKCE (Proof Key for Public Clients). RECOMMENDED for public clients.</summary>
        public const string CodeChallenge = "code_challenge";
        /// <summary>Code challenge method used by the client for PKCE, typically "plain" or "S256". RECOMMENDED for public clients.</summary>
        public const string CodeChallengeMethod = "code_challenge_method";
        /// <summary>Signed JWT containing the request parameters for request object submission. OPTIONAL.</summary>
        public const string Request = "request";
        /// <summary>URL that references a request object containing the authorization request parameters. OPTIONAL.</summary>
        public const string RequestUri = "request_uri";
        /// <summary>Resource indicator. Indicates the resource server URL(s) for which the access token is being requested. OPTIONAL.</summary>
        public const string Resource = "resource";
        /// <summary>DPoP (Demonstration of Proof-of-Possession) key thumbprint. OPTIONAL for DPoP-bound access tokens.</summary>
        public const string DPoPKeyThumbprint = "dpop_jkt";
    }

    /// <summary>
    /// Authorization endpoint error codes as defined in the OpenID Connect Core and OAuth 2.0 specifications.
    /// </summary>
    public static class AuthorizeErrors
    {
        // OAuth2 errors
        /// <summary>The request is missing a required parameter, includes an invalid parameter value, includes a parameter more than once, or is otherwise malformed.</summary>
        public const string InvalidRequest = "invalid_request";
        /// <summary>The client is not authorized to request an authorization code using this method.</summary>
        public const string UnauthorizedClient = "unauthorized_client";
        /// <summary>The resource owner or authorization server denied the request.</summary>
        public const string AccessDenied = "access_denied";
        /// <summary>The authorization server does not support obtaining an authorization code using this method.</summary>
        public const string UnsupportedResponseType = "unsupported_response_type";
        /// <summary>The requested scope is invalid, unknown, or malformed.</summary>
        public const string InvalidScope = "invalid_scope";
        /// <summary>The authorization server encountered an unexpected condition that prevented it from fulfilling the request.</summary>
        public const string ServerError = "server_error";
        /// <summary>The authorization server is currently unable to handle the request due to temporary overloading or maintenance of the server.</summary>
        public const string TemporarilyUnavailable = "temporarily_unavailable";
        /// <summary>The End-User authentication does not meet the requirements specified by the Authorization Server.</summary>
        public const string UnmetAuthenticationRequirements = "unmet_authentication_requirements";

        // OIDC errors
        /// <summary>The Authorization Server requires End-User interaction of some form to proceed. This error MAY be returned when the prompt parameter value in the Authentication Request is none, but the Authentication Request cannot be completed without displaying a user interface for End-User interaction.</summary>
        public const string InteractionRequired = "interaction_required";
        /// <summary>The Authorization Server requires End-User authentication. This error is returned when the prompt parameter value in the Authentication Request is none, but the Authentication Request cannot be completed without displaying a user interface for End-User authentication.</summary>
        public const string LoginRequired = "login_required";
        /// <summary>The Authorization Server requires End-User account selection. This error is returned when the prompt parameter value in the Authentication Request is none, but the Authentication Request cannot be completed without displaying a user interface to prompt for End-User account selection.</summary>
        public const string AccountSelectionRequired = "account_selection_required";
        /// <summary>The Authorization Server requires End-User consent. This error is returned when the prompt parameter value in the Authentication Request is none, but the Authentication Request cannot be completed without displaying a user interface for End-User consent.</summary>
        public const string ConsentRequired = "consent_required";
        /// <summary>The request_uri in the Authorization Request returns an error or contains invalid data.</summary>
        public const string InvalidRequestUri = "invalid_request_uri";
        /// <summary>The request parameter contains an invalid Request Object.</summary>
        public const string InvalidRequestObject = "invalid_request_object";
        /// <summary>The Authorization Server does not support the use of the request parameter.</summary>
        public const string RequestNotSupported = "request_not_supported";
        /// <summary>The Authorization Server does not support the use of the request_uri parameter.</summary>
        public const string RequestUriNotSupported = "request_uri_not_supported";
        /// <summary>The Authorization Server does not support dynamic client registration.</summary>
        public const string RegistrationNotSupported = "registration_not_supported";

        // resource indicator spec
        /// <summary>The target resource is invalid, does not exist, or the Authorization Server does not support the resource indicator for the client.</summary>
        public const string InvalidTarget = "invalid_target";
    }

    /// <summary>
    /// Authorization endpoint response parameters.
    /// </summary>
    public static class AuthorizeResponse
    {
        /// <summary>The scopes that were granted in the authorization response.</summary>
        public const string Scope = "scope";
        /// <summary>An authorization code that can be exchanged for tokens.</summary>
        public const string Code = "code";
        /// <summary>An access token for the resource server.</summary>
        public const string AccessToken = "access_token";
        /// <summary>The lifetime in seconds of the access token.</summary>
        public const string ExpiresIn = "expires_in";
        /// <summary>The type of the returned token.</summary>
        public const string TokenType = "token_type";
        /// <summary>A refresh token that can be used to obtain a new access token.</summary>
        public const string RefreshToken = "refresh_token";
        /// <summary>An ID Token that contains claims about the authentication of an End-User.</summary>
        public const string IdentityToken = "id_token";
        /// <summary>The state value from the request.</summary>
        public const string State = "state";
        /// <summary>Session state for checking session status at the RP.</summary>
        public const string SessionState = "session_state";
        /// <summary>The issuer identifier.</summary>
        public const string Issuer = "iss";
        /// <summary>An error code indicating that an error has occurred.</summary>
        public const string Error = "error";
        /// <summary>A human-readable description of the error.</summary>
        public const string ErrorDescription = "error_description";
    }

    /// <summary>
    /// Device Authorization endpoint response parameters.
    /// </summary>
    public static class DeviceAuthorizationResponse
    {
        /// <summary>A unique code for the device.</summary>
        public const string DeviceCode = "device_code";
        /// <summary>A user code for the End-User to enter on the verification endpoint.</summary>
        public const string UserCode = "user_code";
        /// <summary>The verification URI where the user enters the user code.</summary>
        public const string VerificationUri = "verification_uri";
        /// <summary>The complete verification URI including the user code.</summary>
        public const string VerificationUriComplete = "verification_uri_complete";
        /// <summary>The lifetime in seconds of the device_code and user_code.</summary>
        public const string ExpiresIn = "expires_in";
        /// <summary>The minimum interval in seconds that the client MUST wait between polling requests.</summary>
        public const string Interval = "interval";
    }

    /// <summary>
    /// End Session endpoint request parameters.
    /// </summary>
    public static class EndSessionRequest
    {
        /// <summary>A previously issued ID Token as a hint about the End-User for whom the logout is being requested.</summary>
        public const string IdTokenHint = "id_token_hint";
        /// <summary>The URL to which the End-User's User Agent will be redirected after the RP has performed logout.</summary>
        public const string PostLogoutRedirectUri = "post_logout_redirect_uri";
        /// <summary>An opaque value that the RP can use to maintain state between the logout request and the callback.</summary>
        public const string State = "state";
        /// <summary>Session ID from the ID Token, used to logout from a specific session.</summary>
        public const string Sid = "sid";
        /// <summary>The issuer identifier for validation purposes.</summary>
        public const string Issuer = "iss";
        /// <summary>BCP47 language tag for the End-User's preferred language for the logout interface.</summary>
        public const string UiLocales = "ui_locales";
    }

    /// <summary>
    /// Token endpoint request parameters.
    /// </summary>
    public static class TokenRequest
    {
        /// <summary>The authorization grant type being used.</summary>
        public const string GrantType = "grant_type";
        /// <summary>The authorization code received from the authorization server.</summary>
        public const string Code = "code";
        /// <summary>The redirect URI used in the authorization request.</summary>
        public const string RedirectUri = "redirect_uri";
        /// <summary>The client identifier issued during registration.</summary>
        public const string ClientId = "client_id";
        /// <summary>The client secret.</summary>
        public const string ClientSecret = "client_secret";
        /// <summary>A JWT assertion for client authentication.</summary>
        public const string ClientAssertion = "client_assertion";
        /// <summary>The type of client assertion.</summary>
        public const string ClientAssertionType = "client_assertion_type";
        /// <summary>An assertion for the resource owner password credentials grant or token exchange.</summary>
        public const string Assertion = "assertion";
        /// <summary>The refresh token to exchange for a new access token.</summary>
        public const string RefreshToken = "refresh_token";
        /// <summary>The scope of the access request.</summary>
        public const string Scope = "scope";
        /// <summary>The resource owner's username.</summary>
        public const string UserName = "username";
        /// <summary>The resource owner's password.</summary>
        public const string Password = "password";
        /// <summary>The code verifier for PKCE.</summary>
        public const string CodeVerifier = "code_verifier";
        /// <summary>The type of token being requested.</summary>
        public const string TokenType = "token_type";
        /// <summary>The algorithm used for token signing.</summary>
        public const string Algorithm = "alg";
        /// <summary>The cryptographic key used for token validation.</summary>
        public const string Key = "key";
        /// <summary>The device code for the device authorization grant.</summary>
        public const string DeviceCode = "device_code";

        // token exchange
        /// <summary>The resource server URL(s) for which the access token is being requested.</summary>
        public const string Resource = "resource";
        /// <summary>The target audience for the access token.</summary>
        public const string Audience = "audience";
        /// <summary>The type of token being requested in a token exchange.</summary>
        public const string RequestedTokenType = "requested_token_type";
        /// <summary>The subject token for token exchange.</summary>
        public const string SubjectToken = "subject_token";
        /// <summary>The type of the subject token in a token exchange.</summary>
        public const string SubjectTokenType = "subject_token_type";
        /// <summary>The actor token for delegation in a token exchange.</summary>
        public const string ActorToken = "actor_token";
        /// <summary>The type of the actor token in a token exchange.</summary>
        public const string ActorTokenType = "actor_token_type";
        
        // ciba
        /// <summary>The authentication request ID returned from a backchannel authentication request.</summary>
        public const string AuthenticationRequestId = "auth_req_id";
    }

    /// <summary>
    /// Backchannel Authentication endpoint request parameters.
    /// </summary>
    public static class BackchannelAuthenticationRequest
    {
        /// <summary>The scope of the access request.</summary>
        public const string Scope = "scope";
        /// <summary>A token that the client uses to push token notifications back to the token endpoint.</summary>
        public const string ClientNotificationToken = "client_notification_token";
        /// <summary>Requested Authentication Context Class Reference values.</summary>
        public const string AcrValues = "acr_values";
        /// <summary>A hint about the login identifier the user should use for authentication.</summary>
        public const string LoginHintToken = "login_hint_token";
        /// <summary>An ID Token as a hint about the user to authenticate.</summary>
        public const string IdTokenHint = "id_token_hint";
        /// <summary>A hint about the login identifier the user should use for authentication.</summary>
        public const string LoginHint = "login_hint";
        /// <summary>A human-readable message shown to the user during the authentication process.</summary>
        public const string BindingMessage = "binding_message";
        /// <summary>A user code that the user should enter to authorize the request.</summary>
        public const string UserCode = "user_code";
        /// <summary>The requested expiration time in seconds for the authentication request.</summary>
        public const string RequestedExpiry = "requested_expiry";
        /// <summary>A signed JWT containing the request parameters.</summary>
        public const string Request = "request";
        /// <summary>The resource server URL(s) for which the access token is being requested.</summary>
        public const string Resource = "resource";
        /// <summary>DPoP key thumbprint for DPoP-bound access tokens.</summary>
        public const string DPoPKeyThumbprint = "dpop_jkt";
    }

    /// <summary>
    /// Backchannel Authentication request error codes.
    /// </summary>
    public static class BackchannelAuthenticationRequestErrors
    {
        /// <summary>The request object is invalid.</summary>
        public const string InvalidRequestObject = "invalid_request_object";
        /// <summary>The request is missing a required parameter or contains an invalid parameter.</summary>
        public const string InvalidRequest = "invalid_request";
        /// <summary>The requested scope is invalid, unknown, or malformed.</summary>
        public const string InvalidScope = "invalid_scope";
        /// <summary>The login hint token provided has expired.</summary>
        public const string ExpiredLoginHintToken = "expired_login_hint_token";
        /// <summary>The specified user is unknown to the authorization server.</summary>
        public const string UnknownUserId = "unknown_user_id";
        /// <summary>The client is not authorized to make backchannel authentication requests.</summary>
        public const string UnauthorizedClient = "unauthorized_client";
        /// <summary>The user code is missing from the request when required.</summary>
        public const string MissingUserCode = "missing_user_code";
        /// <summary>The user code provided is invalid.</summary>
        public const string InvalidUserCode = "invalid_user_code";
        /// <summary>The binding message is invalid.</summary>
        public const string InvalidBindingMessage = "invalid_binding_message";
        /// <summary>The client authentication failed.</summary>
        public const string InvalidClient = "invalid_client";
        /// <summary>The user denied the authentication request.</summary>
        public const string AccessDenied = "access_denied";
        /// <summary>The target resource is invalid or not supported.</summary>
        public const string InvalidTarget = "invalid_target";
    }

    /// <summary>
    /// Token request types.
    /// </summary>
    public static class TokenRequestTypes
    {
        /// <summary>Bearer token type.</summary>
        public const string Bearer = "bearer";
        /// <summary>Proof-of-Possession token type.</summary>
        public const string Pop = "pop";
    }

    /// <summary>
    /// Token endpoint error codes.
    /// </summary>
    public static class TokenErrors
    {
        /// <summary>The request is missing a required parameter, or otherwise malformed.</summary>
        public const string InvalidRequest = "invalid_request";
        /// <summary>Client authentication failed.</summary>
        public const string InvalidClient = "invalid_client";
        /// <summary>The provided authorization code, refresh token, or other grant is invalid, expired, or revoked.</summary>
        public const string InvalidGrant = "invalid_grant";
        /// <summary>The client is not authorized to use the requested grant type.</summary>
        public const string UnauthorizedClient = "unauthorized_client";
        /// <summary>The requested grant type is not supported by the authorization server.</summary>
        public const string UnsupportedGrantType = "unsupported_grant_type";
        /// <summary>The requested response type is not supported by the authorization server.</summary>
        public const string UnsupportedResponseType = "unsupported_response_type";
        /// <summary>The requested scope is invalid, unknown, or malformed.</summary>
        public const string InvalidScope = "invalid_scope";
        /// <summary>The authorization request is still pending.</summary>
        public const string AuthorizationPending = "authorization_pending";
        /// <summary>The user denied the authorization request.</summary>
        public const string AccessDenied = "access_denied";
        /// <summary>The client is making too many requests.</summary>
        public const string SlowDown = "slow_down";
        /// <summary>The authorization code or device code has expired.</summary>
        public const string ExpiredToken = "expired_token";
        /// <summary>The target resource is invalid or does not exist.</summary>
        public const string InvalidTarget = "invalid_target";
        /// <summary>The DPoP proof is invalid or malformed.</summary>
        public const string InvalidDPoPProof = "invalid_dpop_proof";
        /// <summary>The server requires a DPoP proof in the request.</summary>
        public const string UseDPoPNonce = "use_dpop_nonce";
    }

    /// <summary>
    /// Token endpoint response parameters.
    /// </summary>
    public static class TokenResponse
    {
        /// <summary>The access token issued by the authorization server.</summary>
        public const string AccessToken = "access_token";
        /// <summary>The lifetime in seconds of the access token.</summary>
        public const string ExpiresIn = "expires_in";
        /// <summary>The type of the token.</summary>
        public const string TokenType = "token_type";
        /// <summary>A refresh token that can be used to obtain a new access token when the current one expires.</summary>
        public const string RefreshToken = "refresh_token";
        /// <summary>An ID Token containing claims about the authentication of the End-User.</summary>
        public const string IdentityToken = "id_token";
        /// <summary>An error code indicating that an error has occurred.</summary>
        public const string Error = "error";
        /// <summary>A human-readable description of the error.</summary>
        public const string ErrorDescription = "error_description";
        /// <summary>The "Bearer" token type.</summary>
        public const string BearerTokenType = "Bearer";
        /// <summary>The "DPoP" token type for Demonstration of Proof-of-Possession.</summary>
        public const string DPoPTokenType = "DPoP";
        /// <summary>The type of the issued token in a token exchange request.</summary>
        public const string IssuedTokenType = "issued_token_type";
        /// <summary>The scope of the access token.</summary>
        public const string Scope = "scope";
    }

    /// <summary>
    /// Backchannel Authentication endpoint response parameters.
    /// </summary>
    public static class BackchannelAuthenticationResponse
    {
        /// <summary>A unique identifier for this authentication request.</summary>
        public const string AuthenticationRequestId = "auth_req_id";
        /// <summary>The lifetime in seconds of the authentication request.</summary>
        public const string ExpiresIn = "expires_in";
        /// <summary>The minimum interval in seconds that the client MUST wait between polling requests.</summary>
        public const string Interval = "interval";
    }

    /// <summary>
    /// Token Introspection endpoint request parameters.
    /// </summary>
    public static class TokenIntrospectionRequest
    {
        /// <summary>The token to introspect.</summary>
        public const string Token = "token";
        /// <summary>A hint about the type of the token submitted.</summary>
        public const string TokenTypeHint = "token_type_hint";
    }

    /// <summary>
    /// Dynamic Client Registration response parameters.
    /// </summary>
    public static class RegistrationResponse
    {
        /// <summary>An error code if registration failed.</summary>
        public const string Error = "error";
        /// <summary>A human-readable description of the registration error.</summary>
        public const string ErrorDescription = "error_description";
        /// <summary>The unique client identifier issued by the authorization server.</summary>
        public const string ClientId = "client_id";
        /// <summary>The client secret issued by the authorization server for confidential clients.</summary>
        public const string ClientSecret = "client_secret";
        /// <summary>A registration access token that can be used to retrieve or update the client's registration information.</summary>
        public const string RegistrationAccessToken = "registration_access_token";
        /// <summary>The URI of the client configuration endpoint where the client can retrieve its registration information.</summary>
        public const string RegistrationClientUri = "registration_client_uri";
        /// <summary>The timestamp when the client_id was issued.</summary>
        public const string ClientIdIssuedAt = "client_id_issued_at";
        /// <summary>The timestamp when the client_secret will expire, or 0 if it does not expire.</summary>
        public const string ClientSecretExpiresAt = "client_secret_expires_at";
        /// <summary>A signed JWT containing client metadata provided by the software publisher.</summary>
        public const string SoftwareStatement = "software_statement";
    }

    /// <summary>
    /// Client metadata parameters for dynamic client registration.
    /// </summary>
    public static class ClientMetadata
    {
        /// <summary>Array of redirection URIs that can be used as callbacks to the client.</summary>
        public const string RedirectUris = "redirect_uris";
        /// <summary>Array of response types that the client is declaring that it uses.</summary>
        public const string ResponseTypes = "response_types";
        /// <summary>Array of grant types that the client is declaring that it uses.</summary>
        public const string GrantTypes = "grant_types";
        /// <summary>The application type, either "web" or "native".</summary>
        public const string ApplicationType = "application_type";
        /// <summary>Array of email addresses for people responsible for this client.</summary>
        public const string Contacts = "contacts";
        /// <summary>A human-readable name of the client.</summary>
        public const string ClientName = "client_name";
        /// <summary>A URL that points to a logo image for the client.</summary>
        public const string LogoUri = "logo_uri";
        /// <summary>A URL of a web page containing information about the client.</summary>
        public const string ClientUri = "client_uri";
        /// <summary>A URL that points to the client's policy for use of the user's data.</summary>
        public const string PolicyUri = "policy_uri";
        /// <summary>A URL that points to the client's terms of service.</summary>
        public const string TosUri = "tos_uri";
        /// <summary>A URL that returns the client's JSON Web Key Set.</summary>
        public const string JwksUri = "jwks_uri";
        /// <summary>The client's JSON Web Key Set containing the keys used by the client.</summary>
        public const string Jwks = "jwks";
        /// <summary>A URL using the https scheme to be used in calculating pseudonymous identifiers by the OP.</summary>
        public const string SectorIdentifierUri = "sector_identifier_uri";
        /// <summary>The scope of the access request.</summary>
        public const string Scope = "scope";
        /// <summary>Array of post-logout redirect URIs that are pre-registered with the OP for use in logout requests.</summary>
        public const string PostLogoutRedirectUris = "post_logout_redirect_uris";
        /// <summary>A URI that the RP can register with the OP to be notified when a user logs out.</summary>
        public const string FrontChannelLogoutUri = "frontchannel_logout_uri";
        /// <summary>Boolean indicating whether the OP should include the session ID in the logout notification.</summary>
        public const string FrontChannelLogoutSessionRequired = "frontchannel_logout_session_required";
        /// <summary>A URI that the OP will call via an API call to notify the RP of logout events.</summary>
        public const string BackchannelLogoutUri = "backchannel_logout_uri";
        /// <summary>Boolean indicating whether the OP should include the session ID when sending logout notifications.</summary>
        public const string BackchannelLogoutSessionRequired = "backchannel_logout_session_required";
        /// <summary>A software identifier provided by the software publisher.</summary>
        public const string SoftwareId = "software_id";
        /// <summary>A signed JWT containing metadata about the software published by the software publisher.</summary>
        public const string SoftwareStatement = "software_statement";
        /// <summary>The version of the software.</summary>
        public const string SoftwareVersion = "software_version";
        /// <summary>The subject type of the client, either "public" or "pairwise".</summary>
        public const string SubjectType = "subject_type";
        /// <summary>The method the client will use to authenticate at the token endpoint.</summary>
        public const string TokenEndpointAuthenticationMethod = "token_endpoint_auth_method";
        /// <summary>The JWS algorithm that the client uses to sign requests to the token endpoint.</summary>
        public const string TokenEndpointAuthenticationSigningAlgorithm = "token_endpoint_auth_signing_alg";
        /// <summary>Default Maximum Authentication Age.</summary>
        public const string DefaultMaxAge = "default_max_age";
        /// <summary>Boolean indicating if auth_time claim in the ID Token is REQUIRED.</summary>
        public const string RequireAuthenticationTime = "require_auth_time";
        /// <summary>Default requested Authentication Context Class Reference values.</summary>
        public const string DefaultAcrValues = "default_acr_values";
        /// <summary>A URL the RP uses to initiate a login at the OP.</summary>
        public const string InitiateLoginUri = "initiate_login_uri";
        /// <summary>Array of request_uri values that are pre-registered with the OP for use in requests.</summary>
        public const string RequestUris = "request_uris";
        /// <summary>The JWS algorithm that the OP uses to sign ID Tokens to this client.</summary>
        public const string IdentityTokenSignedResponseAlgorithm = "id_token_signed_response_alg";
        /// <summary>The JWE encryption algorithm that the OP uses to encrypt ID Tokens to this client.</summary>
        public const string IdentityTokenEncryptedResponseAlgorithm = "id_token_encrypted_response_alg";
        /// <summary>The JWE encryption algorithm that the OP uses to encrypt the ciphertext of ID Tokens to this client.</summary>
        public const string IdentityTokenEncryptedResponseEncryption = "id_token_encrypted_response_enc";
        /// <summary>The JWS algorithm that the OP uses to sign the UserInfo response to this client.</summary>
        public const string UserinfoSignedResponseAlgorithm = "userinfo_signed_response_alg";
        /// <summary>The JWE encryption algorithm that the OP uses to encrypt the UserInfo response to this client.</summary>
        public const string UserInfoEncryptedResponseAlgorithm = "userinfo_encrypted_response_alg";
        /// <summary>The JWE encryption algorithm that the OP uses to encrypt the ciphertext of the UserInfo response to this client.</summary>
        public const string UserinfoEncryptedResponseEncryption = "userinfo_encrypted_response_enc";
        /// <summary>The JWS algorithm that the OP uses to sign request objects that are passed to the OP.</summary>
        public const string RequestObjectSigningAlgorithm = "request_object_signing_alg";
        /// <summary>The JWE encryption algorithm that the OP uses to encrypt request objects sent by the client.</summary>
        public const string RequestObjectEncryptionAlgorithm = "request_object_encryption_alg";
        /// <summary>The JWE encryption algorithm that the OP uses to encrypt the ciphertext of request objects sent by the client.</summary>
        public const string RequestObjectEncryptionEncryption = "request_object_encryption_enc";
        /// <summary>Boolean indicating if request objects from this client are REQUIRED to be signed.</summary>
        public const string RequireSignedRequestObject = "require_signed_request_object";
        /// <summary>Boolean indicating if all access tokens issued to this client MUST be bound to a DPoP proof.</summary>
        public const string AlwaysUseDPoPBoundAccessTokens = "dpop_bound_access_tokens";
    }

    /// <summary>
    /// Token types.
    /// </summary>
    public static class TokenTypes
    {
        /// <summary>Access token for accessing protected resources.</summary>
        public const string AccessToken = "access_token";
        /// <summary>ID Token containing claims about the End-User's authentication.</summary>
        public const string IdentityToken = "id_token";
        /// <summary>Refresh token for obtaining new access tokens.</summary>
        public const string RefreshToken = "refresh_token";
    }

    /// <summary>
    /// Token type identifiers for token exchange.
    /// </summary>
    public static class TokenTypeIdentifiers
    {
        /// <summary>OAuth 2.0 access token.</summary>
        public const string AccessToken = "urn:ietf:params:oauth:token-type:access_token";
        /// <summary>OAuth 2.0 ID Token from OpenID Connect.</summary>
        public const string IdentityToken = "urn:ietf:params:oauth:token-type:id_token";
        /// <summary>OAuth 2.0 refresh token.</summary>
        public const string RefreshToken = "urn:ietf:params:oauth:token-type:refresh_token";
        /// <summary>SAML 1.1 assertion.</summary>
        public const string Saml11 = "urn:ietf:params:oauth:token-type:saml1";
        /// <summary>SAML 2.0 assertion.</summary>
        public const string Saml2 = "urn:ietf:params:oauth:token-type:saml2";
        /// <summary>JSON Web Token (JWT).</summary>
        public const string Jwt = "urn:ietf:params:oauth:token-type:jwt";
    }

    /// <summary>
    /// Authentication schemes for sending access tokens.
    /// </summary>
    public static class AuthenticationSchemes
    {
        /// <summary>Bearer token sent in the Authorization header.</summary>
        public const string AuthorizationHeaderBearer = "Bearer";
        /// <summary>DPoP token sent in the Authorization header.</summary>
        public const string AuthorizationHeaderDPoP = "DPoP";
        
        /// <summary>Bearer token sent in the form body.</summary>
        public const string FormPostBearer = "access_token";
        /// <summary>Bearer token sent as a query parameter.</summary>
        public const string QueryStringBearer = "access_token";

        /// <summary>Proof-of-Possession token sent in the Authorization header.</summary>
        public const string AuthorizationHeaderPop = "PoP";
        /// <summary>Proof-of-Possession token sent in the form body.</summary>
        public const string FormPostPop = "pop_access_token";
        /// <summary>Proof-of-Possession token sent as a query parameter.</summary>
        public const string QueryStringPop = "pop_access_token";
    }

    /// <summary>
    /// Grant types.
    /// </summary>
    public static class GrantTypes
    {
        /// <summary>Resource Owner Password Credentials Grant.</summary>
        public const string Password = "password";
        /// <summary>Authorization Code Grant.</summary>
        public const string AuthorizationCode = "authorization_code";
        /// <summary>Client Credentials Grant.</summary>
        public const string ClientCredentials = "client_credentials";
        /// <summary>Refresh Token Grant.</summary>
        public const string RefreshToken = "refresh_token";
        /// <summary>Implicit Grant.</summary>
        public const string Implicit = "implicit";
        /// <summary>SAML 2.0 Bearer Token Grant.</summary>
        public const string Saml2Bearer = "urn:ietf:params:oauth:grant-type:saml2-bearer";
        /// <summary>JWT Bearer Token Grant.</summary>
        public const string JwtBearer = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        /// <summary>Device Code Grant.</summary>
        public const string DeviceCode = "urn:ietf:params:oauth:grant-type:device_code";
        /// <summary>Token Exchange Grant.</summary>
        public const string TokenExchange = "urn:ietf:params:oauth:grant-type:token-exchange";
        /// <summary>CIBA Grant.</summary>
        public const string Ciba = "urn:openid:params:grant-type:ciba";
    }

    /// <summary>
    /// Client assertion types for client authentication.
    /// </summary>
    public static class ClientAssertionTypes
    {
        /// <summary>JWT Bearer Assertion.</summary>
        public const string JwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        /// <summary>SAML Bearer Assertion.</summary>
        public const string SamlBearer = "urn:ietf:params:oauth:client-assertion-type:saml2-bearer";
    }

    /// <summary>
    /// Response types.
    /// </summary>
    public static class ResponseTypes
    {
        /// <summary>Authorization Code flow response type.</summary>
        public const string Code = "code";
        /// <summary>Implicit flow response type returning only an access token.</summary>
        public const string Token = "token";
        /// <summary>Implicit flow response type returning only an ID Token.</summary>
        public const string IdToken = "id_token";
        /// <summary>Implicit flow response type returning an ID Token and access token.</summary>
        public const string IdTokenToken = "id_token token";
        /// <summary>Hybrid flow response type returning authorization code and ID Token.</summary>
        public const string CodeIdToken = "code id_token";
        /// <summary>Hybrid flow response type returning authorization code and access token.</summary>
        public const string CodeToken = "code token";
        /// <summary>Hybrid flow response type returning authorization code, ID Token, and access token.</summary>
        public const string CodeIdTokenToken = "code id_token token";
    }

    /// <summary>
    /// Response modes.
    /// </summary>
    public static class ResponseModes
    {
        /// <summary>The response is sent as an HTML form with the response parameters.</summary>
        public const string FormPost = "form_post";
        /// <summary>The response is sent via the query string.</summary>
        public const string Query = "query";
        /// <summary>The response is sent via the fragment component of the URI.</summary>
        public const string Fragment = "fragment";
    }

    /// <summary>
    /// Display modes.
    /// </summary>
    public static class DisplayModes
    {
        /// <summary>Display the authentication and consent pages on a full user-agent window.</summary>
        public const string Page = "page";
        /// <summary>Display the authentication and consent pages in a popup window.</summary>
        public const string Popup = "popup";
        /// <summary>Display the authentication and consent pages that fit within an embedded window.</summary>
        public const string Touch = "touch";
        /// <summary>Display the authentication and consent pages that fit within a WAP device.</summary>
        public const string Wap = "wap";
    }

    /// <summary>
    /// Prompt modes.
    /// </summary>
    public static class PromptModes
    {
        /// <summary>Do not display any authentication or consent screens.</summary>
        public const string None = "none";
        /// <summary>Prompt the End-User for authentication.</summary>
        public const string Login = "login";
        /// <summary>Prompt the End-User for consent.</summary>
        public const string Consent = "consent";
        /// <summary>Prompt the End-User to select a user account.</summary>
        public const string SelectAccount = "select_account";
        /// <summary>Prompt the End-User to create a new account.</summary>
        public const string Create = "create";
    }

    /// <summary>
    /// Code challenge methods for PKCE.
    /// </summary>
    public static class CodeChallengeMethods
    {
        /// <summary>The code verifier is sent as-is to the token endpoint.</summary>
        public const string Plain = "plain";
        /// <summary>The code challenge is the URL-safe Base64URL encoding of the SHA256 hash of the code verifier.</summary>
        public const string Sha256 = "S256";
    }

    /// <summary>
    /// Protected Resource error codes.
    /// </summary>
    public static class ProtectedResourceErrors
    {
        /// <summary>The access token provided is expired, revoked, malformed, or invalid.</summary>
        public const string InvalidToken = "invalid_token";
        /// <summary>The access token provided has expired.</summary>
        public const string ExpiredToken = "expired_token";
        /// <summary>The request is missing a required parameter or is otherwise malformed.</summary>
        public const string InvalidRequest = "invalid_request";
        /// <summary>The request requires higher privileges than provided by the access token.</summary>
        public const string InsufficientScope = "insufficient_scope";
    }

    /// <summary>
    /// Token endpoint authentication methods.
    /// </summary>
    public static class EndpointAuthenticationMethods
    {
        /// <summary>Client secret is sent in the request body.</summary>
        public const string PostBody = "client_secret_post";
        /// <summary>Client ID and secret are sent using HTTP Basic Authentication.</summary>
        public const string BasicAuthentication = "client_secret_basic";
        /// <summary>Client uses a JWT signed with its private key for authentication.</summary>
        public const string PrivateKeyJwt = "private_key_jwt";
        /// <summary>Client uses OAuth 2.0 Mutual TLS Client Authentication.</summary>
        public const string TlsClientAuth = "tls_client_auth";
        /// <summary>Client uses Self-Signed Mutual TLS Client Authentication.</summary>
        public const string SelfSignedTlsClientAuth = "self_signed_tls_client_auth";
    }

    /// <summary>
    /// Authentication methods.
    /// </summary>
    public static class AuthenticationMethods
    {
        /// <summary>Facial Recognition.</summary>
        public const string FacialRecognition = "face";
        /// <summary>Fingerprint Biometric.</summary>
        public const string FingerprintBiometric = "fpt";
        /// <summary>Geolocation.</summary>
        public const string Geolocation = "geo";
        /// <summary>Proof-of-Possession of a Hardware-Secured Key.</summary>
        public const string ProofOfPossessionHardwareSecuredKey = "hwk";
        /// <summary>Iris Scan Biometric.</summary>
        public const string IrisScanBiometric = "iris";
        /// <summary>Knowledge-Based Authentication.</summary>
        public const string KnowledgeBasedAuthentication = "kba";
        /// <summary>Multiple Channel Authentication.</summary>
        public const string MultipleChannelAuthentication = "mca";
        /// <summary>Multi-Factor Authentication.</summary>
        public const string MultiFactorAuthentication = "mfa";
        /// <summary>One-Time Password.</summary>
        public const string OneTimePassword = "otp";
        /// <summary>Personal Identification or Pattern.</summary>
        public const string PersonalIdentificationOrPattern = "pin";
        /// <summary>Password-based authentication.</summary>
        public const string Password = "pwd";
        /// <summary>Risk-Based Authentication.</summary>
        public const string RiskBasedAuthentication = "rba";
        /// <summary>Retina Scan Biometric.</summary>
        public const string RetinaScanBiometric = "retina";
        /// <summary>Smart Card.</summary>
        public const string SmartCard = "sc";
        /// <summary>Confirmation by SMS.</summary>
        public const string ConfirmationBySms = "sms";
        /// <summary>Proof-of-Possession of a Software-Secured Key.</summary>
        public const string ProofOfPossessionSoftwareSecuredKey = "swk";
        /// <summary>Confirmation by Telephone.</summary>
        public const string ConfirmationByTelephone = "tel";
        /// <summary>User Presence Test.</summary>
        public const string UserPresenceTest = "user";
        /// <summary>Voice Biometric.</summary>
        public const string VoiceBiometric = "vbm";
        /// <summary>Windows Integrated Authentication.</summary>
        public const string WindowsIntegratedAuthentication = "wia";
    }

    /// <summary>
    /// Cryptographic algorithms.
    /// </summary>
    public static class Algorithms
    {
        /// <summary>No algorithm - the token is not signed.</summary>
        public const string None = "none";

        /// <summary>
        /// Symmetric signing algorithms.
        /// </summary>
        public static class Symmetric
        {
            /// <summary>HMAC with SHA-256.</summary>
            public const string HS256 = "HS256";
            /// <summary>HMAC with SHA-384.</summary>
            public const string HS384 = "HS284";
            /// <summary>HMAC with SHA-512.</summary>
            public const string HS512 = "HS512";
        }

        /// <summary>
        /// Asymmetric signing algorithms.
        /// </summary>
        public static class Asymmetric
        {
            /// <summary>RSASSA-PKCS1-v1_5 with SHA-256.</summary>
            public const string RS256 = "RS256";
            /// <summary>RSASSA-PKCS1-v1_5 with SHA-384.</summary>
            public const string RS384 = "RS384";
            /// <summary>RSASSA-PKCS1-v1_5 with SHA-512.</summary>
            public const string RS512 = "RS512";

            /// <summary>ECDSA using P-256 curve and SHA-256.</summary>
            public const string ES256 = "ES256";
            /// <summary>ECDSA using P-384 curve and SHA-384.</summary>
            public const string ES384 = "ES384";
            /// <summary>ECDSA using P-521 curve and SHA-512.</summary>
            public const string ES512 = "ES512";

            /// <summary>RSASSA-PSS with SHA-256.</summary>
            public const string PS256 = "PS256";
            /// <summary>RSASSA-PSS with SHA-384.</summary>
            public const string PS384 = "PS384";
            /// <summary>RSASSA-PSS with SHA-512.</summary>
            public const string PS512 = "PS512";

        }
    }

    /// <summary>
    /// OpenID Connect Discovery Metadata.
    /// </summary>
    public static class Discovery
    {
        /// <summary>The issuer identifier for the OpenID Provider.</summary>
        public const string Issuer = "issuer";

        // endpoints
        /// <summary>The Authorization Endpoint URL.</summary>
        public const string AuthorizationEndpoint = "authorization_endpoint";
        /// <summary>The Device Authorization Endpoint URL.</summary>
        public const string DeviceAuthorizationEndpoint = "device_authorization_endpoint";
        /// <summary>The Token Endpoint URL.</summary>
        public const string TokenEndpoint = "token_endpoint";
        /// <summary>The UserInfo Endpoint URL.</summary>
        public const string UserInfoEndpoint = "userinfo_endpoint";
        /// <summary>The Token Introspection Endpoint URL.</summary>
        public const string IntrospectionEndpoint = "introspection_endpoint";
        /// <summary>The Token Revocation Endpoint URL.</summary>
        public const string RevocationEndpoint = "revocation_endpoint";
        /// <summary>The OpenID Provider's Discovery Endpoint URL.</summary>
        public const string DiscoveryEndpoint = ".well-known/openid-configuration";
        /// <summary>The JSON Web Key Set Endpoint URL.</summary>
        public const string JwksUri = "jwks_uri";
        /// <summary>The End Session Endpoint URL.</summary>
        public const string EndSessionEndpoint = "end_session_endpoint";
        /// <summary>The OP is able to cause the RP to log in its End-User by using an HTML iframe.</summary>
        public const string CheckSessionIframe = "check_session_iframe";
        /// <summary>The Dynamic Client Registration Endpoint URL.</summary>
        public const string RegistrationEndpoint = "registration_endpoint";
        /// <summary>MTLS/mTLS endpoint aliases.</summary>
        public const string MtlsEndpointAliases = "mtls_endpoint_aliases";

        // common capabilities
        /// <summary>Boolean indicating if the OP supports Front-Channel Logout.</summary>
        public const string FrontChannelLogoutSupported = "frontchannel_logout_supported";
        /// <summary>Boolean indicating if the OP supports Front-Channel Logout with session information.</summary>
        public const string FrontChannelLogoutSessionSupported = "frontchannel_logout_session_supported";
        /// <summary>Boolean indicating if the OP supports Back-Channel Logout.</summary>
        public const string BackChannelLogoutSupported = "backchannel_logout_supported";
        /// <summary>Boolean indicating if the OP supports Back-Channel Logout with session information.</summary>
        public const string BackChannelLogoutSessionSupported = "backchannel_logout_session_supported";
        /// <summary>JSON array of grant types supported by the OP.</summary>
        public const string GrantTypesSupported = "grant_types_supported";
        /// <summary>JSON array of PKCE code challenge methods supported by the OP.</summary>
        public const string CodeChallengeMethodsSupported = "code_challenge_methods_supported";
        /// <summary>JSON array of scope values supported by the OP.</summary>
        public const string ScopesSupported = "scopes_supported";
        /// <summary>JSON array of subject identifier types supported by the OP.</summary>
        public const string SubjectTypesSupported = "subject_types_supported";
        /// <summary>JSON array of response modes supported by the OP.</summary>
        public const string ResponseModesSupported = "response_modes_supported";
        /// <summary>JSON array of response types supported by the OP.</summary>
        public const string ResponseTypesSupported = "response_types_supported";
        /// <summary>JSON array of claim names that the OP supports.</summary>
        public const string ClaimsSupported = "claims_supported";
        /// <summary>JSON array of token endpoint authentication methods supported by the OP.</summary>
        public const string TokenEndpointAuthenticationMethodsSupported = "token_endpoint_auth_methods_supported";

        // more capabilities
        /// <summary>JSON array of locale values supported by the OP for claim values.</summary>
        public const string ClaimsLocalesSupported = "claims_locales_supported";
        /// <summary>Boolean indicating if the claims request parameter is supported.</summary>
        public const string ClaimsParameterSupported = "claims_parameter_supported";
        /// <summary>JSON array of claim types supported by the OP.</summary>
        public const string ClaimTypesSupported = "claim_types_supported";
        /// <summary>JSON array of display parameter values supported by the OP.</summary>
        public const string DisplayValuesSupported = "display_values_supported";
        /// <summary>JSON array of Authentication Context Class Reference values supported by the OP.</summary>
        public const string AcrValuesSupported = "acr_values_supported";
        /// <summary>JSON array of JWE encryption algorithms supported by the OP for ID Tokens.</summary>
        public const string IdTokenEncryptionAlgorithmsSupported = "id_token_encryption_alg_values_supported";
        /// <summary>JSON array of JWE encryption content encryption algorithms supported by the OP for ID Tokens.</summary>
        public const string IdTokenEncryptionEncValuesSupported = "id_token_encryption_enc_values_supported";
        /// <summary>JSON array of JWS signing algorithms supported by the OP for ID Tokens.</summary>
        public const string IdTokenSigningAlgorithmsSupported = "id_token_signing_alg_values_supported";
        /// <summary>The OP's policy URI.</summary>
        public const string OpPolicyUri = "op_policy_uri";
        /// <summary>The OP's terms of service URI.</summary>
        public const string OpTosUri = "op_tos_uri";
        /// <summary>JSON array of JWE encryption algorithms supported by the OP for request objects.</summary>
        public const string RequestObjectEncryptionAlgorithmsSupported = "request_object_encryption_alg_values_supported";
        /// <summary>JSON array of JWE encryption content encryption algorithms supported by the OP for request objects.</summary>
        public const string RequestObjectEncryptionEncValuesSupported = "request_object_encryption_enc_values_supported";
        /// <summary>JSON array of JWS signing algorithms supported by the OP for request objects.</summary>
        public const string RequestObjectSigningAlgorithmsSupported = "request_object_signing_alg_values_supported";
        /// <summary>Boolean indicating if the request parameter is supported.</summary>
        public const string RequestParameterSupported = "request_parameter_supported";
        /// <summary>Boolean indicating if the request_uri parameter is supported.</summary>
        public const string RequestUriParameterSupported = "request_uri_parameter_supported";
        /// <summary>Boolean indicating if request_uri values must be pre-registered.</summary>
        public const string RequireRequestUriRegistration = "require_request_uri_registration";
        /// <summary>URL of the OP's service documentation.</summary>
        public const string ServiceDocumentation = "service_documentation";
        /// <summary>JSON array of JWS signing algorithms supported by the OP for token endpoint authentication.</summary>
        public const string TokenEndpointAuthSigningAlgorithmsSupported = "token_endpoint_auth_signing_alg_values_supported";
        /// <summary>JSON array of UI locale values supported by the OP.</summary>
        public const string UILocalesSupported = "ui_locales_supported";
        /// <summary>JSON array of JWE encryption algorithms supported by the OP for UserInfo responses.</summary>
        public const string UserInfoEncryptionAlgorithmsSupported = "userinfo_encryption_alg_values_supported";
        /// <summary>JSON array of JWE encryption content encryption algorithms supported by the OP for UserInfo responses.</summary>
        public const string UserInfoEncryptionEncValuesSupported = "userinfo_encryption_enc_values_supported";
        /// <summary>JSON array of JWS signing algorithms supported by the OP for UserInfo responses.</summary>
        public const string UserInfoSigningAlgorithmsSupported = "userinfo_signing_alg_values_supported";
        /// <summary>Boolean indicating if the OP supports TLS client certificate-bound access tokens.</summary>
        public const string TlsClientCertificateBoundAccessTokens = "tls_client_certificate_bound_access_tokens";
        /// <summary>Boolean indicating if the OP supports the iss parameter in the authorization response.</summary>
        public const string AuthorizationResponseIssParameterSupported = "authorization_response_iss_parameter_supported";
        /// <summary>JSON array of prompt parameter values supported by the OP.</summary>
        public const string PromptValuesSupported = "prompt_values_supported";

        // CIBA
        /// <summary>JSON array of backchannel token delivery modes supported by the OP.</summary>
        public const string BackchannelTokenDeliveryModesSupported = "backchannel_token_delivery_modes_supported";
        /// <summary>The backchannel authentication endpoint URI.</summary>
        public const string BackchannelAuthenticationEndpoint = "backchannel_authentication_endpoint";
        /// <summary>JSON array of JWS signing algorithms supported by the OP for backchannel authentication requests.</summary>
        public const string BackchannelAuthenticationRequestSigningAlgValuesSupported = "backchannel_authentication_request_signing_alg_values_supported";
        /// <summary>Boolean indicating if the user code parameter is supported in backchannel authentication requests.</summary>
        public const string BackchannelUserCodeParameterSupported = "backchannel_user_code_parameter_supported";
        
        // DPoP
        /// <summary>JSON array of JWS signing algorithms supported by the OP for DPoP proofs.</summary>
        public const string DPoPSigningAlgorithmsSupported = "dpop_signing_alg_values_supported";
    }

    /// <summary>
    /// Backchannel token delivery modes.
    /// </summary>
    public static class BackchannelTokenDeliveryModes
    {
        /// <summary>The client polls the token endpoint to obtain tokens.</summary>
        public const string Poll = "poll";
        /// <summary>The OP sends a ping notification to the client when the token is ready.</summary>
        public const string Ping = "ping";
        /// <summary>The OP pushes the token to the client via a direct call.</summary>
        public const string Push = "push";
    }

    /// <summary>
    /// Events.
    /// </summary>
    public static class Events
    {
        /// <summary>Backchannel logout event.</summary>
        public const string BackChannelLogout = "http://schemas.openid.net/event/backchannel-logout";
    }

    /// <summary>
    /// Backchannel logout request parameters.
    /// </summary>
    public static class BackChannelLogoutRequest
    {
        /// <summary>A logout token containing claims about the user being logged out.</summary>
        public const string LogoutToken = "logout_token";
    }

    /// <summary>
    /// Standard OpenID Connect scope values as defined in the OpenID Connect Core specification.
    /// </summary>
    public static class StandardScopes
    {
        /// <summary>REQUIRED. Informs the Authorization Server that the Client is making an OpenID Connect request. If the <c>openid</c> scope value is not present, the behavior is entirely unspecified.</summary>
        public const string OpenId = "openid";
        /// <summary>OPTIONAL. This scope value requests access to the End-User's default profile Claims, which are: <c>name</c>, <c>family_name</c>, <c>given_name</c>, <c>middle_name</c>, <c>nickname</c>, <c>preferred_username</c>, <c>profile</c>, <c>picture</c>, <c>website</c>, <c>gender</c>, <c>birthdate</c>, <c>zoneinfo</c>, <c>locale</c>, and <c>updated_at</c>.</summary>
        public const string Profile = "profile";
        /// <summary>OPTIONAL. This scope value requests access to the <c>email</c> and <c>email_verified</c> Claims.</summary>
        public const string Email = "email";
        /// <summary>OPTIONAL. This scope value requests access to the <c>address</c> Claim.</summary>
        public const string Address = "address";
        /// <summary>OPTIONAL. This scope value requests access to the <c>phone_number</c> and <c>phone_number_verified</c> Claims.</summary>
        public const string Phone = "phone";
        /// <summary>OPTIONAL. This scope value requests offline access to the End-User's information.</summary>
        public const string OfflineAccess = "offline_access";
    }
    
    /// <summary>
    /// HTTP header names used in OpenID Connect and OAuth 2.0 protocols.
    /// </summary>
    public static class HttpHeaders
    {
        /// <summary>The DPoP (Demonstration of Proof-of-Possession) header used for DPoP-bound access tokens.</summary>
        public const string DPoP = "DPoP";
        /// <summary>The DPoP-Nonce header used by the server to provide a nonce for DPoP proof binding.</summary>
        public const string DPoPNonce = "DPoP-Nonce";
    }
}