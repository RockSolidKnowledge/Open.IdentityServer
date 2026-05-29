.. _refResourceIndicators:
Using Resource Indicators
=========================

By default Open.IdentityServer access tokens are generated with an **aud** claim that includes all API resources that a client is permitted to access based on the requested scope. This can create tokens that have broad access
and violate the principle of least privilege. Tokens generated to be used by one API resource could be used by other API resources that are within the requested scope and the client is permitted to access.

In Open.IdentityServer we have implemented Resource Indicators, as defined in `RFC 8707 <https://datatracker.ietf.org/doc/html/rfc8707>`_.

This feature allows a client application to specifically specify which resources it wants an access token for, clients do this by specifying a ``resource`` parameter on the authorize and token endpoints. This provides:

- **Reduced token privilege** — tokens are only valid for the intended API, limiting the exposure if a token is compromised.
- **Token isolation** — different APIs receive different tokens, preventing token replay across services.
- **Clearer intent** — the authorization server knows exactly which resource the client needs to access.

Configuration
-------------

To use resource indicators, your API resources must be configured with a unique identifier (must be a valid URI, typically the base URL of the API):

.. code-block:: csharp
    
    new ApiResource("https://api1.example.com")
    {
        Scopes = { "read", "write" }
    };

    new ApiResource("https://api2.example.com")
    {
        Scopes = { "reports" }
    };

Requesting Tokens
-----------------

Clients specify the ``resource`` parameter when making authorization or token requests:

.. code-block:: http

    GET /authorize?
        response_type=code&
        client_id=my_client&
        scope=read write&
        resource=https://api1.example.com&
        redirect_uri=https://app.example.com/callback

Multiple resource indicators can be specified in the authorization request::

    resource=https://api1.example.com&resource=https://api2.example.com

However, when exchanging the authorization code for a token, only **one** resource can be specified per token request. To obtain tokens for multiple resources, use the refresh token to request additional access tokens for each resource individually:

.. code-block:: http

    POST /token
        grant_type=refresh_token&
        refresh_token=...&
        resource=https://api2.example.com

Token Result
------------

The resulting access token will have its ``aud`` (audience) claim set only to the requested resource, and will only contain scopes that are associated with that resource.

Validation
----------

API resources should validate that the ``aud`` claim in received tokens matches their own resource identifier to ensure the token was intended for them.