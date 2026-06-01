BEGIN TRANSACTION;
DROP INDEX [IX_IdentityResourceProperties_IdentityResourceId] ON [IdentityResourceProperties];

DROP INDEX [IX_IdentityResourceClaims_IdentityResourceId] ON [IdentityResourceClaims];

DROP INDEX [IX_ClientScopes_ClientId] ON [ClientScopes];

DROP INDEX [IX_ClientRedirectUris_ClientId] ON [ClientRedirectUris];

DROP INDEX [IX_ClientProperties_ClientId] ON [ClientProperties];

DROP INDEX [IX_ClientPostLogoutRedirectUris_ClientId] ON [ClientPostLogoutRedirectUris];

DROP INDEX [IX_ClientIdPRestrictions_ClientId] ON [ClientIdPRestrictions];

DROP INDEX [IX_ClientGrantTypes_ClientId] ON [ClientGrantTypes];

DROP INDEX [IX_ClientCorsOrigins_ClientId] ON [ClientCorsOrigins];

DROP INDEX [IX_ClientClaims_ClientId] ON [ClientClaims];

DROP INDEX [IX_ApiScopeProperties_ScopeId] ON [ApiScopeProperties];

DROP INDEX [IX_ApiScopeClaims_ScopeId] ON [ApiScopeClaims];

DROP INDEX [IX_ApiResourceScopes_ApiResourceId] ON [ApiResourceScopes];

DROP INDEX [IX_ApiResourceProperties_ApiResourceId] ON [ApiResourceProperties];

DROP INDEX [IX_ApiResourceClaims_ApiResourceId] ON [ApiResourceClaims];

ALTER TABLE [Clients] ADD [CibaLifetime] int NULL;

ALTER TABLE [Clients] ADD [CoordinateLifetimeWithUserSession] bit NULL;

ALTER TABLE [Clients] ADD [DPoPClockSkew] time NOT NULL DEFAULT '00:00:00';

ALTER TABLE [Clients] ADD [DPoPValidationMode] int NOT NULL DEFAULT 0;

ALTER TABLE [Clients] ADD [InitiateLoginUri] nvarchar(2000) NULL;

ALTER TABLE [Clients] ADD [PollingInterval] int NULL;

ALTER TABLE [Clients] ADD [PushedAuthorizationLifetime] int NULL;

ALTER TABLE [Clients] ADD [RequireDPoP] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Clients] ADD [RequirePushedAuthorization] bit NOT NULL DEFAULT CAST(0 AS bit);

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientRedirectUris]') AND [c].[name] = N'RedirectUri');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [ClientRedirectUris] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [ClientRedirectUris] ALTER COLUMN [RedirectUri] nvarchar(400) NOT NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientPostLogoutRedirectUris]') AND [c].[name] = N'PostLogoutRedirectUri');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ClientPostLogoutRedirectUris] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [ClientPostLogoutRedirectUris] ALTER COLUMN [PostLogoutRedirectUri] nvarchar(400) NOT NULL;

ALTER TABLE [ApiScopes] ADD [Created] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [ApiScopes] ADD [LastAccessed] datetime2 NULL;

ALTER TABLE [ApiScopes] ADD [NonEditable] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ApiScopes] ADD [Updated] datetime2 NULL;

ALTER TABLE [ApiResources] ADD [RequireResourceIndicator] bit NOT NULL DEFAULT CAST(0 AS bit);

CREATE TABLE [IdentityProviders] (
    [Id] int NOT NULL IDENTITY,
    [Scheme] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(200) NULL,
    [Enabled] bit NOT NULL,
    [Type] nvarchar(20) NOT NULL,
    [Properties] nvarchar(max) NULL,
    [Created] datetime2 NOT NULL,
    [LastAccessed] datetime2 NULL,
    [NonEditable] bit NOT NULL,
    [Updated] datetime2 NULL,
    CONSTRAINT [PK_IdentityProviders] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_IdentityResourceProperties_IdentityResourceId_Key] ON [IdentityResourceProperties] ([IdentityResourceId], [Key]);

CREATE UNIQUE INDEX [IX_IdentityResourceClaims_IdentityResourceId_Type] ON [IdentityResourceClaims] ([IdentityResourceId], [Type]);

CREATE UNIQUE INDEX [IX_ClientScopes_ClientId_Scope] ON [ClientScopes] ([ClientId], [Scope]);

CREATE UNIQUE INDEX [IX_ClientRedirectUris_ClientId_RedirectUri] ON [ClientRedirectUris] ([ClientId], [RedirectUri]);

CREATE UNIQUE INDEX [IX_ClientProperties_ClientId_Key] ON [ClientProperties] ([ClientId], [Key]);

CREATE UNIQUE INDEX [IX_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri] ON [ClientPostLogoutRedirectUris] ([ClientId], [PostLogoutRedirectUri]);

CREATE UNIQUE INDEX [IX_ClientIdPRestrictions_ClientId_Provider] ON [ClientIdPRestrictions] ([ClientId], [Provider]);

CREATE UNIQUE INDEX [IX_ClientGrantTypes_ClientId_GrantType] ON [ClientGrantTypes] ([ClientId], [GrantType]);

CREATE UNIQUE INDEX [IX_ClientCorsOrigins_ClientId_Origin] ON [ClientCorsOrigins] ([ClientId], [Origin]);

CREATE UNIQUE INDEX [IX_ClientClaims_ClientId_Type_Value] ON [ClientClaims] ([ClientId], [Type], [Value]);

CREATE UNIQUE INDEX [IX_ApiScopeProperties_ScopeId_Key] ON [ApiScopeProperties] ([ScopeId], [Key]);

CREATE UNIQUE INDEX [IX_ApiScopeClaims_ScopeId_Type] ON [ApiScopeClaims] ([ScopeId], [Type]);

CREATE UNIQUE INDEX [IX_ApiResourceScopes_ApiResourceId_Scope] ON [ApiResourceScopes] ([ApiResourceId], [Scope]);

CREATE UNIQUE INDEX [IX_ApiResourceProperties_ApiResourceId_Key] ON [ApiResourceProperties] ([ApiResourceId], [Key]);

CREATE UNIQUE INDEX [IX_ApiResourceClaims_ApiResourceId_Type] ON [ApiResourceClaims] ([ApiResourceId], [Type]);

CREATE UNIQUE INDEX [IX_IdentityProviders_Scheme] ON [IdentityProviders] ([Scheme]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260601121103_Configuration_to_OpenIdS', N'10.0.8');

COMMIT;
GO

