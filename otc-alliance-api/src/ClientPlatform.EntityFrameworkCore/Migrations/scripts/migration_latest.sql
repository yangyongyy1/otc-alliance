START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304040834_AddProductNameToAlliance') THEN
    ALTER TABLE "Alliances" ADD "ProductName" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304040834_AddProductNameToAlliance') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260304040834_AddProductNameToAlliance', '9.0.5');
    END IF;
END $EF$;
COMMIT;

