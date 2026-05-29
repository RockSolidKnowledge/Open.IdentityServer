START TRANSACTION;
ALTER TABLE `IdentityResourceProperties` DROP INDEX `IX_IdentityResourceProperties_IdentityResourceId`;

ALTER TABLE `IdentityResourceClaims` DROP INDEX `IX_IdentityResourceClaims_IdentityResourceId`;

ALTER TABLE `ClientRedirectUris` DROP INDEX `IX_ClientRedirectUris_ClientId`;

ALTER TABLE `ClientPostLogoutRedirectUris` DROP INDEX `IX_ClientPostLogoutRedirectUris_ClientId`;

ALTER TABLE `ClientIdPRestrictions` DROP INDEX `IX_ClientIdPRestrictions_ClientId`;

ALTER TABLE `ClientGrantTypes` DROP INDEX `IX_ClientGrantTypes_ClientId`;

ALTER TABLE `ClientCorsOrigins` DROP INDEX `IX_ClientCorsOrigins_ClientId`;

ALTER TABLE `ClientClaims` DROP INDEX `IX_ClientClaims_ClientId`;

ALTER TABLE `ApiScopeProperties` DROP INDEX `IX_ApiScopeProperties_ScopeId`;

ALTER TABLE `ApiScopeClaims` DROP INDEX `IX_ApiScopeClaims_ScopeId`;

ALTER TABLE `ApiResourceScopes` DROP INDEX `IX_ApiResourceScopes_ApiResourceId`;

ALTER TABLE `ApiResourceProperties` DROP INDEX `IX_ApiResourceProperties_ApiResourceId`;

ALTER TABLE `ApiResourceClaims` DROP INDEX `IX_ApiResourceClaims_ApiResourceId`;

ALTER TABLE `Clients` ADD `CibaLifetime` int NULL;

ALTER TABLE `Clients` ADD `CoordinateLifetimeWithUserSession` tinyint(1) NULL;

ALTER TABLE `Clients` ADD `DPoPClockSkew` time(6) NOT NULL DEFAULT '00:00:00';

ALTER TABLE `Clients` ADD `DPoPValidationMode` int NOT NULL DEFAULT 0;

ALTER TABLE `Clients` ADD `InitiateLoginUri` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Clients` ADD `PollingInterval` int NULL;

ALTER TABLE `Clients` ADD `PushedAuthorizationLifetime` int NULL;

ALTER TABLE `Clients` ADD `RequireDPoP` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `Clients` ADD `RequirePushedAuthorization` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `ClientRedirectUris` MODIFY COLUMN `RedirectUri` varchar(400) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `ClientPostLogoutRedirectUris` MODIFY COLUMN `PostLogoutRedirectUri` varchar(400) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `ApiScopes` ADD `Created` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE `ApiScopes` ADD `LastAccessed` datetime(6) NULL;

ALTER TABLE `ApiScopes` ADD `NonEditable` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `ApiScopes` ADD `Updated` datetime(6) NULL;

ALTER TABLE `ApiResources` ADD `RequireResourceIndicator` tinyint(1) NOT NULL DEFAULT FALSE;

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

CREATE UNIQUE INDEX `IX_IdentityResourceProperties_IdentityResourceId_Key` ON `IdentityResourceProperties` (`IdentityResourceId`, `Key`);

CREATE UNIQUE INDEX `IX_IdentityResourceClaims_IdentityResourceId_Type` ON `IdentityResourceClaims` (`IdentityResourceId`, `Type`);

CREATE UNIQUE INDEX `IX_ClientRedirectUris_ClientId_RedirectUri` ON `ClientRedirectUris` (`ClientId`, `RedirectUri`);

CREATE UNIQUE INDEX `IX_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri` ON `ClientPostLogoutRedirectUris` (`ClientId`, `PostLogoutRedirectUri`);

CREATE UNIQUE INDEX `IX_ClientIdPRestrictions_ClientId_Provider` ON `ClientIdPRestrictions` (`ClientId`, `Provider`);

CREATE UNIQUE INDEX `IX_ClientGrantTypes_ClientId_GrantType` ON `ClientGrantTypes` (`ClientId`, `GrantType`);

CREATE UNIQUE INDEX `IX_ClientCorsOrigins_ClientId_Origin` ON `ClientCorsOrigins` (`ClientId`, `Origin`);

CREATE UNIQUE INDEX `IX_ClientClaims_ClientId_Type_Value` ON `ClientClaims` (`ClientId`, `Type`, `Value`);

CREATE UNIQUE INDEX `IX_ApiScopeProperties_ScopeId_Key` ON `ApiScopeProperties` (`ScopeId`, `Key`);

CREATE UNIQUE INDEX `IX_ApiScopeClaims_ScopeId_Type` ON `ApiScopeClaims` (`ScopeId`, `Type`);

CREATE UNIQUE INDEX `IX_ApiResourceScopes_ApiResourceId_Scope` ON `ApiResourceScopes` (`ApiResourceId`, `Scope`);

CREATE UNIQUE INDEX `IX_ApiResourceProperties_ApiResourceId_Key` ON `ApiResourceProperties` (`ApiResourceId`, `Key`);

CREATE UNIQUE INDEX `IX_ApiResourceClaims_ApiResourceId_Type` ON `ApiResourceClaims` (`ApiResourceId`, `Type`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260529101748_Configuration_to_OpenIdS', '9.0.16');

COMMIT;

