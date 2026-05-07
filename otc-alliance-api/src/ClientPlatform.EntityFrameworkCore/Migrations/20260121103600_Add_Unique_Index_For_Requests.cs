using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_Index_For_Requests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayChannelServiceRequests_UserId",
                table: "PayChannelServiceRequests");

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelServiceRequests_UserId_ChannelProvider_Currency",
                table: "PayChannelServiceRequests",
                columns: new[] { "UserId", "ChannelProvider", "Currency" },
                unique: true,
                filter: "\"Status\" NOT IN (4, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayChannelServiceRequests_UserId_ChannelProvider_Currency",
                table: "PayChannelServiceRequests");

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelServiceRequests_UserId",
                table: "PayChannelServiceRequests",
                column: "UserId");
        }
    }
}
