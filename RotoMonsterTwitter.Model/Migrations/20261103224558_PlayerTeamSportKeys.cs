using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonsterTwitter.Model.Migrations
{
    /// <inheritdoc />
    public partial class PlayerTeamSportKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetPlayers_TwitterPlayers_PlayerId",
                table: "TweetPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_TweetTeams_TwitterTeams_TeamId",
                table: "TweetTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_TwitterPlayerAliases_TwitterPlayers_PlayerId",
                table: "TwitterPlayerAliases");

            migrationBuilder.DropForeignKey(
                name: "FK_TwitterTeamAliases_TwitterTeams_TeamId",
                table: "TwitterTeamAliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TwitterTeams",
                table: "TwitterTeams");

            migrationBuilder.DropIndex(
                name: "IX_TwitterTeamAliases_TeamId",
                table: "TwitterTeamAliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TwitterPlayers",
                table: "TwitterPlayers");

            migrationBuilder.DropIndex(
                name: "IX_TwitterPlayerAliases_PlayerId",
                table: "TwitterPlayerAliases");

            migrationBuilder.DropIndex(
                name: "IX_TweetTeams_TeamId",
                table: "TweetTeams");

            migrationBuilder.DropIndex(
                name: "IX_TweetPlayers_PlayerId",
                table: "TweetPlayers");

            migrationBuilder.AddColumn<int>(
                name: "SportId",
                table: "TwitterTeamAliases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SportId",
                table: "TwitterPlayerAliases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SportId",
                table: "TweetTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SportId",
                table: "TweetPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE ""TwitterPlayerAliases"" a SET ""SportId"" = p.""SportId"" FROM ""TwitterPlayers"" p WHERE p.""PlayerId"" = a.""PlayerId"";
UPDATE ""TwitterTeamAliases"" a SET ""SportId"" = t.""SportId"" FROM ""TwitterTeams"" t WHERE t.""TeamId"" = a.""TeamId"";
UPDATE ""TweetPlayers"" x SET ""SportId"" = w.""SportId"" FROM ""Tweets"" w WHERE w.""TweetId"" = x.""TweetId"";
UPDATE ""TweetTeams"" x SET ""SportId"" = w.""SportId"" FROM ""Tweets"" w WHERE w.""TweetId"" = x.""TweetId"";
UPDATE ""TwitterPlayers"" SET ""PlayerId"" = ""PlayerId"" - 30000000 WHERE ""SportId"" = 3 AND ""PlayerId"" >= 30000000;
UPDATE ""TwitterPlayers"" SET ""TeamId"" = ""TeamId"" - 30000000 WHERE ""SportId"" = 3 AND ""TeamId"" >= 30000000;
UPDATE ""TwitterTeams"" SET ""TeamId"" = ""TeamId"" - 30000000 WHERE ""SportId"" = 3 AND ""TeamId"" >= 30000000;
UPDATE ""TwitterPlayerAliases"" SET ""PlayerId"" = ""PlayerId"" - 30000000 WHERE ""SportId"" = 3 AND ""PlayerId"" >= 30000000;
UPDATE ""TwitterTeamAliases"" SET ""TeamId"" = ""TeamId"" - 30000000 WHERE ""SportId"" = 3 AND ""TeamId"" >= 30000000;
UPDATE ""TweetPlayers"" SET ""PlayerId"" = ""PlayerId"" - 30000000 WHERE ""SportId"" = 3 AND ""PlayerId"" >= 30000000;
UPDATE ""TweetTeams"" SET ""TeamId"" = ""TeamId"" - 30000000 WHERE ""SportId"" = 3 AND ""TeamId"" >= 30000000;
");


            migrationBuilder.AddPrimaryKey(
                name: "PK_TwitterTeams",
                table: "TwitterTeams",
                columns: new[] { "SportId", "TeamId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TwitterPlayers",
                table: "TwitterPlayers",
                columns: new[] { "SportId", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_TwitterTeamAliases_SportId_TeamId",
                table: "TwitterTeamAliases",
                columns: new[] { "SportId", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_TwitterPlayerAliases_SportId_PlayerId",
                table: "TwitterPlayerAliases",
                columns: new[] { "SportId", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_TweetTeams_SportId_TeamId",
                table: "TweetTeams",
                columns: new[] { "SportId", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_TweetPlayers_SportId_PlayerId",
                table: "TweetPlayers",
                columns: new[] { "SportId", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TwitterPlayerAliases_TwitterPlayers_SportId_PlayerId",
                table: "TwitterPlayerAliases",
                columns: new[] { "SportId", "PlayerId" },
                principalTable: "TwitterPlayers",
                principalColumns: new[] { "SportId", "PlayerId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TwitterTeamAliases_TwitterTeams_SportId_TeamId",
                table: "TwitterTeamAliases",
                columns: new[] { "SportId", "TeamId" },
                principalTable: "TwitterTeams",
                principalColumns: new[] { "SportId", "TeamId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TwitterPlayerAliases_TwitterPlayers_SportId_PlayerId",
                table: "TwitterPlayerAliases");

            migrationBuilder.DropForeignKey(
                name: "FK_TwitterTeamAliases_TwitterTeams_SportId_TeamId",
                table: "TwitterTeamAliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TwitterTeams",
                table: "TwitterTeams");

            migrationBuilder.DropIndex(
                name: "IX_TwitterTeamAliases_SportId_TeamId",
                table: "TwitterTeamAliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TwitterPlayers",
                table: "TwitterPlayers");

            migrationBuilder.DropIndex(
                name: "IX_TwitterPlayerAliases_SportId_PlayerId",
                table: "TwitterPlayerAliases");

            migrationBuilder.DropIndex(
                name: "IX_TweetTeams_SportId_TeamId",
                table: "TweetTeams");

            migrationBuilder.DropIndex(
                name: "IX_TweetPlayers_SportId_PlayerId",
                table: "TweetPlayers");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "TwitterTeamAliases");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "TwitterPlayerAliases");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "TweetTeams");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "TweetPlayers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TwitterTeams",
                table: "TwitterTeams",
                column: "TeamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TwitterPlayers",
                table: "TwitterPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TwitterTeamAliases_TeamId",
                table: "TwitterTeamAliases",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TwitterPlayerAliases_PlayerId",
                table: "TwitterPlayerAliases",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TweetTeams_TeamId",
                table: "TweetTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TweetPlayers_PlayerId",
                table: "TweetPlayers",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetPlayers_TwitterPlayers_PlayerId",
                table: "TweetPlayers",
                column: "PlayerId",
                principalTable: "TwitterPlayers",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TweetTeams_TwitterTeams_TeamId",
                table: "TweetTeams",
                column: "TeamId",
                principalTable: "TwitterTeams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TwitterPlayerAliases_TwitterPlayers_PlayerId",
                table: "TwitterPlayerAliases",
                column: "PlayerId",
                principalTable: "TwitterPlayers",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TwitterTeamAliases_TwitterTeams_TeamId",
                table: "TwitterTeamAliases",
                column: "TeamId",
                principalTable: "TwitterTeams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
