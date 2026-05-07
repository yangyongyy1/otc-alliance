using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddMiddleNameToKycApplicant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FixedMiddleName",
                table: "AppKycApplicants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FixedMiddleNameEn",
                table: "AppKycApplicants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "AppKycApplicants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleNameEn",
                table: "AppKycApplicants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedMiddleName",
                table: "AppKycApplicants");

            migrationBuilder.DropColumn(
                name: "FixedMiddleNameEn",
                table: "AppKycApplicants");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "AppKycApplicants");

            migrationBuilder.DropColumn(
                name: "MiddleNameEn",
                table: "AppKycApplicants");
        }
    }
}
