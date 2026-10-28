.. _refCompatibility:
Compatibility
=============

With Open.IdentityServer we have decided to match the current Duende IdentityServer schema to enable
the use of the existing IdentityServer database with no migration needed.

With Open.IdentityServer version 1, most of these additional tables and fields will be unused until we come to implement
these features in Open.IdentityServer.

Keys Compatibility Store
^^^^^^^^^^^^^^^^^^^^^^^^

You will be able to configure read-only access to the ``Keys`` table in Open.IdentityServer. This will take the latest 
signing key that exists for each configured algorithm and use it for signing, and all non-expired signing keys in the 
table will be used for validation. We have added the extension method ``IIdentityServerBuilder.AddCompatibilityKeyStores`` 
which will register these key stores in the DI container for you.

.. code-block:: csharp

    services.AddIdentityServer(options =>
        {
            //IdS Config
        })
        .AddCompatibilityKeyStores();

.. note::
    By default, the default propagation time will be respected if possible, and only RS256 keys will be loaded for signing, 
    and keys will expire (not used for signing or validation) after 90 days.
    
    You can configure this behaviour in the ``CompatibilityKeyStoreOptions``.
    
    This store is intended only for transitioning to Open.IdentityServer, more robust key management should be 
    set up before keys reach expiry. If you need automatic key rotation, see our KeyRotation component on 
    `identityserver.com products page <https://www.identityserver.com/products>`_.
