CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `DeviceCodes` (
    `UserCode` varchar(200) NOT NULL,
    `DeviceCode` varchar(200) NOT NULL,
    `SubjectId` varchar(200) NULL,
    `SessionId` varchar(100) NULL,
    `ClientId` varchar(200) NOT NULL,
    `Description` varchar(200) NULL,
    `CreationTime` datetime(6) NOT NULL,
    `Expiration` datetime(6) NOT NULL,
    `Data` longtext NOT NULL,
    PRIMARY KEY (`UserCode`)
);

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

CREATE TABLE `PersistedGrants` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `Key` varchar(200) NULL,
    `Type` varchar(50) NOT NULL,
    `SubjectId` varchar(200) NULL,
    `SessionId` varchar(100) NULL,
    `ClientId` varchar(200) NOT NULL,
    `Description` varchar(200) NULL,
    `CreationTime` datetime(6) NOT NULL,
    `Expiration` datetime(6) NULL,
    `ConsumedTime` datetime(6) NULL,
    `Data` longtext NOT NULL,
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

CREATE UNIQUE INDEX `IX_DeviceCodes_DeviceCode` ON `DeviceCodes` (`DeviceCode`);

CREATE INDEX `IX_DeviceCodes_Expiration` ON `DeviceCodes` (`Expiration`);

CREATE INDEX `IX_PersistedGrants_ConsumedTime` ON `PersistedGrants` (`ConsumedTime`);

CREATE INDEX `IX_PersistedGrants_Expiration` ON `PersistedGrants` (`Expiration`);

CREATE UNIQUE INDEX `IX_PersistedGrants_Key` ON `PersistedGrants` (`Key`);

CREATE INDEX `IX_PersistedGrants_SubjectId_ClientId_Type` ON `PersistedGrants` (`SubjectId`, `ClientId`, `Type`);

CREATE INDEX `IX_PersistedGrants_SubjectId_SessionId_Type` ON `PersistedGrants` (`SubjectId`, `SessionId`, `Type`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521090113_Grants', '10.0.7');

COMMIT;

