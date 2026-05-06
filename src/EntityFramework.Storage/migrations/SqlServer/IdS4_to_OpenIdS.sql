BEGIN TRANSACTION;

-- Configurations Schema
ALTER TABLE [ApiResources] ADD [RequireResourceIndicator] bit NOT NULL DEFAULT CAST(0 AS bit);

-- Grants Schema
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

CREATE UNIQUE INDEX [IX_PersistedGrants_Key] ON [PersistedGrants] ([Key]) WHERE [Key] IS NOT NULL;

COMMIT;
GO

