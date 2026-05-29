.. _refDiscovery:
Discovery Endpoint
==================

The discovery endpoint can be used to retrieve metadata about your IdentityServer - 
it returns information like the issuer name, key material, supported scopes etc. See the `spec <https://openid.net/specs/openid-connect-discovery-1_0.html>`_ for more details.

The discovery endpoint is available via `/.well-known/openid-configuration` relative to the base address, for example if your base address is 'https://demo.identityserver.com'::

    https://demo.identityserver.com/.well-known/openid-configuration