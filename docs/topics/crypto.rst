.. _refCrypto:
Cryptography, Keys and HTTPS
============================

Open.IdentityServer relies on a couple of crypto mechanisms to do its job.

Token signing and validation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Open.IdentityServer needs an asymmetric key pair to sign and validate JWTs. 
This keymaterial can be either packaged as a certificate or just raw keys.
Both RSA and ECDSA keys are supported and the supported signing algorithms are: RS256, RS384, RS512, PS256, PS384, PS512, ES256, ES384 and ES512.

You can use multiple signing keys simultaneously, but only one signing key per algorithm is supported.
The first signing key you register is considered the default signing key.

Both :ref:`clients <refClient>` and :ref:`API resources <refApiResource>` can express preferences on the signing algorithm.
If you request a single token for multiple API resources, all resources need to agree on at least one allowed signing algorithm.

Loading of signing key and the corresponding validation part is done by implementations of ``ISigningCredentialStore`` and ``IValidationKeysStore``.
If you want to customize the loading of the keys, you can implement those interfaces and register them with DI.

The DI builder extensions has a couple of convenience methods to set signing and validation keys - see :ref:`here <refStartupKeyMaterial>`.

Signing key rollover
^^^^^^^^^^^^^^^^^^^^
While you can only use one signing key at a time, you can publish more than one validation key to the discovery document.
This is useful for key rollover.

In a nutshell, a rollover typically works like this:

1. you request/create new key material
2. you publish the new validation key in addition to the current one. You can use the ``AddValidationKey`` builder extension method for that.
3. all clients and APIs now have a chance to learn about the new key the next time they update their local copy of the discovery document
4. after a certain amount of time (e.g. 24h) all clients and APIs should now accept both the old and the new key material
5. keep the old key material around for as long as you like, maybe you have long-lived tokens that need validation
6. retire the old key material when it is not used anymore
7. all clients and APIs will "forget" the old key next time they update their local copy of the discovery document

This requires that clients and APIs use the discovery document, and also have a feature to periodically refresh their configuration.

This process is also commonly referred to as key rotation. There are many ways that you could implement key rotation in your Open.IdentityServer:

- manually update them
- write some logic to automatically manage them
- use a 3rd party tool to do it for you

A commercial key rotation component for Open.IdentityServer is planned from Rock Solid Knowledge and is on the roadmap.

.. Note:: With v1 of Open.IdentityServer you will be able to get :ref:`read-only access <refReadOnlyKeyStore>` to the keys managed by Duende IdentityServer but rotation will be disabled

Data protection
^^^^^^^^^^^^^^^
Cookie authentication in ASP.NET Core (or anti-forgery in MVC) use the ASP.NET Core data protection feature.
Depending on your deployment scenario, this might require additional configuration. See the Microsoft `docs <https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview>`_ for more information.

HTTPS
^^^^^
We don't enforce the use of HTTPS, but for production it is mandatory for every interaction with Open.IdentityServer.
