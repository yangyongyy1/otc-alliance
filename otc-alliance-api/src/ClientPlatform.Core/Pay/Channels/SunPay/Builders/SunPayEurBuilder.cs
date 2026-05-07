using Abp.Dependency;
using System.Collections.Generic;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Models;

namespace ClientPlatform.Pay.Channels.SunPay.Builders
{
    public class SunPayEurBuilder : ISunPayParamBuilder
    {
        public bool CanHandle(string currency)
        {
            return currency?.ToUpper() == "EUR";
        }

        public CreateAccountInput Build(StandardVaCreationRequest request)
        {
            // 1. 构建 AccountName (优先使用拆分的 First/Middle/Last Name)
            string accountName = request.HolderName;
            if (!string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName))
            {
                var parts = new List<string> { request.FirstName, request.MiddleName, request.LastName };
                // 移除空值并用空格连接
                accountName = string.Join(" ", parts.FindAll(s => !string.IsNullOrWhiteSpace(s)));
            }

            // 2. 构建 Details 字典
            // [User Feedback]: Details dictionary construction removed as fields are removed from StandardVaCreationRequest
            // var details = ...

            // 3. 返回 CreateAccountInput
            return new CreateAccountInput
            {
                Currency = "EUR",
                CustomerId = request.ReferenceId, // 假设 ReferenceId 映射为 SunPay 的 Customer 关联ID
                AccountName = accountName,

                // Detailed Name
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,

                // Contact & Address
                Email = request.Email,
                Phone = request.Phone,
                AddressLine = request.AddressLine,
                City = request.City,
                PostCode = request.PostCode,
                State = request.State,
                CountryCode = request.CountryCode,
                Nationality = request.Nationality,
                BirthDate = request.BirthDate,

                // Bank Specific
                // BankCountryCode = request.BankCountryCode, // [User Removed]

                // Identity
                IdNumber = request.IdNumber,
                IdDocumentType = request.IdDocumentType,

                // Company Fields
                CompanyName = request.CompanyName,
                RegistrationNumber = request.RegistrationNumber,
                CompanyRegistrationDate = request.CompanyRegistrationDate,
                CountryOfIncorporation = request.CountryOfIncorporation,
                CompanyType = request.CompanyType,
                CompanyContact = request.CompanyContact,

                // Trading Address
                TradingAddress = request.TradingAddress,
                TradingCity = request.TradingCity,
                TradingCountry = request.TradingCountry,

                // USD Fields (Mapped just in case logic is shared or extended)
                EmploymentStatus = request.EmploymentStatus,
                Occupation = request.Occupation,
                SourceOfFunds = request.SourceOfFunds,

                // Documents - Identity
                PassportDocumentId = request.PassportDocumentId,
                IdFrontSideDocumentId = request.IdFrontSideDocumentId,
                IdentityCardFrontDocumentId = request.IdentityCardFrontDocumentId,
                IdentityCardBackDocumentId = request.IdentityCardBackDocumentId,
                DriversLicenceFrontDocumentId = request.DriversLicenceFrontDocumentId,
                DriversLicenceBackDocumentId = request.DriversLicenceBackDocumentId,

                // Documents - Proof of Address
                ProofOfAddressBankStatementDocumentId = request.ProofOfAddressBankStatementDocumentId,
                ProofOfAddressUtilityBillDocumentId = request.ProofOfAddressUtilityBillDocumentId,
                ProofOfAddressLeaseAgreementDocumentId = request.ProofOfAddressLeaseAgreementDocumentId,

                // Documents - Company
                CompanyIncorporationDocumentId = request.CompanyIncorporationDocumentId,
                CompanyArticlesOfIncorporationDocumentId = request.CompanyArticlesOfIncorporationDocumentId,
                CompanyBeneficialOwnershipCertificateDocumentId = request.CompanyBeneficialOwnershipCertificateDocumentId,
                CompanyBankStatementDocumentId = request.CompanyBankStatementDocumentId,

                // Use the details built above, merged with any existing details if necessary
                // Details = details // [User Feedback]: Remove Details/iban_required/purpose as they are not valid params
            };
        }
    }
}
