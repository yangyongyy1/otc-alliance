-- Migration: AddAuthStandardToMerchant (20260309083748)
-- 商户表增加认证标准字段（可空），枚举 AuthStandardLevel

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309083748_AddAuthStandardToMerchant') THEN
        ALTER TABLE "Merchants" ADD "AuthStandard" integer NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309083748_AddAuthStandardToMerchant') THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260309083748_AddAuthStandardToMerchant', '9.0.5');
    END IF;
END $EF$;

COMMIT;
