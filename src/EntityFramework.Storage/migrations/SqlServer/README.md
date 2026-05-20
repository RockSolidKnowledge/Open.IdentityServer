Migration Scripts
====================

This directory contain a collection of SQL scripts that will be useful for setting up Open.IdentityServer, weather starting fresh or migrating from an existing solution.

## Stating Fresh

- SQL Server [Configuration](./Migrations/ConfigurationDb.sql) and [PersistedGrant/Operational](./Migrations/PersistedGrantDb.sql)
- Postgre SQL (TODO)
- My SQL (TODO)

## Migrating from Another Solution

- SQL Server IdentityServer4 to Open.IdentityServer [Configuration and PersistedGrant/Operational](./IdS4_to_OpenIdS.sql) (TODO, split into script for each context)
- Postgre SQL IdentityServer4 to Open.IdentityServer (TODO)
- My SQL IdentityServer4 to Open.IdentityServer (TODO)