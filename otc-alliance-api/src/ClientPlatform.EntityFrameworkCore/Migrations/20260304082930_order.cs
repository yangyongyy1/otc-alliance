using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionOrderDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayerName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PayerCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PayerIban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PayerSwiftBic = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RecipientAccountHolderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientIban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientSwiftBic = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientBankName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientBankAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecipientBankCountry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortCode = table.Column<string>(type: "text", nullable: true),
                    RequestInfo = table.Column<string>(type: "json", nullable: true),
                    ResponseInfo = table.Column<string>(type: "json", nullable: true),
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
                    table.PrimaryKey("PK_CollectionOrderDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformOrderNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChannelOrderNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AllianceId = table.Column<int>(type: "integer", nullable: false),
                    AllianceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MerchantId = table.Column<int>(type: "integer", nullable: false),
                    MerchantName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserEmail = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ChannelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
                    PayerName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_CollectionOrders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionOrderDetails");

            migrationBuilder.DropTable(
                name: "CollectionOrders");
        }
    }
}
