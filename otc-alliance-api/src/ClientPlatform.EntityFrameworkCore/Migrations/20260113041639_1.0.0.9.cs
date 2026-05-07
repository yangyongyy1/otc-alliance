using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class _1009 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OpenClose",
                table: "MerchantChannelCurrencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PayChannelCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelProvider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ChannelCustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RawData = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayChannelCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayChannelCustomers_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayChannelAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelProvider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ChannelAccountId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BankName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    RawData = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayChannelAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayChannelAccounts_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayChannelAccounts_PayChannelCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "PayChannelCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelAccounts_CustomerId",
                table: "PayChannelAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelAccounts_UserId",
                table: "PayChannelAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelCustomers_UserId",
                table: "PayChannelCustomers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayChannelAccounts");

            migrationBuilder.DropTable(
                name: "PayChannelCustomers");

            migrationBuilder.DropColumn(
                name: "OpenClose",
                table: "MerchantChannelCurrencies");
        }
    }
}
