using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class RevertFieldNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "MerchantChannelCurrencies",
                newName: "OpenClose");

            migrationBuilder.AddColumn<DateTime>(
                name: "BalanceTime",
                table: "MerchantChannelCurrencies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChannelCode",
                table: "MerchantChannelCurrencies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceTime",
                table: "MerchantChannelCurrencies");

            migrationBuilder.DropColumn(
                name: "ChannelCode",
                table: "MerchantChannelCurrencies");

            migrationBuilder.RenameColumn(
                name: "OpenClose",
                table: "MerchantChannelCurrencies",
                newName: "Status");
        }
    }
}
