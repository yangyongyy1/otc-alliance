using System;
using System.Collections.Generic;
using ClientPlatform.Pay.Dto.Request;

namespace ClientPlatform.Pay.Mappers
{
    /// <summary>
    /// VA 账户创建请求映射器
    /// 用于将 CreateAccountInput 转换为 StandardVaCreationRequest
    /// </summary>
    public class VaCreationRequestMapper
    {
        /// <summary>
        /// 从 CreateAccountInput 构建 StandardVaCreationRequest
        /// </summary>
        /// <param name="input">账户创建输入</param>
        /// <param name="customerId">客户 ID（来自验证后的 PayCustomer）</param>
        /// <returns>标准化的账户创建请求</returns>
        public StandardVaCreationRequest Map(CreateAccountInput input, string customerId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return new StandardVaCreationRequest
            {
                // 基本信息
                Currency = input.Currency,
                ReferenceId = customerId,
                HolderName = input.AccountName ?? BuildHolderName(input),

                // 个人信息
                FirstName = input.FirstName,
                MiddleName = input.MiddleName,
                LastName = input.LastName,
                BirthDate = input.BirthDate,
                Nationality = input.Nationality,

                // 联系方式
                Email = input.Email,
                Phone = input.Phone,

                // 地址信息
                AddressLine = input.AddressLine,
                City = input.City,
                State = input.State,
                PostCode = input.PostCode,
                CountryCode = input.CountryCode,

                // 身份证件
                IdNumber = input.IdNumber,
                IdDocumentType = input.IdDocumentType,

                // 企业信息
                CompanyName = input.CompanyName,
                RegistrationNumber = input.RegistrationNumber,
                CompanyRegistrationDate = input.CompanyRegistrationDate,
                CountryOfIncorporation = input.CountryOfIncorporation,
                CompanyType = input.CompanyType,
                CompanyContact = input.CompanyContact,

                // 经营地址
                TradingAddress = input.TradingAddress,
                TradingCity = input.TradingCity,
                TradingCountry = input.TradingCountry,

                // 就业信息（USD 特定）
                EmploymentStatus = input.EmploymentStatus,
                Occupation = input.Occupation,
                SourceOfFunds = input.SourceOfFunds,

                // 文档 - 身份证明
                PassportDocumentId = input.PassportDocumentId,
                IdFrontSideDocumentId = input.IdFrontSideDocumentId,
                IdentityCardFrontDocumentId = input.IdentityCardFrontDocumentId,
                IdentityCardBackDocumentId = input.IdentityCardBackDocumentId,
                DriversLicenceFrontDocumentId = input.DriversLicenceFrontDocumentId,
                DriversLicenceBackDocumentId = input.DriversLicenceBackDocumentId,

                // 文档 - 地址证明
                ProofOfAddressBankStatementDocumentId = input.ProofOfAddressBankStatementDocumentId,
                ProofOfAddressUtilityBillDocumentId = input.ProofOfAddressUtilityBillDocumentId,
                ProofOfAddressLeaseAgreementDocumentId = input.ProofOfAddressLeaseAgreementDocumentId,

                // 文档 - 企业证明
                CompanyIncorporationDocumentId = input.CompanyIncorporationDocumentId,
                CompanyArticlesOfIncorporationDocumentId = input.CompanyArticlesOfIncorporationDocumentId,
                CompanyBeneficialOwnershipCertificateDocumentId = input.CompanyBeneficialOwnershipCertificateDocumentId,
                CompanyBankStatementDocumentId = input.CompanyBankStatementDocumentId
            };
        }

        /// <summary>
        /// 构建账户持有人姓名
        /// 按照 FirstName MiddleName LastName 的顺序组合
        /// </summary>
        /// <param name="input">账户创建输入</param>
        /// <returns>完整姓名</returns>
        private string BuildHolderName(CreateAccountInput input)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(input.FirstName))
                parts.Add(input.FirstName);

            if (!string.IsNullOrWhiteSpace(input.MiddleName))
                parts.Add(input.MiddleName);

            if (!string.IsNullOrWhiteSpace(input.LastName))
                parts.Add(input.LastName);

            return string.Join(" ", parts);
        }
    }
}
