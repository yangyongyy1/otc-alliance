using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddCallbackToCollectionOrder2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Callback",
                table: "CollectionOrders");

            migrationBuilder.AddColumn<string>(
                name: "Callback",
                table: "CollectionOrderDetails",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Callback",
                table: "CollectionOrderDetails");

            migrationBuilder.AddColumn<string>(
                name: "Callback",
                table: "CollectionOrders",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }
    }
}
