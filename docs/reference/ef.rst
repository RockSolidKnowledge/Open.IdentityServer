.. _refEF:
Entity Framework Support
========================

An EntityFramework-based implementation is provided for the configuration and operational data extensibility points in IdentityServer.
The use of EntityFramework allows any EF-supported database to be used with this library.

The code for this library is located `here <https://github.com/RockSolidKnowledge/Open.IdentityServer/tree/main/src/EntityFramework>`_ (with the underlying storage code `here <https://github.com/RockSolidKnowledge/Open.IdentityServer/tree/main/src/EntityFramework.Storage>`_) and the NuGet package is `here <https://www.nuget.org/packages/Open.IdentityServer.EntityFramework>`_.

The features provided by this library are broken down into two main areas: configuration store and operational store support.
These two different areas can be used independently or together, based upon the needs of the hosting application.

Configuration Store support for Clients, Resources, and CORS settings
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

If client, identity resource, API resource, or CORS data is desired to be loaded from a EF-supported database 
(rather than use in-memory configuration), then the configuration store can be used.
This support provides implementations of the ``IClientStore``, ``IResourceStore``, and the ``ICorsPolicyService`` extensibility points.
These implementations use a ``DbContext``-derived class called ``ConfigurationDbContext`` to model the tables in the database.

To use the configuration store support, use the ``AddConfigurationStore`` extension method after the call to ``AddIdentityServer``

.. code-block:: csharp

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        const string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;database=Open.IdentityServer.EntityFramework-2.0.0;trusted_connection=yes;";
        var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
        
        services.AddIdentityServer()
            // this adds the config data from DB (clients, resources, CORS)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
            });
    }

To configure the configuration store, use the ``ConfigurationStoreOptions`` options object passed to the configuration callback.

ConfigurationStoreOptions
^^^^^^^^^^^^^^^^^^^^^^^^^
This options class contains properties to control the configuration store and ``ConfigurationDbContext``.

``ConfigureDbContext``
    Delegate of type ``Action<DbContextOptionsBuilder>`` used as a callback to configure the underlying ``ConfigurationDbContext``.
    The delegate can configure the ``ConfigurationDbContext`` in the same way if EF were being used directly with ``AddDbContext``, which allows any EF-supported database to be used.
``DefaultSchema``
    Allows setting the default database schema name for all the tables in the ``ConfigurationDbContext``
    ::
            options.DefaultSchema = "myConfigurationSchema";      

If you need to change the schema for the Migration History Table, you can chain another action to the ``UseSqlServer``

.. code-block:: csharp

    options.ConfigureDbContext = b =>
        b.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(migrationsAssembly).MigrationsHistoryTable("MyConfigurationMigrationTable", "myConfigurationSchema"));

Operational Store support for persisted grants
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

If :ref:`persisted grants <refPersistedGrants>` are desired to be loaded from a EF-supported database (rather than the default in-memory database), then the operational store can be used.
This support provides implementations of the ``IPersistedGrantStore`` extensibility point.
The implementation uses a ``DbContext``-derived class called ``PersistedGrantDbContext`` to model the table in the database.

To use the operational store support, use the ``AddOperationalStore`` extension method after the call to ``AddIdentityServer``

.. code-block:: csharp

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        const string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;database=Open.IdentityServer.EntityFramework-2.0.0;trusted_connection=yes;";
        var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
        
        services.AddIdentityServer()
            // this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
            });
    }

To configure the operational store, use the ``OperationalStoreOptions`` options object passed to the configuration callback.

OperationalStoreOptions
^^^^^^^^^^^^^^^^^^^^^^^
This options class contains properties to control the operational store and ``PersistedGrantDbContext``.

``ConfigureDbContext``
    Delegate of type ``Action<DbContextOptionsBuilder>`` used as a callback to configure the underlying ``PersistedGrantDbContext``.
    The delegate can configure the ``PersistedGrantDbContext`` in the same way if EF were being used directly with ``AddDbContext``, which allows any EF-supported database to be used.
``DefaultSchema``
    Allows setting the default database schema name for all the tables in the ``PersistedGrantDbContext``.
``EnableTokenCleanup``
    Indicates whether expired grants will be automatically cleaned up from the database. The default is ``false``.
``TokenCleanupInterval``
    The token cleanup interval (in seconds). The default is 3600 (1 hour).

.. note:: The token cleanup feature does *not* remove persisted grants that are *consumed* (see :ref:`persisted grants <refPersistedGrants>`).

Database creation and schema changes across different versions of IdentityServer
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

It is very likely that across different versions of IdentityServer (and the EF support) that the database schema will change to accommodate new and changing features.

If your looking for support in creating your database or migrating data please get in touch with our `sales team <mailto:sales@identityserver.com>`_. You can also 
manage the database creation, schema changes, and data migration in any way your organization sees fit.

Using EF migrations is one possible approach to this. 
If you do wish to use migrations, then see the :ref:`EF quickstart <refEntityFrameworkQuickstart>` for samples on how to get started, or consult the Microsoft `documentation on EF migrations <https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/index>`_.

We also publish `sample SQL scripts <https://github.com/RockSolidKnowledge/Open.IdentityServer/tree/main/src/EntityFramework.Storage/migrations/SqlServer/scripts>`_ for the current version of the database schema.

Generating Delta Scripts
^^^^^^^^^^^^^^^^^^^^^^^^

It is possible with some manual configuration to use our `Migrator Project <https://github.com/RockSolidKnowledge/Open.IdentityServer/tree/main/src/EntityFramework.Storage/migrations/SqlServer>`_ to generate scripts for migrating the schema from one version to another.

.. note:: The connection string used in these steps only needs to be valid in format — it does not need to connect to a real database. EF migrations tooling uses it to determine the provider, not to execute against a live database.

1. In the Migrator project's ``.csproj`` file, change the project reference to a package reference for your **current** version of Open.IdentityServer:

.. code-block:: xml

    <!-- Remove/comment the following line -->
    <ProjectReference Include="..\..\src\Open.IdentityServer.EntityFramework.Storage.csproj"/>

    <!-- Add the following line, where x.x.x is your current version -->
    <PackageReference Include="Open.IdentityServer.EntityFramework.Storage" Version="x.x.x"/>

2. Run the ``buildschema.sh`` bash script. This takes ``DbProvider`` (SqlServer, PostgreSql, or MySql) and a ``ConnectionString`` as arguments in that order. Once complete, you should have a migration for each ``DbContext`` that matches the referenced version of ``Open.IdentityServer.EntityFramework.Storage``.

3. Update the ``Open.IdentityServer.EntityFramework.Storage`` package reference in the ``.csproj`` file to the **new** version you are migrating to.

4. Generate new migrations for each ``DbContext``, then use the two migrations to produce delta SQL scripts. For example:

.. code-block:: bash

    export DbProvider=MySql
    export ConnectionStrings__db='Server=myServerAddress;Database=myDataBase;'

    dotnet ef migrations add Grants_to_XXX -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
    dotnet ef migrations add Configuration_to_XXX -c ConfigurationDbContext -o Migrations/ConfigurationDb

    dotnet ef migrations script Grants Grants_to_XXX -c PersistedGrantDbContext -o PersistedGrantDb_vxxx_to_vxxx.sql
    dotnet ef migrations script Configuration Configuration_to_XXX -c ConfigurationDbContext -o ConfigurationDb_vxxx_to_vxxx.sql