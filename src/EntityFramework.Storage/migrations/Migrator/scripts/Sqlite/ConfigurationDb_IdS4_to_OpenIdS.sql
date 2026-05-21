BEGIN TRANSACTION;
DROP INDEX "IX_IdentityResourceProperties_IdentityResourceId";

DROP INDEX "IX_IdentityResourceClaims_IdentityResourceId";

DROP INDEX "IX_ClientRedirectUris_ClientId";

DROP INDEX "IX_ClientPostLogoutRedirectUris_ClientId";

DROP INDEX "IX_ClientIdPRestrictions_ClientId";

DROP INDEX "IX_ClientGrantTypes_ClientId";

DROP INDEX "IX_ClientCorsOrigins_ClientId";

DROP INDEX "IX_ClientClaims_ClientId";

DROP INDEX "IX_ApiScopeProperties_ScopeId";

DROP INDEX "IX_ApiScopeClaims_ScopeId";

DROP INDEX "IX_ApiResourceScopes_ApiResourceId";

DROP INDEX "IX_ApiResourceProperties_ApiResourceId";

DROP INDEX "IX_ApiResourceClaims_ApiResourceId";

ALTER TABLE "Clients" ADD "CibaLifetime" INTEGER NULL;

ALTER TABLE "Clients" ADD "CoordinateLifetimeWithUserSession" INTEGER NULL;

ALTER TABLE "Clients" ADD "DPoPClockSkew" TEXT NOT NULL DEFAULT '00:00:00';

ALTER TABLE "Clients" ADD "DPoPValidationMode" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Clients" ADD "InitiateLoginUri" TEXT NULL;

ALTER TABLE "Clients" ADD "PollingInterval" INTEGER NULL;

ALTER TABLE "Clients" ADD "PushedAuthorizationLifetime" INTEGER NULL;

ALTER TABLE "Clients" ADD "RequireDPoP" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Clients" ADD "RequirePushedAuthorization" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "ApiScopes" ADD "Created" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE "ApiScopes" ADD "LastAccessed" TEXT NULL;

ALTER TABLE "ApiScopes" ADD "NonEditable" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "ApiScopes" ADD "Updated" TEXT NULL;

ALTER TABLE "ApiResources" ADD "RequireResourceIndicator" INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "IdentityProviders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IdentityProviders" PRIMARY KEY AUTOINCREMENT,
    "Scheme" TEXT NOT NULL,
    "DisplayName" TEXT NULL,
    "Enabled" INTEGER NOT NULL,
    "Type" TEXT NOT NULL,
    "Properties" TEXT NULL,
    "Created" TEXT NOT NULL,
    "LastAccessed" TEXT NULL,
    "NonEditable" INTEGER NOT NULL,
    "Updated" TEXT NULL
);

CREATE UNIQUE INDEX "IX_IdentityResourceProperties_IdentityResourceId_Key" ON "IdentityResourceProperties" ("IdentityResourceId", "Key");

CREATE UNIQUE INDEX "IX_IdentityResourceClaims_IdentityResourceId_Type" ON "IdentityResourceClaims" ("IdentityResourceId", "Type");

CREATE UNIQUE INDEX "IX_ClientRedirectUris_ClientId_RedirectUri" ON "ClientRedirectUris" ("ClientId", "RedirectUri");

CREATE UNIQUE INDEX "IX_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri" ON "ClientPostLogoutRedirectUris" ("ClientId", "PostLogoutRedirectUri");

CREATE UNIQUE INDEX "IX_ClientIdPRestrictions_ClientId_Provider" ON "ClientIdPRestrictions" ("ClientId", "Provider");

CREATE UNIQUE INDEX "IX_ClientGrantTypes_ClientId_GrantType" ON "ClientGrantTypes" ("ClientId", "GrantType");

CREATE UNIQUE INDEX "IX_ClientCorsOrigins_ClientId_Origin" ON "ClientCorsOrigins" ("ClientId", "Origin");

CREATE UNIQUE INDEX "IX_ClientClaims_ClientId_Type_Value" ON "ClientClaims" ("ClientId", "Type", "Value");

CREATE UNIQUE INDEX "IX_ApiScopeProperties_ScopeId_Key" ON "ApiScopeProperties" ("ScopeId", "Key");

CREATE UNIQUE INDEX "IX_ApiScopeClaims_ScopeId_Type" ON "ApiScopeClaims" ("ScopeId", "Type");

CREATE UNIQUE INDEX "IX_ApiResourceScopes_ApiResourceId_Scope" ON "ApiResourceScopes" ("ApiResourceId", "Scope");

CREATE UNIQUE INDEX "IX_ApiResourceProperties_ApiResourceId_Key" ON "ApiResourceProperties" ("ApiResourceId", "Key");

CREATE UNIQUE INDEX "IX_ApiResourceClaims_ApiResourceId_Type" ON "ApiResourceClaims" ("ApiResourceId", "Type");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260521135324_Configuration_to_OpenIdS', '10.0.8');

COMMIT;

