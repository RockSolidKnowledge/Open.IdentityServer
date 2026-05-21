Migration Scripts
====================

This directory contain a collection of SQL scripts that will be useful for setting up Open.IdentityServer, weather starting fresh or migrating from an existing solution.

## Stating Fresh

- SQL Server [Configuration](./SqlServer/ConfigurationDb.sql) and [PersistedGrant/Operational](./SqlServer/PersistedGrantDb.sql)
- Postgre SQL [Configuration](./PostgreSql/ConfigurationDb.sql) and [PersistedGrant/Operational](./PostgreSql/PersistedGrantDb.sql)
- My SQL [Configuration](./MySql/ConfigurationDb.sql) and [PersistedGrant/Operational](./MySql/PersistedGrantDb.sql)
- SQLite [Configuration](./Sqlite/ConfigurationDb.sql) and [PersistedGrant/Operational](./Sqlite/PersistedGrantDb.sql)

## Migrating from Another Solution

- SQL Server IdentityServer4 to Open.IdentityServer [Configuration](./SqlServer/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./SqlServer/PersistedGrantDb_IdS4_to_OpenIdS.sql)
- Postgre SQL IdentityServer4 to Open.IdentityServer [Configuration](./PostgreSql/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./PostgreSql/PersistedGrantDb_IdS4_to_OpenIdS.sql)
- My SQL IdentityServer4 to Open.IdentityServer [Configuration](./MySql/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./MySql/PersistedGrantDb_IdS4_to_OpenIdS.sql)
- SQLite IdentityServer4 to Open.IdentityServer [Configuration](./Sqlite/ConfigurationDb_IdS4_to_OpenIdS.sql) and [PersistedGrant/Operational](./Sqlite/PersistedGrantDb_IdS4_to_OpenIdS.sql)