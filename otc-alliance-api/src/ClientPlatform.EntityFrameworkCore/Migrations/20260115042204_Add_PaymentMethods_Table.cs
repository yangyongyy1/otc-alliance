using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Add_PaymentMethods_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayChannelAccountPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AccountRoutingNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Memo = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SwiftBic = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IntermediarySwiftBic = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstitutionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstitutionAddressLine1 = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InstitutionCity = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstitutionState = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstitutionPostalCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstitutionCountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    HolderAddressLine1 = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HolderCity = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    HolderState = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    HolderPostalCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    HolderCountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    table.PrimaryKey("PK_PayChannelAccountPaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayChannelAccountPaymentMethods_PayChannelAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PayChannelAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelAccountPaymentMethods_AccountId",
                table: "PayChannelAccountPaymentMethods",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayChannelAccountPaymentMethods");
        }
    }
}
