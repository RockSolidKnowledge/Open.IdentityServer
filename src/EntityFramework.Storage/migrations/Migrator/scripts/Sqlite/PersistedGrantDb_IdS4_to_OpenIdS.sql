BEGIN TRANSACTION;
ALTER TABLE "PersistedGrants" ADD "Id" INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "Keys" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Keys" PRIMARY KEY,
    "Version" INTEGER NOT NULL,
    "Use" TEXT NULL,
    "DataProtected" INTEGER NOT NULL,
    "Algorithm" TEXT NOT NULL,
    "IsX509Certificate" INTEGER NOT NULL,
    "Data" TEXT NOT NULL,
    "Created" TEXT NOT NULL
);

CREATE TABLE "PushedAuthorizationRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PushedAuthorizationRequests" PRIMARY KEY AUTOINCREMENT,
    "ReferenceHashValue" TEXT NOT NULL,
    "Created" TEXT NOT NULL,
    "Parameters" TEXT NOT NULL
);

CREATE TABLE "ServerSideSessions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ServerSideSessions" PRIMARY KEY AUTOINCREMENT,
    "Key" TEXT NOT NULL,
    "Scheme" TEXT NOT NULL,
    "SubjectId" TEXT NOT NULL,
    "SessionId" TEXT NULL,
    "DisplayName" TEXT NULL,
    "Created" TEXT NOT NULL,
    "Renewed" TEXT NOT NULL,
    "Expires" TEXT NULL,
    "Data" TEXT NOT NULL
);

CREATE INDEX "IX_PersistedGrants_ConsumedTime" ON "PersistedGrants" ("ConsumedTime");

CREATE UNIQUE INDEX "IX_PersistedGrants_Key" ON "PersistedGrants" ("Key");

CREATE TABLE "ef_temp_PersistedGrants" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PersistedGrants" PRIMARY KEY AUTOINCREMENT,
    "ClientId" TEXT NOT NULL,
    "ConsumedTime" TEXT NULL,
    "CreationTime" TEXT NOT NULL,
    "Data" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Expiration" TEXT NULL,
    "Key" TEXT NULL,
    "SessionId" TEXT NULL,
    "SubjectId" TEXT NULL,
    "Type" TEXT NOT NULL
);

INSERT INTO "ef_temp_PersistedGrants" ("ClientId", "ConsumedTime", "CreationTime", "Data", "Description", "Expiration", "Key", "SessionId", "SubjectId", "Type")
SELECT "ClientId", "ConsumedTime", "CreationTime", "Data", "Description", "Expiration", "Key", "SessionId", "SubjectId", "Type"
FROM "PersistedGrants";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "PersistedGrants";

ALTER TABLE "ef_temp_PersistedGrants" RENAME TO "PersistedGrants";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_PersistedGrants_ConsumedTime" ON "PersistedGrants" ("ConsumedTime");

CREATE INDEX "IX_PersistedGrants_Expiration" ON "PersistedGrants" ("Expiration");

CREATE UNIQUE INDEX "IX_PersistedGrants_Key" ON "PersistedGrants" ("Key");

CREATE INDEX "IX_PersistedGrants_SubjectId_ClientId_Type" ON "PersistedGrants" ("SubjectId", "ClientId", "Type");

CREATE INDEX "IX_PersistedGrants_SubjectId_SessionId_Type" ON "PersistedGrants" ("SubjectId", "SessionId", "Type");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260521135317_Grants_to_OpenIdS', '10.0.8');

