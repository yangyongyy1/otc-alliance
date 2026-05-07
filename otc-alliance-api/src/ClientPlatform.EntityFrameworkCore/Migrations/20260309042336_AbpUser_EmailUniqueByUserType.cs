using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AbpUser_EmailUniqueByUserType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_TenantId_NormalizedEmailAddress",
                table: "AbpUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_TenantId_NormalizedEmailAddress_UserType",
                table: "AbpUsers",
                columns: new[] { "TenantId", "NormalizedEmailAddress", "UserType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_TenantId_NormalizedEmailAddress_UserType",
                table: "AbpUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_TenantId_NormalizedEmailAddress",
                table: "AbpUsers",
                columns: new[] { "TenantId", "NormalizedEmailAddress" });
        }
    }
}
