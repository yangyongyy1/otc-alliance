using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class _1005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "City",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "DocumentPhotoBackUrl",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "DocumentPhotoFrontUrl",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "VAAccountIdentities");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "VAAccountIdentities");

            migrationBuilder.RenameColumn(
                name: "Extra",
                table: "VAAccountIdentities",
                newName: "FormJson");

            migrationBuilder.RenameColumn(
                name: "DocumentType",
                table: "VAAccountIdentities",
                newName: "AccountUserType");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "VAAccountIdentities",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "MerchantId",
                table: "VAAccountIdentities",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FormJson",
                table: "VAAccountIdentities",
                newName: "Extra");

            migrationBuilder.RenameColumn(
                name: "AccountUserType",
                table: "VAAccountIdentities",
                newName: "DocumentType");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "VAAccountIdentities",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MerchantId",
                table: "VAAccountIdentities",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "VAAccountIdentities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "VAAccountIdentities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "VAAccountIdentities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentPhotoBackUrl",
                table: "VAAccountIdentities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPhotoFrontUrl",
                table: "VAAccountIdentities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "VAAccountIdentities",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "VAAccountIdentities",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "VAAccountIdentities",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "VAAccountIdentities",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "VAAccountIdentities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
