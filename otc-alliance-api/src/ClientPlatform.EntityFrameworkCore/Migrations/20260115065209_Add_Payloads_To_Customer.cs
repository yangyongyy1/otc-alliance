using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Add_Payloads_To_Customer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallbackPayload",
                table: "PayChannelCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestPayload",
                table: "PayChannelCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsePayload",
                table: "PayChannelCustomers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackPayload",
                table: "PayChannelCustomers");

            migrationBuilder.DropColumn(
                name: "RequestPayload",
                table: "PayChannelCustomers");

            migrationBuilder.DropColumn(
                name: "ResponsePayload",
                table: "PayChannelCustomers");
        }
    }
}
