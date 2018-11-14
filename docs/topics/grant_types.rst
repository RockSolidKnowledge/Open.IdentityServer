Grant Types
^^^^^^^^^^^

Grant types are a way to specify how a client wants to interact with IdentityServer.
The OpenID Connect and OAuth 2 specs define the following grant types:

* Implicit
* Authorization code
* Hybrid
* Client credentials
* Resource owner password
* Device flow
* Refresh tokens
* Extension grants

You can specify which grant type a client can use via the ``AllowedGrantTypes`` property on the ``Client`` configuration.

A client can be configured to use more than a single grant type (e.g. Hybrid for user centric operations and client credentials for server to server communication).
The ``GrantTypes`` class can be used to pick from typical grant type combinations::

    Client.AllowedGrantTypes = GrantTypes.HybridAndClientCredentials;

You can also specify the grant types list manually::

    Client.AllowedGrantTypes = 
    {
        GrantType.Hybrid, 
        GrantType.ClientCredentials,
        "my_custom_grant_type" 
    };

If you want to transmit access tokens via the browser channel, you also need to allow that explicitly on the client configuration::

    Client.AllowAccessTokensViaBrowser = true;

.. Note:: For security reasons, not all grant type combinations are allowed. See below for more details.

For the remainder, the grant types are briefly described, and when you would use them.
It is also recommended, that in addition you read the corresponding specs to get a better understanding of the differences.

Client credentials
==================
This is the simplest grant type and is used for server to server communication - tokens are always requested on behalf of a client, not a user.

With this grant type you send a token request to the token endpoint, and get an access token back that represents the client.
The client typically has to authenticate with the token endpoint using its client ID and secret.

See the :ref:`Client Credentials Quick Start <refClientCredentialsQuickstart>` for a sample how to use it. 


Resource owner password
=======================
The resource owner password grant type allows to request tokens on behalf of a user by sending the user's name and password to the token endpoint.
This is so called "non-interactive" authentication and is generally not recommended.

There might be reasons for certain legacy or first-party integration scenarios, where this grant type is useful, but the general recommendation
is to use an interactive flow like implicit or hybrid for user authentication instead.

See the :ref:`Resource Owner Password Quick Start <_refResourceOwnerQuickstart>` for a sample how to use it.
You also need to provide code for the username/password validation which can be supplied by implementing the ``IResourceOwnerPasswordValidator`` interface.
You can find more information about this interface :ref:`here <refResourceOwnerPasswordValidator>`. 

Implicit
========
The implicit grant type is optimized for browser-based applications. Either for user authentication-only (both server-side and JavaScript applications),
or authentication and access token requests (JavaScript applications).

In the implicit flow, all tokens are transmitted via the browser, and advanced features like refresh tokens are thus not allowed.

:ref:`This <refImplicitQuickstart>` quickstart shows authentication for service-side web apps, and 
:ref:`this <refJavaScriptQuickstart>` shows JavaScript.

Authorization code
==================
Authorization code flow was originally specified by OAuth 2, and provides a way to retrieve tokens on a back-channel as opposed to the browser front-channel.
It also support client authentication.

While this grant type is supported on its own, it is generally recommended you combine that with identity tokens
which turns it into the so called hybrid flow.
Hybrid flow gives you important extra features like signed protocol responses.

Hybrid
======
Hybrid flow is a combination of the implicit and authorization code flow - it uses combinations of multiple grant types, most typically ``code id_token``.

In hybrid flow the identity token is transmitted via the browser channel and contains the signed protocol response along with signatures for other artifacts
like the authorization code. This mitigates a number of attacks that apply to the browser channel.
After successful validation of the response, the back-channel is used to retrieve the access and refresh token.

This is the recommended flow for native applications that want to retrieve access tokens (and possibly refresh tokens as well) and is used
for server-side web applications and native desktop/mobile applications.

See :ref:`this <refHybridQuickstart>` quickstart for more information about using hybrid flow with MVC. 

Device flow
===========
Device flow is designed for browserless and input constrained devices, where the device is unable to securely capture user credentials. This flow outsources user authentication and consent to an external device (e.g. a smart phone).

This flow is typically used by IoT devices and can request both identity and API resources.

Refresh tokens
==============
Refresh tokens allow gaining long lived access to APIs.

You typically want to keep the lifetime of access tokens as short as possible, but at the same time don't want to bother the user
over and over again with doing a front-channel roundtrips to IdentityServer for requesting new ones.

Refresh tokens allow requesting new access tokens without user interaction. Every time the client refreshes a token it needs to make an 
(authenticated) back-channel call to IdentityServer. This allows checking if the refresh token is still valid, or has been revoked in the meantime.

Refresh tokens are supported in hybrid, authorization code, device flow and resource owner password flows. 
To request a refresh token, the client needs to include the ``offline_access`` scope in the token request (and must be authorized to request for that scope). 

Extension grants
================
Extension grants allow extending the token endpoint with new grant types. See :ref:`this <refExtensionGrants>` for more details. 

Incompatible grant types
========================
Some grant type combinations are forbidden:

* Mixing implicit and authorization code or hybrid would allow a downgrade attack from the more secure code based flow to implicit.
* Same concern exists for allowing both authorization code and hybrid
