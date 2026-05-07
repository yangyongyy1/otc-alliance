using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class Add_PayChannelTransactions_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayChannelTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelProvider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OrderNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OutOrderNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OutUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Reference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PaymentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BizType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BizStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SettlementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SettlementAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    SettlementCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SettlementFee = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    SettlementFeeCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SettlementTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SenderName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SenderAccountNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SenderIban = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SenderSwiftBic = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RecipientAccountId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RecipientCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RecipientIban = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RecipientSwiftBic = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RecipientBankCountry = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RecipientBankAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecipientAccountNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RecipientBankName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecipientBankAccountHolderName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TransactionCreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransactionCompletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PayChannelTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayChannelTransactions_PayChannelAccounts_ChannelAccountId",
                        column: x => x.ChannelAccountId,
                        principalTable: "PayChannelAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayChannelTransactions_ChannelAccountId",
                table: "PayChannelTransactions",
                column: "ChannelAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayChannelTransactions");
        }
    }
}
