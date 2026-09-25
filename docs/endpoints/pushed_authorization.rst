.. _refPushedAuthorization:

Pushed Authorization Endpoint
==============================

The pushed authorization endpoint accepts an authorization request from a client and returns a short-lived ``request_uri``. The client then sends that value to the :ref:`authorize endpoint <refAuthorize>` instead of sending the authorization parameters through the browser.

The endpoint URL is published in the ``pushed_authorization_request_endpoint`` field of the :ref:`discovery document <refDiscovery>` when the endpoint is enabled.

The endpoint is available at ``/connect/par`` relative to the IdentityServer base address. Requests must use ``POST`` and ``application/x-www-form-urlencoded`` encoding. The client must authenticate at the endpoint using one of the configured client authentication methods.

Request parameters
^^^^^^^^^^^^^^^^^^

The request accepts the authorization request parameters supported by the :ref:`authorize endpoint <refAuthorize>`, including:

``client_id``
    The client identifier (required).
``scope``
    One or more registered scopes (required).
``redirect_uri``
    A redirect URI registered for the client (required when required by the authorization request).
``response_type``
    The response type requested by the client (required).
``state``, ``nonce``, ``code_challenge`` and ``code_challenge_method``
    Optional authorization request parameters as described in the :ref:`authorize endpoint <refAuthorize>` documentation.

The ``request_uri`` parameter is not allowed at this endpoint. Request objects can be used according to the authorization request validation rules, but a PAR request cannot contain another request URI.

Response
^^^^^^^^

A successful request returns ``201 Created`` and a JSON response. The response is not cacheable.

.. code-block:: json

    {
        "request_uri": "urn:ietf:params:oauth:request-uri:example",
        "expires_in": 600
    }

``request_uri``
    The URI to pass to the ``request_uri`` parameter at the authorize endpoint.
``expires_in``
    The lifetime of the request URI in seconds.

Errors are returned using the standard OAuth error response format. Invalid client authentication returns ``invalid_client``. Invalid or incomplete authorization parameters return ``invalid_request`` or the corresponding authorization request validation error.

Example
^^^^^^^

.. code-block:: http

    POST /connect/par HTTP/1.1
    Host: demo.identityserver.com
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic Y2xpZW50MTpzZWNyZXQ=

    client_id=client1&scope=openid%20api1&response_type=code&redirect_uri=https%3A%2F%2Fclient.example%2Fcallback&state=abc&code_challenge=...&code_challenge_method=S256

The client uses the returned URI in the browser authorization request:

.. code-block:: http

    GET /connect/authorize?client_id=client1&request_uri=urn%3Aietf%3Aparams%3Aoauth%3Arequest-uri%3Aexample

The ``client_id`` in the authorize request must match the client that created the pushed request. The stored request is removed after a successful authorization flow and is unavailable after its lifetime expires.
