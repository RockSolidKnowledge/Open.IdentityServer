Authorize Request Objects
=========================

Instead of providing all parameters for an authorize request as individual query string parameters, you can package them up in signed JWTs.
You can either transmit them by value or by reference to the authorize endpoint - see the `spec <https://openid.net/specs/openid-connect-core-1_0.html#JWTRequests>`_ for more details.

IdentityServer requires the request JWTs to be signed. We support X509 certificates, symmetric and RSA keys. 
For symmetric and RSA you need to add a `JWK <https://tools.ietf.org/html/rfc7517>`_ secret to the corresponding client, e.g.::

    private readonly string _symmetricJwk = @"{ 'kty': 'oct', 'use': 'sig', 'kid': '1', 'k': 'nYA-IFt8xTsdBHe9hunvizcp3Dt7f6qGqudq18kZHNtvqEGjJ9Ud-9x3kbQ-LYfLHS3xM2MpFQFg1JzT_0U_F8DI40oby4TvBDGszP664UgA8_5GjB7Flnrlsap1NlitvNpgQX3lpyTvC2zVuQ-UVsXbBDAaSBUSlnw7SE4LM8Ye2WYZrdCCXL8yAX9vIR7vf77yvNTEcBCI6y4JlvZaqMB4YKVSfygs8XqGGCHjLpE5bvI-A4ESbAUX26cVFvCeDg9pR6HK7BmwPMlO96krgtKZcXEJtUELYPys6-rbwAIdmxJxKxpgRpt0FRv_9fm6YPwG7QivYBX-vRwaodL1TA', 'alg': 'HS256'}";

    var client = new Client
    {
        ClientId = "foo",

        ClientSecrets = 
        {
            new Secret
            {
                Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                Value = _symmetricJwk
            }
        }
    }

.. note:: Microsoft.IdentityModel.Tokens.JsonWebKeyConverter has various helpers to convert keys to JWKs

Because of a bug in Microsoft's JWT library, X509 certificates cannot be formatted as JWKs right now. You can use the base64 representation instead::

    ClientSecrets =     
    {
        new Secret
        {
            Type = IdentityServerConstants.SecretTypes.X509CertificateBase64,
            Value = Convert.ToBase64String(cert.Export(X509ContentType.Cert))
        }
    }

Passing request JWTs by reference
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
If the ``request_uri`` parameter is used, IdentityServer will make an outgoing HTTP call to fetch the JWT from the specified URL.

You can customize the HTTP client used for this outgoing connection, e.g. to add caching or retry logic (e.g. via the Polly library)::

    builder.AddJwtRequestUriHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
        .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3)
        }));

.. note:: Request URI processing is disabled by default. Enable on the :ref:`IdentityServer Options <refOptions>` under ``Endpoints``.

Accessing the request object data
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
You can access the validated data from the request object in two ways

* wherever you have access to the ``ValidatedAuthorizeRequest``, the ``RequestObjectValues`` dictionary holds the values
* in the UI code you can call ``IIdentityServerInteractionService.GetAuthorizationContextAsync``, the resulting ``AuthorizationRequest`` object contains the ``RequestObjectValues`` dictionary as well
