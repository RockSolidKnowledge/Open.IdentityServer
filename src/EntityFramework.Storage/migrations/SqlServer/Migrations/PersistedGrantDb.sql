IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [DeviceCodes] (
    [UserCode] nvarchar(200) NOT NULL,
    [DeviceCode] nvarchar(200) NOT NULL,
    [SubjectId] nvarchar(200) NULL,
    [SessionId] nvarchar(100) NULL,
    [ClientId] nvarchar(200) NOT NULL,
    [Description] nvarchar(200) NULL,
    [CreationTime] datetime2 NOT NULL,
    [Expiration] datetime2 NOT NULL,
    [Data] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_DeviceCodes] PRIMARY KEY ([UserCode])
);

CREATE TABLE [PersistedGrants] (
    [Id] bigint NOT NULL IDENTITY,
    [Key] nvarchar(200) NULL,
    [Type] nvarchar(50) NOT NULL,
    [SubjectId] nvarchar(200) NULL,
    [SessionId] nvarchar(100) NULL,
    [ClientId] nvarchar(200) NOT NULL,
    [Description] nvarchar(200) NULL,
    [CreationTime] datetime2 NOT NULL,
    [Expiration] datetime2 NULL,
    [ConsumedTime] datetime2 NULL,
    [Data] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_PersistedGrants] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_DeviceCodes_DeviceCode] ON [DeviceCodes] ([DeviceCode]);

CREATE INDEX [IX_DeviceCodes_Expiration] ON [DeviceCodes] ([Expiration]);

CREATE INDEX [IX_PersistedGrants_Expiration] ON [PersistedGrants] ([Expiration]);

CREATE UNIQUE INDEX [IX_PersistedGrants_Key] ON [PersistedGrants] ([Key]) WHERE [Key] IS NOT NULL;

CREATE INDEX [IX_PersistedGrants_SubjectId_ClientId_Type] ON [PersistedGrants] ([SubjectId], [ClientId], [Type]);

CREATE INDEX [IX_PersistedGrants_SubjectId_SessionId_Type] ON [PersistedGrants] ([SubjectId], [SessionId], [Type]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260417104126_Grants', N'10.0.5');

COMMIT;
GO

