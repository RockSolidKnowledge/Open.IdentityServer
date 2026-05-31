Migration Scripts
====================

This directory contain a collection of SQL scripts that will be useful for setting up Open.IdentityServer, weather starting fresh or migrating from an existing solution.

## Stating Fresh

- SQL Server [Configuration](./SqlServer/ConfigurationDb.sql) and [PersistedGrant/Operational](./SqlServer/PersistedGrantDb.sql)
- Postgre SQL [Configuration](./PostgreSql/ConfigurationDb.sql) and [PersistedGrant/Operational](./PostgreSql/PersistedGrantDb.sql)
- My SQL [Configuration](./MySql/ConfigurationDb.sql) and [PersistedGrant/Operational](./MySql/PersistedGrantDb.sql)
- SQLite [Configuration](./Sqlite/ConfigurationDb.sql) and [PersistedGrant/Operational](./Sqlite/PersistedGrantDb.sql)

## Migrating from Another Solution

> **Warning**
> Backup your database before attempting any changes!

These migration scripts are a guide, they assume the original schema was generated using EF Core, and constraint and
index names were not explicitly defined and used defaults. Please consult your original methods of schema generation for
any customizations and modify the provided script appropriately.

One example of a difference you may see is with MySQL foreign key constrain name on the 'IdentityResourceProperties' 
table. Between versions of the EF Core tooling FK constraint names that were too long can be handled differently. So in
our script we have used the truncated form 'FK_IdentityResourceProperties_IdentityResources_IdentityResource' which is an
older handling but better for compatibility. But newer versions use 'FK_IdentityResourceProperties_IdentityResources_IdentityResourc~'.

- SQL Server IdentityServer4 to Open.IdentityServer [Configuration](./SqlServer/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./SqlServer/PersistedGrantDb_IdS4_to_OpenIdS.sql)
- Postgre SQL IdentityServer4 to Open.IdentityServer [Configuration](./PostgreSql/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./PostgreSql/PersistedGrantDb_IdS4_to_OpenIdS.sql)
- My SQL IdentityServer4 to Open.IdentityServer [Configuration](./MySql/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./MySql/PersistedGrantDb_IdS4_to_OpenIdS.sql)
- SQLite IdentityServer4 to Open.IdentityServer [Configuration](./Sqlite/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./Sqlite/PersistedGrantDb_IdS4_to_OpenIdS.sql)