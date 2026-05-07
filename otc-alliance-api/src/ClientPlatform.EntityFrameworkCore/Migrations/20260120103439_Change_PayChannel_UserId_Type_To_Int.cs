using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Change_PayChannel_UserId_Type_To_Int : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 删除外键约束
            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelCustomers_ClientUsers_UserId",
                table: "PayChannelCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelAccounts_ClientUsers_UserId",
                table: "PayChannelAccounts");

            // 2. 修改列类型从 bigint 改为 integer
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

            // 3. 重新创建外键约束
            migrationBuilder.AddForeignKey(
                name: "FK_PayChannelCustomers_ClientUsers_UserId",
                table: "PayChannelCustomers",
                column: "UserId",
                principalTable: "ClientUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
            // 回滚：删除外键约束
            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelCustomers_ClientUsers_UserId",
                table: "PayChannelCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_PayChannelAccounts_ClientUsers_UserId",
                table: "PayChannelAccounts");

            // 回滚：修改列类型从 integer 改回 bigint
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

            // 回滚：重新创建外键约束（指向 AbpUsers）
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
