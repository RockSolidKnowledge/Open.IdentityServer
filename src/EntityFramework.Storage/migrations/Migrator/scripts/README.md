Migration Scripts
=================

This directory contain a collection of SQL scripts that will be useful for setting up Open.IdentityServer, weather starting fresh or migrating from an existing solution.

## Stating Fresh

- SQL Server [Configuration](./SqlServer/ConfigurationDb.sql) and [PersistedGrant/Operational](./SqlServer/PersistedGrantDb.sql) (EFCore 10.0)
- Postgre SQL [Configuration](./PostgreSql/ConfigurationDb.sql) and [PersistedGrant/Operational](./PostgreSql/PersistedGrantDb.sql) (EFCore 10.0)
- My SQL [Configuration](./MySql/ConfigurationDb.sql) and [PersistedGrant/Operational](./MySql/PersistedGrantDb.sql) (EFCore 9.0)
- SQLite [Configuration](./Sqlite/ConfigurationDb.sql) and [PersistedGrant/Operational](./Sqlite/PersistedGrantDb.sql) (EFCore 10.0)

## Migrating from Another Solution

> **Warning**
> Backup your database before attempting any changes!

These migration scripts are a guide; they assume the original schema was generated using EF Core, and constraint and
index names were not explicitly defined and used defaults. Please consult your original methods of schema generation for
any customizations and modify the provided script appropriately.

You may also experience differences with default names used by EF Core, depending on the version that was used to 
originally generate your database schema. An example of this is with MySQL, older versions would truncate FK contraint
names that were too long and newer versions truncate and suffix with a `~` character. An example of this would be for
IdentityResourceProperties.IdentityResourceId, the migration from IdentityServer4 to Open.IdentityServer doesn't reference
this field, but it is a good example of possible mismatches that could cause issues.

- SQL Server IdentityServer4 to Open.IdentityServer [Configuration](./SqlServer/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./SqlServer/PersistedGrantDb_IdS4_to_OpenIdS.sql) (EFCore 10.0)
- Postgre SQL IdentityServer4 to Open.IdentityServer [Configuration](./PostgreSql/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./PostgreSql/PersistedGrantDb_IdS4_to_OpenIdS.sql) (EFCore 10.0)
- My SQL IdentityServer4 to Open.IdentityServer [Configuration](./MySql/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./MySql/PersistedGrantDb_IdS4_to_OpenIdS.sql) (EFCore 9.0, Pomelo pkg with manual fixes) 
- SQLite IdentityServer4 to Open.IdentityServer [Configuration](./Sqlite/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./Sqlite/PersistedGrantDb_IdS4_to_OpenIdS.sql) (EFCore 10.0)