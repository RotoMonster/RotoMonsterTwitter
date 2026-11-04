START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TweetPlayers" DROP CONSTRAINT "FK_TweetPlayers_TwitterPlayers_PlayerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TweetTeams" DROP CONSTRAINT "FK_TweetTeams_TwitterTeams_TeamId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterPlayerAliases" DROP CONSTRAINT "FK_TwitterPlayerAliases_TwitterPlayers_PlayerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterTeamAliases" DROP CONSTRAINT "FK_TwitterTeamAliases_TwitterTeams_TeamId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterTeams" DROP CONSTRAINT "PK_TwitterTeams";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    DROP INDEX "IX_TwitterTeamAliases_TeamId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterPlayers" DROP CONSTRAINT "PK_TwitterPlayers";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    DROP INDEX "IX_TwitterPlayerAliases_PlayerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    DROP INDEX "IX_TweetTeams_TeamId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    DROP INDEX "IX_TweetPlayers_PlayerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterTeamAliases" ADD "SportId" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterPlayerAliases" ADD "SportId" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TweetTeams" ADD "SportId" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TweetPlayers" ADD "SportId" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN

    UPDATE "TwitterPlayerAliases" a SET "SportId" = p."SportId" FROM "TwitterPlayers" p WHERE p."PlayerId" = a."PlayerId";
    UPDATE "TwitterTeamAliases" a SET "SportId" = t."SportId" FROM "TwitterTeams" t WHERE t."TeamId" = a."TeamId";
    UPDATE "TweetPlayers" x SET "SportId" = w."SportId" FROM "Tweets" w WHERE w."TweetId" = x."TweetId";
    UPDATE "TweetTeams" x SET "SportId" = w."SportId" FROM "Tweets" w WHERE w."TweetId" = x."TweetId";
    UPDATE "TwitterPlayers" SET "PlayerId" = "PlayerId" - 30000000 WHERE "SportId" = 3 AND "PlayerId" >= 30000000;
    UPDATE "TwitterPlayers" SET "TeamId" = "TeamId" - 30000000 WHERE "SportId" = 3 AND "TeamId" >= 30000000;
    UPDATE "TwitterTeams" SET "TeamId" = "TeamId" - 30000000 WHERE "SportId" = 3 AND "TeamId" >= 30000000;
    UPDATE "TwitterPlayerAliases" SET "PlayerId" = "PlayerId" - 30000000 WHERE "SportId" = 3 AND "PlayerId" >= 30000000;
    UPDATE "TwitterTeamAliases" SET "TeamId" = "TeamId" - 30000000 WHERE "SportId" = 3 AND "TeamId" >= 30000000;
    UPDATE "TweetPlayers" SET "PlayerId" = "PlayerId" - 30000000 WHERE "SportId" = 3 AND "PlayerId" >= 30000000;
    UPDATE "TweetTeams" SET "TeamId" = "TeamId" - 30000000 WHERE "SportId" = 3 AND "TeamId" >= 30000000;

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterTeams" ADD CONSTRAINT "PK_TwitterTeams" PRIMARY KEY ("SportId", "TeamId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterPlayers" ADD CONSTRAINT "PK_TwitterPlayers" PRIMARY KEY ("SportId", "PlayerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    CREATE INDEX "IX_TwitterTeamAliases_SportId_TeamId" ON "TwitterTeamAliases" ("SportId", "TeamId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    CREATE INDEX "IX_TwitterPlayerAliases_SportId_PlayerId" ON "TwitterPlayerAliases" ("SportId", "PlayerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    CREATE INDEX "IX_TweetTeams_SportId_TeamId" ON "TweetTeams" ("SportId", "TeamId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    CREATE INDEX "IX_TweetPlayers_SportId_PlayerId" ON "TweetPlayers" ("SportId", "PlayerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterPlayerAliases" ADD CONSTRAINT "FK_TwitterPlayerAliases_TwitterPlayers_SportId_PlayerId" FOREIGN KEY ("SportId", "PlayerId") REFERENCES "TwitterPlayers" ("SportId", "PlayerId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    ALTER TABLE "TwitterTeamAliases" ADD CONSTRAINT "FK_TwitterTeamAliases_TwitterTeams_SportId_TeamId" FOREIGN KEY ("SportId", "TeamId") REFERENCES "TwitterTeams" ("SportId", "TeamId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261103224558_PlayerTeamSportKeys') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261103224558_PlayerTeamSportKeys', '10.0.10');
    END IF;
END $EF$;
COMMIT;

