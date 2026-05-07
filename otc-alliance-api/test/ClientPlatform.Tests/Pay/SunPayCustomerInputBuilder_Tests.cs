using System;
using System.Collections.Generic;
using Shouldly;
using ClientPlatform.Kyc;
using ClientPlatform.Pay.Channels.SunPay.Builders;
using ClientPlatform.Pay.Dto.Request;
using Xunit;

namespace ClientPlatform.Tests.Pay
{
    public class SunPayCustomerInputBuilder_Tests
    {
        private readonly SunPayCustomerInputBuilder _builder;

        public SunPayCustomerInputBuilder_Tests()
        {
            //_builder = new SunPayCustomerInputBuilder();
        }

        [Fact]
        public void IdentifyDocuments_Individual_Passport_Should_Return_Front()
        {
            // Arrange
            var kyc = new KycApplicant
            {
                ApplicantType = "individual",
                IdDocType = "PASSPORT"
            };
            var docs = new List<KycApplicantDocument>
            {
                new KycApplicantDocument { IdDocType = "PASSPORT", ImageSlot = 0, Url = "path/to/passport.jpg" }
            };

            // Act
            var result = _builder.IdentifyDocuments(kyc, docs);

            // Assert
            result.ContainsKey(SunPayCustomerInputBuilder.DocTypes.IdFront).ShouldBeTrue();
            result[SunPayCustomerInputBuilder.DocTypes.IdFront].Url.ShouldBe("path/to/passport.jpg");
            result.ContainsKey(SunPayCustomerInputBuilder.DocTypes.IdBack).ShouldBeFalse();
        }

        [Fact]
        public void IdentifyDocuments_Individual_IdCard_Should_Return_Front_And_Back()
        {
            // Arrange
            var kyc = new KycApplicant
            {
                ApplicantType = "individual",
                IdDocType = "ID_CARD"
            };
            var docs = new List<KycApplicantDocument>
            {
                new KycApplicantDocument { IdDocType = "ID_CARD", ImageSlot = 0, Url = "path/to/front.jpg" },
                new KycApplicantDocument { IdDocType = "ID_CARD", ImageSlot = 1, Url = "path/to/back.jpg" }
            };

            // Act
            var result = _builder.IdentifyDocuments(kyc, docs);

            // Assert
            result.ContainsKey(SunPayCustomerInputBuilder.DocTypes.IdFront).ShouldBeTrue();
            result.ContainsKey(SunPayCustomerInputBuilder.DocTypes.IdBack).ShouldBeTrue();
        }

        [Fact]
        public void IdentifyDocuments_Company_Should_Return_CompanyDoc()
        {
            // Arrange
            var kyc = new KycApplicant
            {
                ApplicantType = "company",
                IdDocType = "PASSPORT"
            };
            var docs = new List<KycApplicantDocument>
            {
                new KycApplicantDocument { IdDocType = "PASSPORT", ImageSlot = 0, Url = "rep_passport.jpg" },
                new KycApplicantDocument { IdDocType = "COMPANY_REGISTRATION", Url = "company_reg.pdf" }
            };

            // Act
            var result = _builder.IdentifyDocuments(kyc, docs);

            // Assert
            result.ContainsKey(SunPayCustomerInputBuilder.DocTypes.CompanyDoc).ShouldBeTrue();
            result[SunPayCustomerInputBuilder.DocTypes.CompanyDoc].Url.ShouldBe("company_reg.pdf");

            // Representative doc should also be identified as ID_FRONT
            result.ContainsKey(SunPayCustomerInputBuilder.DocTypes.IdFront).ShouldBeTrue();
            result[SunPayCustomerInputBuilder.DocTypes.IdFront].Url.ShouldBe("rep_passport.jpg");
        }

        [Fact]
        public void Build_Individual_Should_Map_Correctly()
        {
            // Arrange
            var kyc = new KycApplicant
            {
                UserId = 12345,
                ApplicantType = "individual",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Country = "GB",
                IdDocType = "PASSPORT",
                IdDocNumber = "P123456"
            };
            var fileIdMap = new Dictionary<string, string>
            {
                { SunPayCustomerInputBuilder.DocTypes.IdFront, "FILE_123" }
            };

            // Act
            var input = _builder.Build(kyc, fileIdMap);

            // Assert
            input.CustomerType.ShouldBe("INDIVIDUAL");
            input.FirstName.ShouldBe("John");
            input.LastName.ShouldBe("Doe");
            input.IdDocumentType.ShouldBe("PASSPORT"); // Assuming builder maps PASSPORT -> PASSPORT
            input.IdDocumentNumber.ShouldBe("P123456");
            input.IdFrontSideDocumentId.ShouldBe("FILE_123");
        }

        [Fact]
        public void Build_Company_Should_Map_Correctly()
        {
            // Arrange
            var kyc = new KycApplicant
            {
                UserId = 67890,
                ApplicantType = "company",
                CompanyName = "Tech Corp",
                CompanyRegistrationNumber = "REG999",
                FirstName = "Jane", // Representative
                LastName = "Doe",
                CompanyEmail = "contact@techcorp.com",
                CompanyCountry = "US",
                IdDocType = "ID_CARD",
                IdDocNumber = "ID888"
            };
            var fileIdMap = new Dictionary<string, string>
            {
                { SunPayCustomerInputBuilder.DocTypes.CompanyDoc, "FILE_REG" },
                { SunPayCustomerInputBuilder.DocTypes.IdFront, "FILE_FRONT" },
                { SunPayCustomerInputBuilder.DocTypes.IdBack, "FILE_BACK" }
            };

            // Act
            var input = _builder.Build(kyc, fileIdMap);

            // Assert
            input.CustomerType.ShouldBe("COMPANY");
            input.CompanyName.ShouldBe("Tech Corp");
            input.RegistrationNumber.ShouldBe("REG999");
            input.CompanyDocumentId.ShouldBe("FILE_REG");
            input.CompanyRepresentativeName.ShouldBe("Jane Doe");
            input.CompanyRepresentativeNumber.ShouldBe("ID888");

            // Depends on if builder maps Rep docs to top-level fields or Company fields
            // Assuming CreateCustomerInput has fields for Rep docs too or reuses IdFront/Back
            // Based on CreateCustomerInput.cs viewing earlier:
            // It has IdFrontSideDocumentId, IdBackSideDocumentId.
            input.IdFrontSideDocumentId.ShouldBe("FILE_FRONT");
            input.IdBackSideDocumentId.ShouldBe("FILE_BACK");
        }
    }
}
