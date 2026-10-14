.. _refPushedAuthorizationTopic:

Pushed Authorization Requests
==============================

`Pushed Authorization Requests (PAR) <https://www.rfc-editor.org/rfc/rfc9126>`_ let a client send an authorization request directly to Open.IdentityServer before redirecting the user to the authorization endpoint. The server validates and stores the request, then returns a short-lived ``request_uri`` that represents it.

PAR reduces the amount of authorization data sent through the browser and helps protect request parameters from being modified in the front channel. It is especially useful for requests containing many parameters or request objects.

PAR Flow
--------

A client using PAR performs these steps:

#. Authenticate and ``POST`` the authorization parameters to the :ref:`Pushed Authorization Endpoint <refPushedAuthorization>`.
#. Receive the ``request_uri`` and ``expires_in`` values from the ``201 Created`` response.
#. Redirect the user to the :ref:`authorize endpoint <refAuthorize>` with ``client_id`` and ``request_uri``.
#. Continue the normal authorization flow. Open.IdentityServer loads the stored parameters and checks that the client identifier matches.

The pushed request is validated using the normal authorization request validation pipeline. The ``request_uri`` parameter itself must only be supplied to the authorize endpoint.

Configuration
-------------

The PAR endpoint is enabled by default. It can be configured through ``IdentityServerOptions``:

.. code-block:: csharp

    builder.AddIdentityServer(options =>
    {
        options.PushedAuthorization.Required = true;
        options.PushedAuthorization.Expiration = TimeSpan.FromMinutes(10);
        options.Endpoints.EnablePushedAuthorizationRequestEndpoint = true;
    });

``PushedAuthorization.Required``
    Requires every authorization request to use PAR. The discovery document exposes this policy as ``require_pushed_authorization_requests``.
``PushedAuthorization.Expiration``
    Sets the default lifetime of a ``request_uri``. The default is 600 seconds (10 minutes).
``Endpoints.EnablePushedAuthorizationRequestEndpoint``
    Enables or disables the PAR endpoint. When disabled, its discovery metadata is omitted and requests to the endpoint return ``404 Not Found``.

PAR can also be required for an individual client:

.. code-block:: csharp

    var client = new Client
    {
        ClientId = "client1",
        RequirePushedAuthorization = true,
        PushedAuthorizationLifetime = 300
    };

``RequirePushedAuthorization``
    Requires this client to use PAR, even when PAR is not required globally.
``PushedAuthorizationLifetime``
    Overrides the server default for this client, in seconds. If it is not set, ``PushedAuthorization.Expiration`` is used.

Storage
-------

Pushed authorization requests are stored through ``IPushedAuthorizationRequestStore``. The in-memory implementation is registered by the in-memory configuration. The Entity Framework storage package provides a persistent implementation for deployments that need PAR requests to be shared across server instances. Applications with another persistence model can register a custom implementation of the interface.

A stored request contains the authorization parameters and an expiration time. Client authentication parameters are removed before the request is stored. The request is removed after successful authorization and is rejected after it expires.

Discovery
---------

When PAR is enabled, the discovery document includes:

``pushed_authorization_request_endpoint``
    The absolute URL of the PAR endpoint.
``require_pushed_authorization_requests``
    Whether PAR is required for all authorization requests.

See the :ref:`Pushed Authorization Endpoint <refPushedAuthorization>` reference for the request and response format.
