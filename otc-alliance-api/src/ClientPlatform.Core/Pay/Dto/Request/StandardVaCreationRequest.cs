using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Request
{
    public class StandardVaCreationRequest
    {
        /// <summary>
        /// 货币代码 (例如: USD, EUR, GBP)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 引用ID (通常为 EntityId)
        /// </summary>
        public string ReferenceId { get; set; }

        // Standard Fields
        /// <summary>
        /// 账户名称 (Combined Name)
        /// </summary>
        public string AccountName { get; set; } // Legacy/Combined

        /// <summary>
        /// 持有人名称 (Alias for AccountName)
        /// </summary>
        public string HolderName { get; set; } // Alias for AccountName?

        // Split Names
        /// <summary>
        /// 名
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// 中间名
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// 国家代码 (ISO 3166-1 alpha-2)
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 出生日期 (YYYY-MM-DD)
        /// </summary>
        public string BirthDate { get; set; }

        /// <summary>
        /// 国籍 (ISO 3166-1 alpha-2)
        /// </summary>
        public string Nationality { get; set; }

        /// <summary>
        /// 地址行
        /// </summary>
        public string AddressLine { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string PostCode { get; set; }

        /// <summary>
        /// 省/州
        /// </summary>
        public string State { get; set; }

        // Identity
        /// <summary>
        /// 身份证件号码
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// 证件类型 (例如: Passport, NationalID)
        /// </summary>
        public string IdDocumentType { get; set; }

        #region Company Fields
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 公司注册号
        /// </summary>
        public string RegistrationNumber { get; set; }

        /// <summary>
        /// 公司注册日期 (YYYY-MM-DD)
        /// </summary>
        public string CompanyRegistrationDate { get; set; }

        /// <summary>
        /// 公司注册国家 (ISO 3166-1 alpha-2)
        /// </summary>
        public string CountryOfIncorporation { get; set; }

        /// <summary>
        /// 公司类型 (例如: LTD, LLC)
        /// </summary>
        public string CompanyType { get; set; }

        /// <summary>
        /// 公司联系人
        /// </summary>
        public string CompanyContact { get; set; }
        #endregion

        #region Trading Address
        /// <summary>
        /// 经营地址
        /// </summary>
        public string TradingAddress { get; set; }

        /// <summary>
        /// 经营城市
        /// </summary>
        public string TradingCity { get; set; }

        /// <summary>
        /// 经营国家
        /// </summary>
        public string TradingCountry { get; set; }
        #endregion

        #region USD Specific Fields
        /// <summary>
        /// 就业状态
        /// </summary>
        public string EmploymentStatus { get; set; }

        /// <summary>
        /// 职业
        /// </summary>
        public string Occupation { get; set; }

        /// <summary>
        /// 资金来源
        /// </summary>
        public string SourceOfFunds { get; set; }
        #endregion

        #region Document Fields
        /// <summary>
        /// 护照文档ID
        /// </summary>
        public string PassportDocumentId { get; set; }

        /// <summary>
        /// 身份证正面文档ID
        /// </summary>
        public string IdFrontSideDocumentId { get; set; }

        /// <summary>
        /// 驾照正面文档ID
        /// </summary>
        public string DriversLicenceFrontDocumentId { get; set; }

        /// <summary>
        /// 驾照背面文档ID
        /// </summary>
        public string DriversLicenceBackDocumentId { get; set; }

        /// <summary>
        /// 身份证/ID卡正面文档ID
        /// </summary>
        public string IdentityCardFrontDocumentId { get; set; }

        /// <summary>
        /// 身份证/ID卡背面文档ID
        /// </summary>
        public string IdentityCardBackDocumentId { get; set; }

        /// <summary>
        /// 银行对账单文档ID (作为地址证明)
        /// </summary>
        public string ProofOfAddressBankStatementDocumentId { get; set; }

        /// <summary>
        /// 水电费账单文档ID (作为地址证明)
        /// </summary>
        public string ProofOfAddressUtilityBillDocumentId { get; set; }

        /// <summary>
        /// 租赁协议文档ID (作为地址证明)
        /// </summary>
        public string ProofOfAddressLeaseAgreementDocumentId { get; set; }

        // Company Docs
        /// <summary>
        /// 公司成立证明文档ID
        /// </summary>
        public string CompanyIncorporationDocumentId { get; set; }

        /// <summary>
        /// 公司章程文档ID
        /// </summary>
        public string CompanyArticlesOfIncorporationDocumentId { get; set; }

        /// <summary>
        /// 实益拥有权证书文档ID
        /// </summary>
        public string CompanyBeneficialOwnershipCertificateDocumentId { get; set; }

        /// <summary>
        /// 公司银行对账单文档ID
        /// </summary>
        public string CompanyBankStatementDocumentId { get; set; }
        #endregion
    }
}
