using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_Status_To_Enum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manual SQL for Data Conversion
            migrationBuilder.Sql(@"
                ALTER TABLE ""PayChannelCustomers"" 
                ALTER COLUMN ""Status"" TYPE integer 
                USING CASE 
                    WHEN upper(""Status"") = 'ACTIVE' THEN 1 
                    WHEN upper(""Status"") = 'FROZEN' THEN 2 
                    WHEN upper(""Status"") = 'DISABLE' OR upper(""Status"") = 'DISABLED' THEN 3 
                    WHEN upper(""Status"") = 'FAILED' THEN 4 
                    ELSE 0 
                END;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""PayChannelCustomers"" 
                ALTER COLUMN ""EntityType"" TYPE integer 
                USING CASE 
                    WHEN upper(""EntityType"") = 'COMPANY' THEN 1 
                    ELSE 0 
                END;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""PayChannelAccounts"" 
                ALTER COLUMN ""Status"" TYPE integer 
                USING CASE 
                    WHEN upper(""Status"") = 'ACTIVE' THEN 1 
                    WHEN upper(""Status"") = 'FAILED' THEN 2 
                    WHEN upper(""Status"") = 'SUSPENDED' THEN 3 
                    WHEN upper(""Status"") = 'CLOSED' THEN 4 
                    ELSE 0 
                END;
            ");

            // EF Core AlterColumn to update metadata/constraints
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PayChannelCustomers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityType",
                table: "PayChannelCustomers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PayChannelAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PayChannelCustomers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "PayChannelCustomers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PayChannelAccounts",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
