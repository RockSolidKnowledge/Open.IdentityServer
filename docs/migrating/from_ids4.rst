IdentityServer4 to Open.IdentityServer
=======================================

This guide is intended for users of IdentityServer4 that wish to upgrade to Open.IdentityServer.

.. note::

    IdentityServer4 reached end-of-life in November 2022. Open.IdentityServer is a supported, open-source successor built on modern .NET.

Prerequisites
-------------

- .NET 10.0 SDK or later
- A working IdentityServer4 project
- Access to your database (if using Entity Framework stores)

Migration Steps
---------------

#. **Back up your database**

   Before making any changes, take a full backup of your IdentityServer4 database. This migration requires schema modifications that are not easily reversible.

   .. code-block:: bash

       # Example for SQL Server
       sqlcmd -S localhost -Q "BACKUP DATABASE [IdentityServer] TO DISK = '/backups/identityserver_backup.bak'"

#. **Migrate the database schema**

   The Open.IdentityServer schema includes additional columns and tables compared to IdentityServer4. Migration scripts are available for common database providers:

   - `Migration scripts on GitHub <https://github.com/RockSolidKnowledge/Open.IdentityServer/src/EntityFramework.Storage/migrations/scripts>`_

   Select the script appropriate for your database provider (SQL Server, PostgreSQL, MySQL, etc.) and run it against your database:

   .. code-block:: bash

       # Example for SQL Server
       sqlcmd -S localhost -d IdentityServer -i ids4_to_openids_sqlserver.sql

   .. warning::

       Review the migration scripts before running them. If you have customised your schema or added additional columns, you may need to adjust the scripts accordingly.

#. **Update your target framework**

   IdentityServer4 targeted .NET Core 3.1 or .NET 5. Open.IdentityServer requires .NET 10.0 or later. Update your project file:

   .. code-block:: xml

       <PropertyGroup>
           <TargetFramework>net10.0</TargetFramework>
       </PropertyGroup>

#. **Replace NuGet packages**

   Remove the IdentityServer4 packages and install the Open.IdentityServer equivalents:

   .. code-block:: bash

       dotnet remove package IdentityServer4
       dotnet remove package IdentityServer4.EntityFramework
       dotnet remove package IdentityServer4.AspNetIdentity

       dotnet add package Open.IdentityServer
       dotnet add package Open.IdentityServer.EntityFramework
       dotnet add package Open.IdentityServer.AspNetIdentity

#. **Update namespaces**

   Replace all ``IdentityServer4`` namespaces with ``Open.IdentityServer`` throughout your project. A find-and-replace across the solution is the quickest approach:

   .. list-table::
       :header-rows: 1

       * - Old Namespace
         - New Namespace
       * - ``IdentityServer4``
         - ``Open.IdentityServer``
       * - ``IdentityServer4.Models``
         - ``Open.IdentityServer.Models``
       * - ``IdentityServer4.Services``
         - ``Open.IdentityServer.Services``
       * - ``IdentityServer4.Stores``
         - ``Open.IdentityServer.Stores``
       * - ``IdentityServer4.EntityFramework``
         - ``Open.IdentityServer.EntityFramework``
       * - ``IdentityServer4.Extensions``
         - ``Open.IdentityServer.Extensions``

   After replacing, attempt a build. Any remaining references will surface as compile errors, making them easy to locate and fix.

#. **Update service registration**

   The service registration in ``Startup.cs`` or ``Program.cs`` remains similar, but verify your configuration:

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

#. **Test your application**

   Run your application and verify:

   - The discovery document loads correctly at ``/.well-known/openid-configuration``
   - Client authentication works as expected
   - Token issuance and validation function correctly
   - Any custom grant types or profile services operate normally

Troubleshooting
---------------

**Database errors at runtime**
    Ensure the migration scripts ran successfully and that your connection string is correct. Compare your schema against the expected schema in the Open.IdentityServer Entity Framework migrations.

**Token validation failures**
    If downstream APIs reject tokens after migration, ensure that the signing key configuration is correct and that APIs are referencing the updated discovery endpoint.

Contact `support@identityserver.com <mailto:support@identityserver.com>`_ if you require assistance.