using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class _10012 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VAAccountIdentityDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VAAccountIdentityId = table.Column<int>(type: "integer", nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccountHolderType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccountHolderAddressLine1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccountHolderAddressLine2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccountHolderCity = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccountHolderState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountHolderPostalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AccountHolderCountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    BankName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankBranchName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankAddressLine1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankAddressLine2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankCity = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankPostalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BankCountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    SwiftBic = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    IntermediaryBankName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IntermediaryBankSwift = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    AchRoutingNumber = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    AbaRoutingNumber = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    FedwireRoutingNumber = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    WireRoutingNumber = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    AccountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WireMemo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AchCompanyId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AchStandardEntryClass = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    SortCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    BsbCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    Clabe = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    BankCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BranchCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Extra = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_VAAccountIdentityDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VAAccountIdentityDetails_VAAccountIdentities_VAAccountIdent~",
                        column: x => x.VAAccountIdentityId,
                        principalTable: "VAAccountIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VAAccountIdentityDetails_VAAccountIdentityId",
                table: "VAAccountIdentityDetails",
                column: "VAAccountIdentityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VAAccountIdentityDetails");
        }
    }
}
