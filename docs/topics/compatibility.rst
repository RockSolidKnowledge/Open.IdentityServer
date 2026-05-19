.. _refCompatibility:
Compatibility
======

With Open.IdentityServer we have decided to match the current Duende IdentityServer schema to enable
the use of the existing IdentityServer database with no migration needed.

With Open.IdentityServer version 1, most of these additional tables and fields will be unused until we come to implement
these features in Open.IdentityServer.

Keys Compatibility Store
^^^^^^^^^^^^^^^^^^^^

You will be able to configure read-only access to the ``Keys`` table in Open.IdentityServer. This will take the latest 
signing key that exists and use it for signing, and all signing keys in the table will be used for validation. We have added the extension method ``IIdentityServerBuilder.AddCompatibilityKeyStores`` which will register these key stores in the DI container for you.

.. code-block:: csharp

    services.AddIdentityServer(options =>
        {
            //IdS Config
        })
        .AddCompatibilityKeyStores();
