using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class _10013 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppKycApplicants_AbpUsers_UserId",
                table: "AppKycApplicants");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AppKycApplicants",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_AppKycApplicants_ClientUsers_UserId",
                table: "AppKycApplicants",
                column: "UserId",
                principalTable: "ClientUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppKycApplicants_ClientUsers_UserId",
                table: "AppKycApplicants");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "AppKycApplicants",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AppKycApplicants_AbpUsers_UserId",
                table: "AppKycApplicants",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
