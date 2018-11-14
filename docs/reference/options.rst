.. _refOptions:
IdentityServer Options
======================

* ``IssuerUri``
    Set the issuer name that will appear in the discovery document and the issued JWT tokens.
    It is recommended to not set this property, which infers the issuer name from the host name that is used by the clients.

* ``PublicOrigin``
    The origin of this server instance, e.g. https://myorigin.com. If not set, the origin name is inferred from the request.

Endpoints
^^^^^^^^^
Allows enabling/disabling individual endpoints, e.g. token, authorize, userinfo etc.

By default all endpoints are enabled, but you can lock down your server by disabling endpoint that you don't need.

Discovery
^^^^^^^^^
Allows enabling/disabling various sections of the discovery document, e.g. endpoints, scopes, claims, grant types etc.

The ``CustomEntries`` dictionary allows adding custom elements to the discovery document.

Authentication
^^^^^^^^^^^^^^
* ``CookieLifetime``
    The authentication cookie lifetime (only effective if the IdentityServer-provided cookie handler is used).

* ``CookieSlidingExpiration``
    Specified if the cookie should be sliding or not (only effective if the IdentityServer-provided cookie handler is used).

* ``RequireAuthenticatedUserForSignOutMessage``
    Indicates if user must be authenticated to accept parameters to end session endpoint. Defaults to false.

* ``CheckSessionCookieName``
    The name of the cookie used for the check session endpoint.

* ``RequireCspFrameSrcForSignout``
    If set, will require frame-src CSP headers being emitting on the end session callback endpoint which renders iframes to clients for front-channel signout notification. Defaults to true.

Events
^^^^^^
Allows configuring if and which events should be submitted to a registered event sink. See :ref:`here <refEvents>` for more information on events.

InputLengthRestrictions
^^^^^^^^^^^^^^^^^^^^^^^
Allows setting length restrictions on various protocol parameters like client id, scope, redirect URI etc.

UserInteraction
^^^^^^^^^^^^^^^

* ``LoginUrl``, ``LogoutUrl``, ``ConsentUrl``, ``ErrorUrl``, ``DeviceVerificationUrl``
    Sets the the URLs for the login, logout, consent, error and device verification pages.
* ``LoginReturnUrlParameter``
    Sets the name of the return URL parameter passed to the login page. Defaults to *returnUrl*.
* ``LogoutIdParameter``
    Sets the name of the logout message id parameter passed to the logout page. Defaults to *logoutId*.
* ``ConsentReturnUrlParameter``
    Sets the name of the return URL parameter passed to the consent page. Defaults to *returnUrl*.
* ``ErrorIdParameter``
    Sets the name of the error message id parameter passed to the error page. Defaults to *errorId*.
* ``CustomRedirectReturnUrlParameter``
    Sets the name of the return URL parameter passed to a custom redirect from the authorization endpoint. Defaults to *returnUrl*.
* ``DeviceVerificationUserCodeParameter``
    Sets the name of the user code parameter passed to the device verification page. Defaults to *userCode*.
* ``CookieMessageThreshold``
    Certain interactions between IdentityServer and some UI pages require a cookie to pass state and context (any of the pages above that have a configurable "message id" parameter).
    Since browsers have limits on the number of cookies and their size, this setting is used to prevent too many cookies being created. 
    The value sets the maximum number of message cookies of any type that will be created.
    The oldest message cookies will be purged once the limit has been reached.
    This effectively indicates how many tabs can be opened by a user when using IdentityServer.

Caching
^^^^^^^
These setting only apply if the respective caching has been enabled in the services configuration in startup.

* ``ClientStoreExpiration``
    Cache duration of client configuration loaded from the client store.

* ``ResourceStoreExpiration``
    Cache duration of identity and API resource configuration loaded from the resource store.

CORS
^^^^
IdentityServer supports CORS for some of its endpoints.
The underlying CORS implementation is provided from ASP.NET Core, and as such it is automatically registered in the dependency injection system.

* ``CorsPolicyName``
    Name of the CORS policy that will be evaluated for CORS requests into IdentityServer (defaults to ``"IdentityServer4"``).
    The policy provider that handles this is implemented in terms of the ``ICorsPolicyService`` registered in the dependency injection system.
    If you wish to customize the set of CORS origins allowed to connect, then it is recommended that you provide a custom implementation of ``ICorsPolicyService``.

* ``CorsPaths``
    The endpoints within IdentityServer where CORS is supported. 
    Defaults to the discovery, user info, token, and revocation endpoints.

* ``PreflightCacheDuration``
    `Nullable<TimeSpan>` indicating the value to be used in the preflight `Access-Control-Max-Age` response header.
    Defaults to `null` indicating no caching header is set on the response.

CSP (Content Security Policy)
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
IdentityServer emits CSP headers for some responses, where appropriate.

* ``Level``
    The level of CSP to use. CSP Level 2 is used by default, but if older browsers must be supported then this be changed to ``CspLevel.One`` to accomodate them.

* ``AddDeprecatedHeader``
    Indicates if the older ``X-Content-Security-Policy`` CSP header should also be emitted (in addition to the standards-based header value). Defaults to true.

Device Flow
^^^^^^^^^^^

* ``DefaultUserCodeType``
    The user code type to use, unless set at the client level. Defaults to *Numeric*, a 9-digit code.
* ``Interval``
    Defines the minimum allowed polling interval on the token endpoint. Defaults to *5*.