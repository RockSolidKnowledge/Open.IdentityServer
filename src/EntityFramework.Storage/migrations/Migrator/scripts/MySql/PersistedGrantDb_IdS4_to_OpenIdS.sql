START TRANSACTION;

ALTER TABLE `PersistedGrants` DROP PRIMARY KEY;

ALTER TABLE `PersistedGrants` MODIFY COLUMN `Key` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `PersistedGrants` ADD `Id` bigint NOT NULL AUTO_INCREMENT,
ADD CONSTRAINT `PK_PersistedGrants` PRIMARY KEY (`Id`);

CREATE TABLE `Keys` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Version` int NOT NULL,
    `Use` longtext CHARACTER SET utf8mb4 NULL,
    `DataProtected` tinyint(1) NOT NULL,
    `Algorithm` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `IsX509Certificate` tinyint(1) NOT NULL,
    `Data` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    CONSTRAINT `PK_Keys` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `PushedAuthorizationRequests` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ReferenceHashValue` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Parameters` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_PushedAuthorizationRequests` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ServerSideSessions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Scheme` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `SubjectId` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `SessionId` varchar(100) CHARACTER SET utf8mb4 NULL,
    `DisplayName` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Created` datetime(6) NOT NULL,
    `Renewed` datetime(6) NOT NULL,
    `Expires` datetime(6) NULL,
    `Data` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ServerSideSessions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_PersistedGrants_ConsumedTime` ON `PersistedGrants` (`ConsumedTime`);

CREATE UNIQUE INDEX `IX_PersistedGrants_Key` ON `PersistedGrants` (`Key`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260529101741_Grants_to_OpenIdS', '9.0.16');

COMMIT;

