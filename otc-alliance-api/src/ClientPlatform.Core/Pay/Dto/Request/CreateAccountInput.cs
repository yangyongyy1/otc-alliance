using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 创建虚拟账户输入参数 (包含个人和企业)
/// </summary>
public class CreateAccountInput : BasePayRequest
{
    /// <summary>
    /// 币种 (例如: EUR)
    /// </summary>
    [Required]
    [JsonProperty("currency")]
    public string Currency { get; set; }



    /// <summary>
    /// 客户 ID（系统自动解析，无需前端传入）
    /// </summary>
    [JsonProperty("customer_id")]
    public string CustomerId { get; set; }

    #region Individual Fields

    /// <summary>
    /// 名 (个人账户必填)
    /// </summary>
    [JsonProperty("first_name")]
    public string FirstName { get; set; }

    /// <summary>
    /// 中间名 (个人账户可选)
    /// </summary>
    [JsonProperty("middle_name")]
    public string MiddleName { get; set; }

    /// <summary>
    /// 姓 (个人账户必填)
    /// </summary>
    [JsonProperty("last_name")]
    public string LastName { get; set; }

    /// <summary>
    /// 电话号码 (个人账户必填)
    /// </summary>
    [JsonProperty("phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 证件号码 (个人账户必填)
    /// </summary>
    [JsonProperty("id_number")]
    public string IdNumber { get; set; }

    /// <summary>
    /// 证件类型 (例如: IDCARD) (个人账户必填)
    /// </summary>
    [JsonProperty("id_document_type")]
    public string IdDocumentType { get; set; }

    /// <summary>
    /// 证件签发日期 (格式: YYYY-MM-DD)
    /// </summary>
    [JsonProperty("id_issue_date")]
    public string IdIssueDate { get; set; }

    /// <summary>
    /// 证件有效日期 (格式: YYYY-MM-DD)
    /// </summary>
    [JsonProperty("id_expiration_date")]
    public string IdExpirationDate { get; set; }

    /// <summary>
    /// 出生国家代码 (例如: LU) (个人账户必填)
    /// </summary>
    [JsonProperty("birth_country")]
    public string BirthCountry { get; set; }

    /// <summary>
    /// 出生日期 (格式: YYYY-MM-DD HH:MM:SS) (个人账户必填)
    /// </summary>
    [JsonProperty("birth_date")]
    public string BirthDate { get; set; }

    /// <summary>
    /// 电子邮箱 (个人账户必填)
    /// </summary>
    [JsonProperty("email")]
    public string Email { get; set; }

    /// <summary>
    /// IP 地址 (个人账户必填)
    /// </summary>
    [JsonProperty("ip_address")]
    public string IpAddress { get; set; }

    #endregion

    #region Company Fields

    /// <summary>
    /// 公司名称 (企业账户必填)
    /// </summary>
    [JsonProperty("company_name")]
    public string CompanyName { get; set; }

    /// <summary>
    /// 公司类型 (例如: LIMITED_LIABILITY)
    /// </summary>
    [JsonProperty("company_type")]
    public string CompanyType { get; set; }

    /// <summary>
    /// 公司注册号 (企业账户必填)
    /// </summary>
    [JsonProperty("registration_number")]
    public string RegistrationNumber { get; set; }

    /// <summary>
    /// 成立日期 (格式: YYYY-MM-DD) (企业账户必填)
    /// </summary>
    [JsonProperty("company_registration_date")]
    public string CompanyRegistrationDate { get; set; }

    /// <summary>
    /// 注册国家代码 (例如: GB)
    /// </summary>
    [JsonProperty("country_of_incorporation")]
    public string CountryOfIncorporation { get; set; }

    /// <summary>
    /// 州/省
    /// </summary>
    [JsonProperty("state")]
    public string State { get; set; }

    /// <summary>
    /// 业务人员名
    /// </summary>
    [JsonProperty("business_person_first_name")]
    public string BusinessPersonFirstName { get; set; }

    /// <summary>
    /// 业务人员姓
    /// </summary>
    [JsonProperty("business_person_last_name")]
    public string BusinessPersonLastName { get; set; }

    /// <summary>
    /// 业务人员持股比例
    /// </summary>
    [JsonProperty("business_person_ownership")]
    public decimal? BusinessPersonOwnership { get; set; }

    #endregion

    #region Common Address Fields

    /// <summary>
    /// 地址行
    /// </summary>
    [JsonProperty("address_line")]
    public string AddressLine { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [JsonProperty("city")]
    public string City { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [JsonProperty("post_code")]
    public string PostCode { get; set; }

    /// <summary>
    /// 国家代码 (例如: DE)
    /// </summary>
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }

    /// <summary>
    /// 银行账户国家代码 (部分渠道如 Stables 需要)
    /// </summary>
    [JsonProperty("bank_country_code")]
    public string BankCountryCode { get; set; }

    /// <summary>
    /// 国籍 (例如: GB)
    /// </summary>
    [JsonProperty("nationality")]
    public string Nationality { get; set; }

    #endregion

    #region Mailing Address Fields (USD)

    /// <summary>
    /// 邮寄地址国家代码 (USD 账户必填)
    /// </summary>
    [JsonProperty("mailing_address_country_code")]
    public string MailingAddressCountryCode { get; set; }

    /// <summary>
    /// 邮寄地址州/省 (USD 账户必填)
    /// </summary>
    [JsonProperty("mailing_address_state")]
    public string MailingAddressState { get; set; }

    /// <summary>
    /// 邮寄地址城市 (USD 账户必填)
    /// </summary>
    [JsonProperty("mailing_address_city")]
    public string MailingAddressCity { get; set; }

    /// <summary>
    /// 邮寄地址行 (USD 账户必填)
    /// </summary>
    [JsonProperty("mailing_address_address_line")]
    public string MailingAddressAddressLine { get; set; }

    /// <summary>
    /// 邮寄地址邮政编码 (USD 账户必填)
    /// </summary>
    [JsonProperty("mailing_address_postal_code")]
    public string MailingAddressPostalCode { get; set; }

    #endregion

    #region Employment & Financial Fields (USD)

    /// <summary>
    /// 就业状态 (例如: EMPLOYED)
    /// </summary>
    [JsonProperty("employment_status")]
    public string EmploymentStatus { get; set; }

    /// <summary>
    /// 就业描述
    /// </summary>
    [JsonProperty("employment_description")]
    public string EmploymentDescription { get; set; }

    /// <summary>
    /// 雇主名称
    /// </summary>
    [JsonProperty("employer_name")]
    public string EmployerName { get; set; }

    /// <summary>
    /// 职业
    /// </summary>
    [JsonProperty("occupation")]
    public string Occupation { get; set; }

    /// <summary>
    /// 主要资金来源 (例如: SALARY)
    /// </summary>
    [JsonProperty("primary_source_of_funds")]
    public string PrimarySourceOfFunds { get; set; }

    /// <summary>
    /// 主要资金来源描述
    /// </summary>
    [JsonProperty("primary_source_of_funds_description")]
    public string PrimarySourceOfFundsDescription { get; set; }

    /// <summary>
    /// 预计法币交易金额 (USD)
    /// </summary>
    [JsonProperty("usd_value_of_fiat")]
    public string UsdValueOfFiat { get; set; }

    /// <summary>
    /// 预计加密货币交易金额 (USD)
    /// </summary>
    [JsonProperty("usd_value_of_crypto")]
    public string UsdValueOfCrypto { get; set; }

    /// <summary>
    /// 每月存款金额
    /// </summary>
    [JsonProperty("monthly_deposits")]
    public string MonthlyDeposits { get; set; }

    /// <summary>
    /// 每月加密货币存款金额
    /// </summary>
    [JsonProperty("monthly_crypto_deposits")]
    public string MonthlyCryptoDeposits { get; set; }

    /// <summary>
    /// 每月投资存款金额
    /// </summary>
    [JsonProperty("monthly_investment_deposit")]
    public string MonthlyInvestmentDeposit { get; set; }

    /// <summary>
    /// 每月加密货币投资存款金额
    /// </summary>
    [JsonProperty("monthly_crypto_investment_deposit")]
    public string MonthlyCryptoInvestmentDeposit { get; set; }

    /// <summary>
    /// 每月取款金额
    /// </summary>
    [JsonProperty("monthly_withdrawals")]
    public string MonthlyWithdrawals { get; set; }

    /// <summary>
    /// 每月加密货币取款金额
    /// </summary>
    [JsonProperty("monthly_crypto_withdrawals")]
    public string MonthlyCryptoWithdrawals { get; set; }

    /// <summary>
    /// 每月投资取款金额
    /// </summary>
    [JsonProperty("monthly_investment_withdrawal")]
    public string MonthlyInvestmentWithdrawal { get; set; }

    /// <summary>
    /// 每月加密货币投资取款金额
    /// </summary>
    [JsonProperty("monthly_crypto_investment_withdrawal")]
    public string MonthlyCryptoInvestmentWithdrawal { get; set; }

    /// <summary>
    /// 资金收付司法管辖区
    /// </summary>
    [JsonProperty("funds_send_receive_jurisdictions")]
    public string FundsSendReceiveJurisdictions { get; set; }

    /// <summary>
    /// 从事活动
    /// </summary>
    [JsonProperty("engage_in_activities")]
    public string EngageInActivities { get; set; }

    /// <summary>
    /// 供应商和交易对手
    /// </summary>
    [JsonProperty("vendors_and_counterparties")]
    public string VendorsAndCounterparties { get; set; }

    #endregion

    #region Tax & Identity Fields (USD)

    /// <summary>
    /// 税务参考号
    /// </summary>
    [JsonProperty("tax_reference_number")]
    public string TaxReferenceNumber { get; set; }

    /// <summary>
    /// 公民身份 (国家代码)
    /// </summary>
    [JsonProperty("citizenship")]
    public string Citizenship { get; set; }

    #endregion

    #region Trading Address Fields (Company)

    /// <summary>
    /// 交易地址
    /// </summary>
    [JsonProperty("trading_address")]
    public string TradingAddress { get; set; }

    /// <summary>
    /// 交易城市
    /// </summary>
    [JsonProperty("trading_city")]
    public string TradingCity { get; set; }

    /// <summary>
    /// 交易国家代码
    /// </summary>
    [JsonProperty("trading_country")]
    public string TradingCountry { get; set; }

    /// <summary>
    /// 公司联系人
    /// </summary>
    [JsonProperty("company_contact")]
    public string CompanyContact { get; set; }

    #endregion

    #region Company Specific Fields (USD)

    /// <summary>
    /// 公司注册地址国家代码
    /// </summary>
    [JsonProperty("company_registered_address_country_code")]
    public string CompanyRegisteredAddressCountryCode { get; set; }

    /// <summary>
    /// 公司注册地址州/省
    /// </summary>
    [JsonProperty("company_registered_address_state")]
    public string CompanyRegisteredAddressState { get; set; }

    /// <summary>
    /// 公司注册地址城市
    /// </summary>
    [JsonProperty("company_registered_address_city")]
    public string CompanyRegisteredAddressCity { get; set; }

    /// <summary>
    /// 公司注册地址行 1
    /// </summary>
    [JsonProperty("company_registered_address_address_line1")]
    public string CompanyRegisteredAddressAddressLine1 { get; set; }

    /// <summary>
    /// 公司注册地址邮政编码
    /// </summary>
    [JsonProperty("company_registered_address_postal_code")]
    public string CompanyRegisteredAddressPostalCode { get; set; }

    /// <summary>
    /// 公司税务参考号
    /// </summary>
    [JsonProperty("company_tax_reference_number")]
    public string CompanyTaxReferenceNumber { get; set; }

    /// <summary>
    /// 公司网站地址
    /// </summary>
    [JsonProperty("company_website_address")]
    public string CompanyWebsiteAddress { get; set; }

    /// <summary>
    /// 公司注册州/省
    /// </summary>
    [JsonProperty("company_state_of_incorporation")]
    public string CompanyStateOfIncorporation { get; set; }

    /// <summary>
    /// 公司实体类型
    /// </summary>
    [JsonProperty("company_entity_type")]
    public string CompanyEntityType { get; set; }

    /// <summary>
    /// 公司实体类型描述
    /// </summary>
    [JsonProperty("company_entity_type_description")]
    public string CompanyEntityTypeDescription { get; set; }

    /// <summary>
    /// 北美行业分类系统 (NAICS) 代码
    /// </summary>
    [JsonProperty("company_naics")]
    public string CompanyNaics { get; set; }

    /// <summary>
    /// NAICS 描述
    /// </summary>
    [JsonProperty("company_naics_description")]
    public string CompanyNaicsDescription { get; set; }

    /// <summary>
    /// 公司主要业务
    /// </summary>
    [JsonProperty("company_primary_business")]
    public string CompanyPrimaryBusiness { get; set; }

    /// <summary>
    /// 公司业务性质描述
    /// </summary>
    [JsonProperty("company_description_of_business_nature")]
    public string CompanyDescriptionOfBusinessNature { get; set; }

    /// <summary>
    /// 公司业务司法管辖区
    /// </summary>
    [JsonProperty("company_business_jurisdictions")]
    public string CompanyBusinessJurisdictions { get; set; }

    /// <summary>
    /// 公司资金收付司法管辖区
    /// </summary>
    [JsonProperty("company_funds_send_receive_jurisdictions")]
    public string CompanyFundsSendReceiveJurisdictions { get; set; }

    /// <summary>
    /// 公司受监管状态
    /// </summary>
    /// <summary>
    /// 公司受监管状态
    /// </summary>
    [JsonProperty("company_regulated_status")]
    public string CompanyRegulatedStatus { get; set; }

    /// <summary>
    /// 公司实体地址行 1
    /// </summary>
    [JsonProperty("company_physical_address_address_line1")]
    public string CompanyPhysicalAddressAddressLine1 { get; set; }

    /// <summary>
    /// 公司实体地址城市
    /// </summary>
    [JsonProperty("company_physical_address_city")]
    public string CompanyPhysicalAddressCity { get; set; }

    /// <summary>
    /// 公司实体地址州/省
    /// </summary>
    [JsonProperty("company_physical_address_state")]
    public string CompanyPhysicalAddressState { get; set; }

    /// <summary>
    /// 公司实体地址邮政编码
    /// </summary>
    [JsonProperty("company_physical_address_postal_code")]
    public string CompanyPhysicalAddressPostalCode { get; set; }

    /// <summary>
    /// 公司实体地址国家代码
    /// </summary>
    [JsonProperty("company_physical_address_country_code")]
    public string CompanyPhysicalAddressCountryCode { get; set; }

    #endregion

    #region Business Person Fields (USD)

    /// <summary>
    /// 业务人员中间名
    /// </summary>
    [JsonProperty("business_person_middle_name")]
    public string BusinessPersonMiddleName { get; set; }

    /// <summary>
    /// 业务人员出生日期
    /// </summary>
    [JsonProperty("business_person_birth_date")]
    public string BusinessPersonBirthDate { get; set; }

    /// <summary>
    /// 业务人员邮箱
    /// </summary>
    [JsonProperty("business_person_email")]
    public string BusinessPersonEmail { get; set; }

    /// <summary>
    /// 业务人员电话
    /// </summary>
    [JsonProperty("business_person_phone")]
    public string BusinessPersonPhone { get; set; }

    /// <summary>
    /// 业务人员税务参考号
    /// </summary>
    [JsonProperty("business_person_tax_reference_number")]
    public string BusinessPersonTaxReferenceNumber { get; set; }

    /// <summary>
    /// 业务人员证件号码
    /// </summary>
    [JsonProperty("business_person_id_number")]
    public string BusinessPersonIdNumber { get; set; }

    /// <summary>
    /// 业务人员证件签发日期
    /// </summary>
    [JsonProperty("business_person_id_issue_date")]
    public string BusinessPersonIdIssueDate { get; set; }

    /// <summary>
    /// 业务人员证件有效日期
    /// </summary>
    [JsonProperty("business_person_id_expiration_date")]
    public string BusinessPersonIdExpirationDate { get; set; }

    /// <summary>
    /// 业务人员国籍/公民身份
    /// </summary>
    [JsonProperty("business_person_citizenship")]
    public string BusinessPersonCitizenship { get; set; }

    /// <summary>
    /// 业务人员角色
    /// </summary>
    [JsonProperty("business_person_role")]
    public string BusinessPersonRole { get; set; }

    /// <summary>
    /// 业务人员类型 (CONTACT, DIRECTOR, UBO)
    /// </summary>
    [JsonProperty("business_person_types")]
    public string BusinessPersonTypes { get; set; }

    /// <summary>
    /// 业务人员地址
    /// </summary>
    [JsonProperty("business_person_address_line")]
    public string BusinessPersonAddressLine { get; set; }

    /// <summary>
    /// 业务人员城市
    /// </summary>
    [JsonProperty("business_person_city")]
    public string BusinessPersonCity { get; set; }

    /// <summary>
    /// 业务人员邮编
    /// </summary>
    [JsonProperty("business_person_postal_code")]
    public string BusinessPersonPostalCode { get; set; }

    /// <summary>
    /// 业务人员国家
    /// </summary>
    [JsonProperty("business_person_country")]
    public string BusinessPersonCountry { get; set; }

    /// <summary>
    /// 业务人员州/省
    /// </summary>
    [JsonProperty("business_person_state")]
    public string BusinessPersonState { get; set; }

    #endregion

    #region Document Fields

    /// <summary>
    /// 护照文件 ID
    /// </summary>
    [JsonProperty("passport_document_id")]
    public string PassportDocumentId { get; set; }

    /// <summary>
    /// 证件正面文件 ID
    /// </summary>
    [JsonProperty("id_front_side_document_id")]
    public string IdFrontSideDocumentId { get; set; }

    /// <summary>
    /// 公司注册文件 ID
    /// </summary>
    [JsonProperty("company_incorporation_document_id")]
    public string CompanyIncorporationDocumentId { get; set; }

    /// <summary>
    /// 驾驶证正面文件 ID
    /// </summary>
    [JsonProperty("drivers_licence_front_document_id")]
    public string DriversLicenceFrontDocumentId { get; set; }

    /// <summary>
    /// 驾驶证背面文件 ID
    /// </summary>
    [JsonProperty("drivers_licence_back_document_id")]
    public string DriversLicenceBackDocumentId { get; set; }

    /// <summary>
    /// 身份证正面文件 ID
    /// </summary>
    [JsonProperty("identity_card_front_document_id")]
    public string IdentityCardFrontDocumentId { get; set; }

    /// <summary>
    /// 身份证背面文件 ID
    /// </summary>
    [JsonProperty("identity_card_back_document_id")]
    public string IdentityCardBackDocumentId { get; set; }

    /// <summary>
    /// 账户协议文件 ID
    /// </summary>
    [JsonProperty("account_agreement_document_id")]
    public string AccountAgreementDocumentId { get; set; }

    /// <summary>
    /// 地址证明类型
    /// </summary>
    [JsonProperty("proof_of_address")]
    public string ProofOfAddress { get; set; }

    /// <summary>
    /// 地址证明 (银行对账单) 文件 ID
    /// </summary>
    [JsonProperty("proof_of_address_bank_statement_document_id")]
    public string ProofOfAddressBankStatementDocumentId { get; set; }

    /// <summary>
    /// 地址证明 (水电费账单) 文件 ID
    /// </summary>
    [JsonProperty("proof_of_address_utility_bill_document_id")]
    public string ProofOfAddressUtilityBillDocumentId { get; set; }

    /// <summary>
    /// 地址证明 (租赁协议) 文件 ID
    /// </summary>
    [JsonProperty("proof_of_address_lease_agreement_document_id")]
    public string ProofOfAddressLeaseAgreementDocumentId { get; set; }

    /// <summary>
    /// 资金来源类型
    /// </summary>
    [JsonProperty("source_of_funds")]
    public string SourceOfFunds { get; set; }

    /// <summary>
    /// 资金来源 (银行对账单) 文件 ID
    /// </summary>
    [JsonProperty("source_of_funds_bank_statement_document_id")]
    public string SourceOfFundsBankStatementDocumentId { get; set; }

    /// <summary>
    /// 资金来源 (工资单) 文件 ID
    /// </summary>
    [JsonProperty("source_of_funds_payslip_document_id")]
    public string SourceOfFundsPayslipDocumentId { get; set; }

    /// <summary>
    /// 公司章程文件 ID
    /// </summary>
    [JsonProperty("company_articles_of_incorporation_document_id")]
    public string CompanyArticlesOfIncorporationDocumentId { get; set; }

    /// <summary>
    /// 公司受益所有权证书文件 ID
    /// </summary>
    [JsonProperty("company_beneficial_ownership_certificate_document_id")]
    public string CompanyBeneficialOwnershipCertificateDocumentId { get; set; }

    /// <summary>
    /// 公司账户协议文件 ID
    /// </summary>
    [JsonProperty("company_account_agreement_document_id")]
    public string CompanyAccountAgreementDocumentId { get; set; }

    /// <summary>
    /// 公司地址证明文件类型
    /// </summary>
    [JsonProperty("company_proof_of_address_document_type")]
    public string CompanyProofOfAddressDocumentType { get; set; }

    /// <summary>
    /// 公司银行对账单文件 ID
    /// </summary>
    [JsonProperty("company_bank_statement_document_id")]
    public string CompanyBankStatementDocumentId { get; set; }

    /// <summary>
    /// 公司资金来源 (银行对账单) 文件 ID
    /// </summary>
    [JsonProperty("company_source_of_funds_bank_statement_document_id")]
    public string CompanySourceOfFundsBankStatementDocumentId { get; set; }

    /// <summary>
    /// 公司资金来源 (财务报表) 文件 ID
    /// </summary>
    [JsonProperty("company_source_of_funds_financial_statement_document_id")]
    public string CompanySourceOfFundsFinancialStatementDocumentId { get; set; }

    /// <summary>
    /// 公司资金来源 (条款书) 文件 ID
    /// </summary>
    [JsonProperty("company_source_of_funds_term_sheet_document_id")]
    public string CompanySourceOfFundsTermSheetDocumentId { get; set; }

    /// <summary>
    /// 公司水电费账单文件 ID
    /// </summary>
    [JsonProperty("company_utility_bill_document_id")]
    public string CompanyUtilityBillDocumentId { get; set; }

    /// <summary>
    /// 公司租赁协议文件 ID
    /// </summary>
    [JsonProperty("company_lease_agreement_document_id")]
    public string CompanyLeaseAgreementDocumentId { get; set; }

    /// <summary>
    /// 公司监管许可证文件 ID
    /// </summary>
    [JsonProperty("company_regulatory_license_document_id")]
    public string CompanyRegulatoryLicenseDocumentId { get; set; }

    /// <summary>
    /// 公司个人证件类型
    /// </summary>
    [JsonProperty("company_individual_id_document_type")]
    public string CompanyIndividualIdDocumentType { get; set; }

    /// <summary>
    /// 自拍照文件 ID
    /// </summary>
    [JsonProperty("selfie_document_id")]
    public string SelfieDocumentId { get; set; }

    /// <summary>
    /// 公司个人地址证明类型
    /// </summary>
    [JsonProperty("company_individual_proof_of_address_document_type")]
    public string CompanyIndividualProofOfAddressDocumentType { get; set; }

    /// <summary>
    /// 公司个人银行对账单文件 ID
    /// </summary>
    [JsonProperty("company_individual_bank_statement_document_id")]
    public string CompanyIndividualBankStatementDocumentId { get; set; }

    /// <summary>
    /// 公司个人水电费账单文件 ID
    /// </summary>
    [JsonProperty("company_individual_utility_bill_document_id")]
    public string CompanyIndividualUtilityBillDocumentId { get; set; }

    /// <summary>
    /// 公司个人租赁协议文件 ID
    /// </summary>
    [JsonProperty("company_individual_lease_agreement_document_id")]
    public string CompanyIndividualLeaseAgreementDocumentId { get; set; }

    /// <summary>
    /// 公司资金来源证明类型
    /// </summary>
    [JsonProperty("company_proof_of_source_of_funds")]
    public string CompanyProofOfSourceOfFunds { get; set; }

    #endregion
    #region Dynamic/Extra Fields


    /// <summary>
    /// 账户名称 (部分接口可能使用此统一字段而非 FirstName/LastName)
    /// </summary>
    [JsonProperty("account_name")]
    public string AccountName { get; set; }

    #endregion
}
