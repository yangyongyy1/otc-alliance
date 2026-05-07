using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthStandardToMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthStandard",
                table: "Merchants",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthStandard",
                table: "Merchants");
        }
    }
}
