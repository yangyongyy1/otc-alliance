using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class RenameOpenCloseToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenClose",
                table: "MerchantChannelCurrencies",
                newName: "Status");

            migrationBuilder.AddColumn<DateTime>(
                name: "BalanceTime",
                table: "MerchantChannelCurrencies",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceTime",
                table: "MerchantChannelCurrencies");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "MerchantChannelCurrencies",
                newName: "OpenClose");
        }
    }
}
