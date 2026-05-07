using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddCallbackToCollectionOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Callback",
                table: "CollectionOrders",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Callback",
                table: "CollectionOrders");
        }
    }
}
