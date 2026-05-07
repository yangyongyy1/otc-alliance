START TRANSACTION;
ALTER TABLE "AppKycApplicants" ADD "IsClosed" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260317083443_AddIsClosedToKycApplicant', '9.0.5');

COMMIT;

