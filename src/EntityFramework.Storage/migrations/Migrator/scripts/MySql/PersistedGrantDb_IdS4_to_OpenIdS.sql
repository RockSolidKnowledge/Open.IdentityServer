START TRANSACTION;
ALTER TABLE `PersistedGrants` MODIFY `Key` varchar(200) NULL;

ALTER TABLE `PersistedGrants` ADD `Id` bigint NOT NULL DEFAULT 0;

CREATE TABLE `Keys` (
    `Id` varchar(255) NOT NULL,
    `Version` int NOT NULL,
    `Use` longtext NULL,
    `DataProtected` tinyint(1) NOT NULL,
    `Algorithm` varchar(100) NOT NULL,
    `IsX509Certificate` tinyint(1) NOT NULL,
    `Data` varchar(max) NOT NULL,
    `Created` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `PushedAuthorizationRequests` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ReferenceHashValue` varchar(64) NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Parameters` varchar(max) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `ServerSideSessions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Key` varchar(100) NOT NULL,
    `Scheme` varchar(100) NOT NULL,
    `SubjectId` varchar(100) NOT NULL,
    `SessionId` varchar(100) NULL,
    `DisplayName` varchar(100) NULL,
    `Created` datetime(6) NOT NULL,
    `Renewed` datetime(6) NOT NULL,
    `Expires` datetime(6) NULL,
    `Data` varchar(max) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE INDEX `IX_PersistedGrants_ConsumedTime` ON `PersistedGrants` (`ConsumedTime`);

CREATE UNIQUE INDEX `IX_PersistedGrants_Key` ON `PersistedGrants` (`Key`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521085044_Grants_to_OpenIdS', '10.0.7');

COMMIT;

