using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPlatform.Migrations
{
    /// <inheritdoc />
    public partial class carl_1006 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppKycApplicants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    KycProvider = table.Column<string>(type: "text", nullable: true),
                    KycChannelProductTypes = table.Column<int>(type: "integer", nullable: false),
                    KycType = table.Column<int>(type: "integer", nullable: false),
                    KycLevelName = table.Column<int>(type: "integer", nullable: false),
                    KycVerificationLink = table.Column<string>(type: "text", nullable: true),
                    KycBizStatus = table.Column<int>(type: "integer", nullable: false),
                    KycChannelStatus = table.Column<string>(type: "text", nullable: true),
                    ApplicantId = table.Column<string>(type: "text", nullable: true),
                    InspectionId = table.Column<string>(type: "text", nullable: true),
                    ExternalUserId = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    ApiKey = table.Column<string>(type: "text", nullable: true),
                    ApplicantType = table.Column<string>(type: "text", nullable: true),
                    ApplicantPlatform = table.Column<string>(type: "text", nullable: true),
                    IpCountry = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    FirstNameEn = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    LastNameEn = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    FixedFirstName = table.Column<string>(type: "text", nullable: true),
                    FixedFirstNameEn = table.Column<string>(type: "text", nullable: true),
                    FixedLastName = table.Column<string>(type: "text", nullable: true),
                    FixedLastNameEn = table.Column<string>(type: "text", nullable: true),
                    FixedDob = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    TaxResidenceCountry = table.Column<string>(type: "text", nullable: true),
                    Street = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    BuildingNumber = table.Column<string>(type: "text", nullable: true),
                    Town = table.Column<string>(type: "text", nullable: true),
                    PostCode = table.Column<string>(type: "text", nullable: true),
                    FormattedAddress = table.Column<string>(type: "text", nullable: true),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    CompanyRegistrationNumber = table.Column<string>(type: "text", nullable: true),
                    CompanyCountry = table.Column<string>(type: "text", nullable: true),
                    CompanyLegalAddress = table.Column<string>(type: "text", nullable: true),
                    CompanyEmail = table.Column<string>(type: "text", nullable: true),
                    OwnershipStructureDepth = table.Column<int>(type: "integer", nullable: true),
                    SkippedBeneficiaryTypes = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ReviewStatus = table.Column<string>(type: "text", nullable: true),
                    ReviewAnswer = table.Column<string>(type: "text", nullable: true),
                    ReviewRejectType = table.Column<string>(type: "text", nullable: true),
                    ModerationComment = table.Column<string>(type: "text", nullable: true),
                    ReviewLevel = table.Column<string>(type: "text", nullable: true),
                    ReviewAttemptCount = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClientComment = table.Column<string>(type: "text", nullable: true),
                    RejectLabels = table.Column<string>(type: "text", nullable: true),
                    AgreementAcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgreementSource = table.Column<string>(type: "text", nullable: true),
                    EmploymentStatus = table.Column<string>(type: "text", nullable: true),
                    AnnualIncome = table.Column<string>(type: "text", nullable: true),
                    IdDocType = table.Column<string>(type: "text", nullable: true),
                    IdDocCountry = table.Column<string>(type: "text", nullable: true),
                    IdDocNumber = table.Column<string>(type: "text", nullable: true),
                    IdDocValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasSelfie = table.Column<bool>(type: "boolean", nullable: true),
                    HasVideo = table.Column<bool>(type: "boolean", nullable: true),
                    HasProofOfAddress = table.Column<bool>(type: "boolean", nullable: true),
                    FileInfos = table.Column<string>(type: "text", nullable: true),
                    QuestionnaireJson = table.Column<string>(type: "text", nullable: true),
                    RequiredIdDocsJson = table.Column<string>(type: "text", nullable: true),
                    RawJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_AppKycApplicants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppKycApplicants_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppKycApplicantBeneficiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeneficiaryApplicantId = table.Column<string>(type: "text", nullable: true),
                    BeneficiaryId = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: true),
                    Submitted = table.Column<bool>(type: "boolean", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_AppKycApplicantBeneficiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppKycApplicantBeneficiaries_AppKycApplicants_KycApplicantId",
                        column: x => x.KycApplicantId,
                        principalTable: "AppKycApplicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppKycApplicantDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionId = table.Column<string>(type: "text", nullable: true),
                    IdDocType = table.Column<string>(type: "text", nullable: true),
                    ImageId = table.Column<string>(type: "text", nullable: true),
                    ImageSlot = table.Column<int>(type: "integer", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: true),
                    ReviewAnswer = table.Column<string>(type: "text", nullable: true),
                    ModerationComment = table.Column<string>(type: "text", nullable: true),
                    ClientComment = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AppKycApplicantDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppKycApplicantDocuments_AppKycApplicants_KycApplicantId",
                        column: x => x.KycApplicantId,
                        principalTable: "AppKycApplicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppKycApplicantBeneficiaries_KycApplicantId",
                table: "AppKycApplicantBeneficiaries",
                column: "KycApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppKycApplicantDocuments_KycApplicantId",
                table: "AppKycApplicantDocuments",
                column: "KycApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppKycApplicants_UserId",
                table: "AppKycApplicants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppKycApplicantBeneficiaries");

            migrationBuilder.DropTable(
                name: "AppKycApplicantDocuments");

            migrationBuilder.DropTable(
                name: "AppKycApplicants");
        }
    }
}
