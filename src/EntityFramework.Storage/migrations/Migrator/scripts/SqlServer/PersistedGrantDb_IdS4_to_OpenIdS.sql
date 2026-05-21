BEGIN TRANSACTION;
ALTER TABLE [PersistedGrants] DROP CONSTRAINT [PK_PersistedGrants];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersistedGrants]') AND [c].[name] = N'Key');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [PersistedGrants] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [PersistedGrants] ALTER COLUMN [Key] nvarchar(200) NULL;

ALTER TABLE [PersistedGrants] ADD [Id] bigint NOT NULL IDENTITY;

ALTER TABLE [PersistedGrants] ADD CONSTRAINT [PK_PersistedGrants] PRIMARY KEY ([Id]);

CREATE TABLE [Keys] (
    [Id] nvarchar(450) NOT NULL,
    [Version] int NOT NULL,
    [Use] nvarchar(max) NULL,
    [DataProtected] bit NOT NULL,
    [Algorithm] nvarchar(100) NOT NULL,
    [IsX509Certificate] bit NOT NULL,
    [Data] nvarchar(max) NOT NULL,
    [Created] datetime2 NOT NULL,
    CONSTRAINT [PK_Keys] PRIMARY KEY ([Id])
);

CREATE TABLE [PushedAuthorizationRequests] (
    [Id] int NOT NULL IDENTITY,
    [ReferenceHashValue] nvarchar(64) NOT NULL,
    [Created] datetime2 NOT NULL,
    [Parameters] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_PushedAuthorizationRequests] PRIMARY KEY ([Id])
);

CREATE TABLE [ServerSideSessions] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(100) NOT NULL,
    [Scheme] nvarchar(100) NOT NULL,
    [SubjectId] nvarchar(100) NOT NULL,
    [SessionId] nvarchar(100) NULL,
    [DisplayName] nvarchar(100) NULL,
    [Created] datetime2 NOT NULL,
    [Renewed] datetime2 NOT NULL,
    [Expires] datetime2 NULL,
    [Data] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ServerSideSessions] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_PersistedGrants_ConsumedTime] ON [PersistedGrants] ([ConsumedTime]);

CREATE UNIQUE INDEX [IX_PersistedGrants_Key] ON [PersistedGrants] ([Key]) WHERE [Key] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521083533_Grants_to_OpenIdS', N'10.0.7');

COMMIT;
GO

