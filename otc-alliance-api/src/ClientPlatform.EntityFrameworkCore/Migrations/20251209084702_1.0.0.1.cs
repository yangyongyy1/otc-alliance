using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class _1001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountryInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CNName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CCA2 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CCA3 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CCN3 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Currency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AreaPhoneCode = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SubRegion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CnRegion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CnSubRegion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_CountryInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryInfos");
        }
    }
}
