using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Update_PayChannel_FK_To_ClientUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 删除 PayChannelCustomers 表的旧外键约束 (指向 AbpUsers)
            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelCustomers_AbpUsers_UserId",
                table: "PayChannelCustomers");

            // 2. 删除 PayChannelAccounts 表的旧外键约束 (指向 AbpUsers)
            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelAccounts_AbpUsers_UserId",
                table: "PayChannelAccounts");

            // 3. 为 PayChannelCustomers 创建新的外键约束 (指向 ClientUsers)
            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelCustomers_ClientUsers_UserId",
                table: "PayChannelCustomers",
                column: "UserId",
                principalTable: "ClientUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 4. 为 PayChannelAccounts 创建新的外键约束 (指向 ClientUsers)
            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelAccounts_ClientUsers_UserId",
                table: "PayChannelAccounts",
                column: "UserId",
                principalTable: "ClientUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 回滚：删除新的外键约束
            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelCustomers_ClientUsers_UserId",
                table: "PayChannelCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelAccounts_ClientUsers_UserId",
                table: "PayChannelAccounts");

            // 回滚：恢复旧的外键约束
            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelCustomers_AbpUsers_UserId",
                table: "PayChannelCustomers",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelAccounts_AbpUsers_UserId",
                table: "PayChannelAccounts",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
