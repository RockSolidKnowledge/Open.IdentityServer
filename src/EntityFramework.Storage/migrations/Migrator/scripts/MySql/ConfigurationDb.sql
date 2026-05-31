CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `ApiResources` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Enabled` tinyint(1) NOT NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `DisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `AllowedAccessTokenSigningAlgorithms` varchar(100) CHARACTER SET utf8mb4 NULL,
    `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NULL,
    `LastAccessed` datetime(6) NULL,
    `NonEditable` tinyint(1) NOT NULL,
    `RequireResourceIndicator` tinyint(1) NOT NULL,
    CONSTRAINT `PK_ApiResources` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ApiScopes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Enabled` tinyint(1) NOT NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `DisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `Required` tinyint(1) NOT NULL,
    `Emphasize` tinyint(1) NOT NULL,
    `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
    `NonEditable` tinyint(1) NOT NULL,
    `Created` datetime(6) NOT NULL,
    `LastAccessed` datetime(6) NULL,
    `Updated` datetime(6) NULL,
    CONSTRAINT `PK_ApiScopes` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Clients` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Enabled` tinyint(1) NOT NULL,
    `ClientId` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ProtocolType` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `RequireClientSecret` tinyint(1) NOT NULL,
    `ClientName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `ClientUri` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `LogoUri` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `RequireConsent` tinyint(1) NOT NULL,
    `AllowRememberConsent` tinyint(1) NOT NULL,
    `AlwaysIncludeUserClaimsInIdToken` tinyint(1) NOT NULL,
    `RequirePkce` tinyint(1) NOT NULL,
    `AllowPlainTextPkce` tinyint(1) NOT NULL,
    `RequireRequestObject` tinyint(1) NOT NULL,
    `AllowAccessTokensViaBrowser` tinyint(1) NOT NULL,
    `FrontChannelLogoutUri` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `FrontChannelLogoutSessionRequired` tinyint(1) NOT NULL,
    `BackChannelLogoutUri` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `BackChannelLogoutSessionRequired` tinyint(1) NOT NULL,
    `AllowOfflineAccess` tinyint(1) NOT NULL,
    `IdentityTokenLifetime` int NOT NULL,
    `AllowedIdentityTokenSigningAlgorithms` varchar(100) CHARACTER SET utf8mb4 NULL,
    `AccessTokenLifetime` int NOT NULL,
    `AuthorizationCodeLifetime` int NOT NULL,
    `ConsentLifetime` int NULL,
    `AbsoluteRefreshTokenLifetime` int NOT NULL,
    `SlidingRefreshTokenLifetime` int NOT NULL,
    `RefreshTokenUsage` int NOT NULL,
    `UpdateAccessTokenClaimsOnRefresh` tinyint(1) NOT NULL,
    `RefreshTokenExpiration` int NOT NULL,
    `AccessTokenType` int NOT NULL,
    `EnableLocalLogin` tinyint(1) NOT NULL,
    `IncludeJwtId` tinyint(1) NOT NULL,
    `AlwaysSendClientClaims` tinyint(1) NOT NULL,
    `ClientClaimsPrefix` varchar(200) CHARACTER SET utf8mb4 NULL,
    `PairWiseSubjectSalt` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NULL,
    `LastAccessed` datetime(6) NULL,
    `UserSsoLifetime` int NULL,
    `UserCodeType` varchar(100) CHARACTER SET utf8mb4 NULL,
    `DeviceCodeLifetime` int NOT NULL,
    `NonEditable` tinyint(1) NOT NULL,
    `CibaLifetime` int NULL,
    `PollingInterval` int NULL,
    `CoordinateLifetimeWithUserSession` tinyint(1) NULL,
    `InitiateLoginUri` longtext CHARACTER SET utf8mb4 NULL,
    `DPoPClockSkew` time(6) NOT NULL,
    `DPoPValidationMode` int NOT NULL,
    `RequireDPoP` tinyint(1) NOT NULL,
    `PushedAuthorizationLifetime` int NULL,
    `RequirePushedAuthorization` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Clients` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `IdentityProviders` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Scheme` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `DisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Enabled` tinyint(1) NOT NULL,
    `Type` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Properties` longtext CHARACTER SET utf8mb4 NULL,
    `Created` datetime(6) NOT NULL,
    `LastAccessed` datetime(6) NULL,
    `NonEditable` tinyint(1) NOT NULL,
    `Updated` datetime(6) NULL,
    CONSTRAINT `PK_IdentityProviders` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `IdentityResources` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Enabled` tinyint(1) NOT NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `DisplayName` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `Required` tinyint(1) NOT NULL,
    `Emphasize` tinyint(1) NOT NULL,
    `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NULL,
    `NonEditable` tinyint(1) NOT NULL,
    CONSTRAINT `PK_IdentityResources` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ApiResourceClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ApiResourceId` int NOT NULL,
    `Type` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ApiResourceClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ApiResourceClaims_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ApiResourceProperties` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ApiResourceId` int NOT NULL,
    `Key` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ApiResourceProperties` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ApiResourceProperties_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ApiResourceScopes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Scope` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ApiResourceId` int NOT NULL,
    CONSTRAINT `PK_ApiResourceScopes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ApiResourceScopes_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ApiResourceSecrets` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ApiResourceId` int NOT NULL,
    `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `Value` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
    `Expiration` datetime(6) NULL,
    `Type` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    CONSTRAINT `PK_ApiResourceSecrets` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ApiResourceSecrets_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ApiScopeClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ScopeId` int NOT NULL,
    `Type` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ApiScopeClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ApiScopeClaims_ApiScopes_ScopeId` FOREIGN KEY (`ScopeId`) REFERENCES `ApiScopes` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ApiScopeProperties` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ScopeId` int NOT NULL,
    `Key` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ApiScopeProperties` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ApiScopeProperties_ApiScopes_ScopeId` FOREIGN KEY (`ScopeId`) REFERENCES `ApiScopes` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Type` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` int NOT NULL,
    CONSTRAINT `PK_ClientClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientClaims_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientCorsOrigins` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Origin` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` int NOT NULL,
    CONSTRAINT `PK_ClientCorsOrigins` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientCorsOrigins_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientGrantTypes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `GrantType` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` int NOT NULL,
    CONSTRAINT `PK_ClientGrantTypes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientGrantTypes_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientIdPRestrictions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Provider` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` int NOT NULL,
    CONSTRAINT `PK_ClientIdPRestrictions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientIdPRestrictions_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientPostLogoutRedirectUris` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PostLogoutRedirectUri` varchar(400) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` int NOT NULL,
    CONSTRAINT `PK_ClientPostLogoutRedirectUris` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientPostLogoutRedirectUris_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientProperties` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ClientId` int NOT NULL,
    `Key` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ClientProperties` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientProperties_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientRedirectUris` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RedirectUri` varchar(400) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` int NOT NULL,
    CONSTRAINT `PK_ClientRedirectUris` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientRedirectUris_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientScopes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Scope` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` int NOT NULL,
    CONSTRAINT `PK_ClientScopes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientScopes_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ClientSecrets` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ClientId` int NOT NULL,
    `Description` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `Value` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
    `Expiration` datetime(6) NULL,
    `Type` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    CONSTRAINT `PK_ClientSecrets` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ClientSecrets_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `IdentityResourceClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `IdentityResourceId` int NOT NULL,
    `Type` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_IdentityResourceClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_IdentityResourceClaims_IdentityResources_IdentityResourceId` FOREIGN KEY (`IdentityResourceId`) REFERENCES `IdentityResources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `IdentityResourceProperties` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `IdentityResourceId` int NOT NULL,
    `Key` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_IdentityResourceProperties` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_IdentityResourceProperties_IdentityResources_IdentityResource` FOREIGN KEY (`IdentityResourceId`) REFERENCES `IdentityResources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_ApiResourceClaims_ApiResourceId_Type` ON `ApiResourceClaims` (`ApiResourceId`, `Type`);

CREATE UNIQUE INDEX `IX_ApiResourceProperties_ApiResourceId_Key` ON `ApiResourceProperties` (`ApiResourceId`, `Key`);

CREATE UNIQUE INDEX `IX_ApiResources_Name` ON `ApiResources` (`Name`);

CREATE UNIQUE INDEX `IX_ApiResourceScopes_ApiResourceId_Scope` ON `ApiResourceScopes` (`ApiResourceId`, `Scope`);

CREATE INDEX `IX_ApiResourceSecrets_ApiResourceId` ON `ApiResourceSecrets` (`ApiResourceId`);

CREATE UNIQUE INDEX `IX_ApiScopeClaims_ScopeId_Type` ON `ApiScopeClaims` (`ScopeId`, `Type`);

CREATE UNIQUE INDEX `IX_ApiScopeProperties_ScopeId_Key` ON `ApiScopeProperties` (`ScopeId`, `Key`);

CREATE UNIQUE INDEX `IX_ApiScopes_Name` ON `ApiScopes` (`Name`);

CREATE UNIQUE INDEX `IX_ClientClaims_ClientId_Type_Value` ON `ClientClaims` (`ClientId`, `Type`, `Value`);

CREATE UNIQUE INDEX `IX_ClientCorsOrigins_ClientId_Origin` ON `ClientCorsOrigins` (`ClientId`, `Origin`);

CREATE UNIQUE INDEX `IX_ClientGrantTypes_ClientId_GrantType` ON `ClientGrantTypes` (`ClientId`, `GrantType`);

CREATE UNIQUE INDEX `IX_ClientIdPRestrictions_ClientId_Provider` ON `ClientIdPRestrictions` (`ClientId`, `Provider`);

CREATE UNIQUE INDEX `IX_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri` ON `ClientPostLogoutRedirectUris` (`ClientId`, `PostLogoutRedirectUri`);

CREATE INDEX `IX_ClientProperties_ClientId` ON `ClientProperties` (`ClientId`);

CREATE UNIQUE INDEX `IX_ClientRedirectUris_ClientId_RedirectUri` ON `ClientRedirectUris` (`ClientId`, `RedirectUri`);

CREATE UNIQUE INDEX `IX_Clients_ClientId` ON `Clients` (`ClientId`);

CREATE INDEX `IX_ClientScopes_ClientId` ON `ClientScopes` (`ClientId`);

CREATE INDEX `IX_ClientSecrets_ClientId` ON `ClientSecrets` (`ClientId`);

CREATE UNIQUE INDEX `IX_IdentityResourceClaims_IdentityResourceId_Type` ON `IdentityResourceClaims` (`IdentityResourceId`, `Type`);

CREATE UNIQUE INDEX `IX_IdentityResourceProperties_IdentityResourceId_Key` ON `IdentityResourceProperties` (`IdentityResourceId`, `Key`);

CREATE UNIQUE INDEX `IX_IdentityResources_Name` ON `IdentityResources` (`Name`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260529102506_Configuration', '9.0.16');

COMMIT;

