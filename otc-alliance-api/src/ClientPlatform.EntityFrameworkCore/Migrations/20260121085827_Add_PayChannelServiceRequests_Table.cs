using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Add_PayChannelServiceRequests_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelAccounts_AbpUsers_UserId",
                table: "PayChannelAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelCustomers_AbpUsers_UserId",
                table: "PayChannelCustomers");

            migrationBuilder.DropColumn(
                name: "KycLevelCompleted",
                table: "AbpUsers");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PayChannelCustomers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PayChannelAccounts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "PayChannelServiceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ChannelProvider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestPayload = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailStep = table.Column<int>(type: "integer", nullable: false),
                    FailReason = table.Column<string>(type: "text", nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AccountId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerResponse = table.Column<string>(type: "text", nullable: true),
                    AccountResponse = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PayChannelServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayChannelServiceRequests_ClientUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ClientUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelServiceRequests_UserId",
                table: "PayChannelServiceRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelAccounts_ClientUsers_UserId",
                table: "PayChannelAccounts",
                column: "UserId",
                principalTable: "ClientUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelCustomers_ClientUsers_UserId",
                table: "PayChannelCustomers",
                column: "UserId",
                principalTable: "ClientUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelAccounts_ClientUsers_UserId",
                table: "PayChannelAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelCustomers_ClientUsers_UserId",
                table: "PayChannelCustomers");

            migrationBuilder.DropTable(
                name: "PayChannelServiceRequests");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "PayChannelCustomers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "PayChannelAccounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "KycLevelCompleted",
                table: "AbpUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelAccounts_AbpUsers_UserId",
                table: "PayChannelAccounts",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelCustomers_AbpUsers_UserId",
                table: "PayChannelCustomers",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
