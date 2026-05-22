.. _refMigrateFromDuende:
Duende.IdentityServer to Open.IdentityServer
=============================================

This guide is intended for users of Duende.IdentityServer that wish to migrate to Open.IdentityServer.

.. note::

    Open.IdentityServer is API-compatible with Duende.IdentityServer for core functionality. However, some :ref:`Duende-specific commercial <refUnsupportedFeatures>` features are not yet available.

Prerequisites
-------------

- .NET 10.0 SDK or later
- A working Duende.IdentityServer project
- Access to your database (if using Entity Framework stores)

Migration Steps
---------------

1. **Replace NuGet packages**

   Remove the Duende.IdentityServer packages and install the Open.IdentityServer equivalents:

   .. code-block:: bash

       dotnet remove package Duende.IdentityServer
       dotnet remove package Duende.IdentityServer.EntityFramework
       dotnet remove package Duende.IdentityServer.AspNetIdentity

       dotnet add package Open.IdentityServer
       dotnet add package Open.IdentityServer.EntityFramework
       dotnet add package Open.IdentityServer.AspNetIdentity

2. **Update namespaces**

   Replace all ``Duende.IdentityServer`` namespaces with ``Open.IdentityServer`` throughout your project:

   .. list-table::
       :header-rows: 1

       * - Old Namespace
         - New Namespace
       * - ``Duende.IdentityServer``
         - ``Open.IdentityServer``
       * - ``Duende.IdentityServer.Models``
         - ``Open.IdentityServer.Models``
       * - ``Duende.IdentityServer.Services``
         - ``Open.IdentityServer.Services``
       * - ``Duende.IdentityServer.Stores``
         - ``Open.IdentityServer.Stores``
       * - ``Duende.IdentityServer.EntityFramework``
         - ``Open.IdentityServer.EntityFramework``
       * - ``Duende.IdentityServer.Extensions``
         - ``Open.IdentityServer.Extensions``

   After replacing, attempt a build. Any remaining references will surface as compile errors, making them easy to locate and fix.

3. **Remove unsupported features**

   .. _refUnsupportedFeatures:

   Some Duende-specific features are not yet supported in Open.IdentityServer. If your project uses any of the following, you will need to remove or replace them:

   .. list-table::
       :header-rows: 1

       * - Feature
         - Action Required
       * - Dynamic Providers
         - Remove all references. Not yet supported in Open.IdentityServer.
       * - Automatic Key Management
         - Remove all references. You will need to configure signing keys manually, or :ref:`configure read-only key store <refCompatibility>`.
       * - Server Side Sessions
         - Remove all references. Not yet supported in Open.IdentityServer.
       * - Pushed Authorisation Requests (PAR)
         - Remove all references. Not yet supported in Open.IdentityServer.
       * - CIBA (Client Initiated Backchannel Authentication)
         - Remove all references. Not yet supported in Open.IdentityServer.

   .. warning::

       If your application relies heavily on any of these features, evaluate the impact before proceeding with migration. You may need to implement alternative solutions.

4. **Update service registration**

   The service registration should remain largely the same. Verify your ``Program.cs`` or ``Startup.cs`` configuration:

   .. code-block:: c#

       builder.Services.AddIdentityServer()
           .AddConfigurationStore(options =>
           {
               options.ConfigureDbContext = b =>
                   b.UseSqlServer(connectionString);
           })
           .AddOperationalStore(options =>
           {
               options.ConfigureDbContext = b =>
                   b.UseSqlServer(connectionString);
           });

   .. _refReadOnlyKeyStore:

   If you were using automatic key management, you will now need to register read-only key store or add a signing key explicitly:

   .. code-block:: c#

       // Read-Only Key Store
       builder.Services.AddIdentityServer()
           .AddCompatibilityKeyStores();

       // Explicit key registration
       builder.Services.AddIdentityServer()
           .AddSigningCredential(certificate);

5. **Migrate the database schema (if applicable)**

   The Entity Framework schema for Open.IdentityServer is compatible with Duende.IdentityServer. In most cases no database migration is required. However, if your Duende version included tables for unsupported features (e.g. server-side sessions or dynamic providers), those tables can be safely left in place — they will simply be unused.

6. **Test your application**

   Run your application and verify:

   - The discovery document loads correctly at ``/.well-known/openid-configuration``
   - Client authentication works as expected
   - Token issuance and validation function correctly
   - Any custom grant types or profile services operate normally

Troubleshooting
---------------

**License-related errors**
    Open.IdentityServer does not require a licence key. Remove any ``AddLicenseKey()`` or licence configuration from your setup.

**Token validation failures**
    If downstream APIs reject tokens after migration, ensure that your signing key configuration is correct and that the key material matches what APIs expect.

Contact `support@identityserver.com <mailto:support@identityserver.com>`_ if you require assistance.