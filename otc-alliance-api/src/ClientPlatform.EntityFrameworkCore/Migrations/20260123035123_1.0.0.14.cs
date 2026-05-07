using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class _10014 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CopyrightInfo",
                table: "Alliances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Disclaimer",
                table: "Alliances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceEmail",
                table: "Alliances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "Alliances",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebSiteIcon",
                table: "Alliances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebSiteUrl",
                table: "Alliances",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CopyrightInfo",
                table: "Alliances");

            migrationBuilder.DropColumn(
                name: "Disclaimer",
                table: "Alliances");

            migrationBuilder.DropColumn(
                name: "ServiceEmail",
                table: "Alliances");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "Alliances");

            migrationBuilder.DropColumn(
                name: "WebSiteIcon",
                table: "Alliances");

            migrationBuilder.DropColumn(
                name: "WebSiteUrl",
                table: "Alliances");
        }
    }
}
