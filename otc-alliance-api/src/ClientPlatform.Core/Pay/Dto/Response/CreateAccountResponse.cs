using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClientPlatform.Pay.Dto.Response;

/// <summary>
/// 创建虚拟账户响应
/// </summary>
public class CreateAccountResponse
{
    /// <summary>
    /// 客户 ID
    /// </summary>
    [JsonProperty("customer_id")]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// 账户号码
    /// </summary>
    [JsonProperty("account_number")]
    public string AccountNumber { get; set; }

    /// <summary>
    /// IBAN
    /// </summary>
    [JsonProperty("iban")]
    public string IBAN { get; set; }

    /// <summary>
    /// 币种
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// 动作说明
    /// </summary>
    [JsonProperty("action_reference")]
    public string ActionReference { get; set; }

    /// <summary>
    /// 动作
    /// </summary>
    [JsonProperty("action")]
    public string Action { get; set; }

    /// <summary>
    /// 认证链接，redirect_authentication 为真则会返回链接
    /// </summary>
    [JsonProperty("href")]
    public string Href { get; set; }

    /// <summary>
    /// 信息
    /// </summary>
    [JsonProperty("msg")]
    public string Msg { get; set; }

    /// <summary>
    /// 名
    /// </summary>
    [JsonProperty("first_name")]
    public string FirstName { get; set; }

    /// <summary>
    /// 中间名
    /// </summary>
    [JsonProperty("middle_name")]
    public string MiddleName { get; set; }

    /// <summary>
    /// 姓
    /// </summary>
    [JsonProperty("last_name")]
    public string LastName { get; set; }

    /// <summary>
    /// 电子邮箱（必须是有效的电子邮件地址）
    /// </summary>
    [JsonProperty("email")]
    public string Email { get; set; }

    /// <summary>
    /// 电话号码（有效的国际格式，例如 "+386 40040040"，不带空格）
    /// </summary>
    [JsonProperty("phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 地址行
    /// </summary>
    [JsonProperty("address_line")]
    public string AddressLine { get; set; }

    /// <summary>
    /// 邮编
    /// </summary>
    [JsonProperty("zip_code")]
    public string ZipCode { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [JsonProperty("city")]
    public string City { get; set; }

    /// <summary>
    /// 州/省
    /// </summary>
    [JsonProperty("state")]
    public string State { get; set; }

    /// <summary>
    /// 国家代码
    /// </summary>
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }

    /// <summary>
    /// 账户 ID
    /// </summary>
    [JsonProperty("account_id")]
    public string AccountId { get; set; }

    /// <summary>
    /// 状态（默认: Submitted）
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// 公司联系人
    /// </summary>
    [JsonProperty("company_contact")]
    public string CompanyContact { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    [JsonProperty("company_name")]
    public string CompanyName { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    [JsonProperty("birth_date")]
    public long? BirthDate { get; set; }

    /// <summary>
    /// 公司注册号码
    /// </summary>
    [JsonProperty("registration_number")]
    public string RegistrationNumber { get; set; }

    /// <summary>
    /// 公司交易地址（欧洲地区）
    /// </summary>
    [JsonProperty("trading_address")]
    public string TradingAddress { get; set; }

    /// <summary>
    /// 公司交易城市（欧洲地区）
    /// </summary>
    [JsonProperty("trading_city")]
    public string TradingCity { get; set; }

    /// <summary>
    /// 公司交易国家（欧洲地区）
    /// </summary>
    [JsonProperty("trading_country")]
    public string TradingCountry { get; set; }

    /// <summary>
    /// 公司类型
    /// </summary>
    [JsonProperty("company_type")]
    public string CompanyType { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [JsonProperty("post_code")]
    public string PostCode { get; set; }

    /// <summary>
    /// 证件号码
    /// </summary>
    [JsonProperty("id_number")]
    public string IdNumber { get; set; }

    /// <summary>
    /// 公司交易地址邮编
    /// </summary>
    [JsonProperty("trading_postal_code")]
    public string TradingPostalCode { get; set; }

    /// <summary>
    /// 用户在您平台上注册时的 IP 地址
    /// </summary>
    [JsonProperty("ip_address")]
    public string IpAddress { get; set; }

    /// <summary>
    /// 出生国家代码
    /// </summary>
    [JsonProperty("birth_country")]
    public string BirthCountry { get; set; }

    /// <summary>
    /// 公司注册日期
    /// </summary>
    [JsonProperty("company_registration_date")]
    public long? CompanyRegistrationDate { get; set; }

    /// <summary>
    /// 公司交易名称
    /// </summary>
    [JsonProperty("company_trading_name")]
    public string CompanyTradingName { get; set; }

    #region Business Person (业务人员信息)

    /// <summary>
    /// 业务人员城市
    /// </summary>
    [JsonProperty("business_person_city")]
    public string BusinessPersonCity { get; set; }

    /// <summary>
    /// 业务人员地址行
    /// </summary>
    [JsonProperty("business_person_address_line")]
    public string BusinessPersonAddressLine { get; set; }

    /// <summary>
    /// 业务人员邮编
    /// </summary>
    [JsonProperty("business_person_postal_code")]
    public string BusinessPersonPostalCode { get; set; }

    /// <summary>
    /// 业务人员名
    /// </summary>
    [JsonProperty("business_person_first_name")]
    public string BusinessPersonFirstName { get; set; }

    /// <summary>
    /// 业务人员中间名
    /// </summary>
    [JsonProperty("business_person_middle_name")]
    public string BusinessPersonMiddleName { get; set; }

    /// <summary>
    /// 业务人员姓
    /// </summary>
    [JsonProperty("business_person_last_name")]
    public string BusinessPersonLastName { get; set; }

    /// <summary>
    /// 业务人员出生日期
    /// </summary>
    [JsonProperty("business_person_birth_date")]
    public long? BusinessPersonBirthDate { get; set; }

    /// <summary>
    /// 业务人员电话
    /// </summary>
    [JsonProperty("business_person_phone")]
    public string BusinessPersonPhone { get; set; }

    /// <summary>
    /// 业务人员电子邮箱
    /// </summary>
    [JsonProperty("business_person_email")]
    public string BusinessPersonEmail { get; set; }

    /// <summary>
    /// 业务人员类型（可以是 CONTACT、DIRECTOR 或 UBO，多选字段）
    /// </summary>
    [JsonProperty("business_person_types")]
    public string BusinessPersonTypes { get; set; }

    /// <summary>
    /// 国籍
    /// </summary>
    [JsonProperty("nationality")]
    public string Nationality { get; set; }

    /// <summary>
    /// 交易州
    /// </summary>
    [JsonProperty("trading_state")]
    public string TradingState { get; set; }

    /// <summary>
    /// 业务人员国家
    /// </summary>
    [JsonProperty("business_person_country")]
    public string BusinessPersonCountry { get; set; }

    /// <summary>
    /// 业务人员州
    /// </summary>
    [JsonProperty("business_person_state")]
    public string BusinessPersonState { get; set; }

    /// <summary>
    /// 业务运营部门
    /// </summary>
    [JsonProperty("sector")]
    public string Sector { get; set; }

    /// <summary>
    /// 业务人员持股比例（百分比）
    /// </summary>
    [JsonProperty("business_person_ownership")]
    public float? BusinessPersonOwnership { get; set; }

    /// <summary>
    /// 业务人员证件类型
    /// </summary>
    [JsonProperty("business_person_id_type")]
    public string BusinessPersonIdType { get; set; }

    /// <summary>
    /// 业务人员税务参考号码
    /// </summary>
    [JsonProperty("business_person_tax_reference_number")]
    public string BusinessPersonTaxReferenceNumber { get; set; }

    #endregion

    #region Financial Information (财务信息)

    /// <summary>
    /// 税务参考号码
    /// </summary>
    [JsonProperty("tax_reference_number")]
    public string TaxReferenceNumber { get; set; }

    /// <summary>
    /// 公民身份国家
    /// </summary>
    [JsonProperty("citizenship")]
    public string Citizenship { get; set; }

    /// <summary>
    /// 就业状态
    /// </summary>
    [JsonProperty("employment_status")]
    public string EmploymentStatus { get; set; }

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
    /// 主要资金来源
    /// </summary>
    [JsonProperty("primary_source_of_funds")]
    public string PrimarySourceOfFunds { get; set; }

    /// <summary>
    /// 当资金来源为 OTHER 时必须填写
    /// </summary>
    [JsonProperty("primary_source_of_funds_description")]
    public string PrimarySourceOfFundsDescription { get; set; }

    /// <summary>
    /// 总资产
    /// </summary>
    [JsonProperty("total_assets")]
    public string TotalAssets { get; set; }

    /// <summary>
    /// 法币价值（美元计）
    /// </summary>
    [JsonProperty("usd_value_of_fiat")]
    public string UsdValueOfFiat { get; set; }

    /// <summary>
    /// 加密货币价值（美元计）
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
    /// 每月提款金额
    /// </summary>
    [JsonProperty("monthly_withdrawals")]
    public string MonthlyWithdrawals { get; set; }

    /// <summary>
    /// 每月加密货币提款金额
    /// </summary>
    [JsonProperty("monthly_crypto_withdrawals")]
    public string MonthlyCryptoWithdrawals { get; set; }

    /// <summary>
    /// 每月投资提款金额
    /// </summary>
    [JsonProperty("monthly_investment_withdrawal")]
    public string MonthlyInvestmentWithdrawal { get; set; }

    /// <summary>
    /// 每月加密货币投资提款金额
    /// </summary>
    [JsonProperty("monthly_crypto_investment_withdrawal")]
    public string MonthlyCryptoInvestmentWithdrawal { get; set; }

    /// <summary>
    /// 资金发送/接收司法管辖区
    /// </summary>
    [JsonProperty("funds_send_receive_jurisdictions")]
    public string FundsSendReceiveJurisdictions { get; set; }

    /// <summary>
    /// 供应商和对手方
    /// </summary>
    [JsonProperty("vendors_and_counterparties")]
    public string VendorsAndCounterparties { get; set; }

    /// <summary>
    /// 当就业状态为 OTHER 时必须填写
    /// </summary>
    [JsonProperty("employment_description")]
    public string EmploymentDescription { get; set; }

    /// <summary>
    /// 企业所从事的活动
    /// </summary>
    [JsonProperty("engage_in_activities")]
    public string EngageInActivities { get; set; }

    #endregion

    #region Company Information (企业信息)

    /// <summary>
    /// 企业税务参考号码
    /// </summary>
    [JsonProperty("company_tax_reference_number")]
    public string CompanyTaxReferenceNumber { get; set; }

    /// <summary>
    /// 企业实体类型
    /// </summary>
    [JsonProperty("company_entity_type")]
    public string CompanyEntityType { get; set; }

    /// <summary>
    /// 企业实体类型描述
    /// </summary>
    [JsonProperty("company_entity_type_description")]
    public string CompanyEntityTypeDescription { get; set; }

    /// <summary>
    /// 公司注册地址单元号
    /// </summary>
    [JsonProperty("company_registered_address_unit_number")]
    public string CompanyRegisteredAddressUnitNumber { get; set; }

    /// <summary>
    /// 公司注册地址行1
    /// </summary>
    [JsonProperty("company_registered_address_address_line1")]
    public string CompanyRegisteredAddressAddressLine1 { get; set; }

    /// <summary>
    /// 公司注册地址城市
    /// </summary>
    [JsonProperty("company_registered_address_city")]
    public string CompanyRegisteredAddressCity { get; set; }

    /// <summary>
    /// 公司注册地址州
    /// </summary>
    [JsonProperty("company_registered_address_state")]
    public string CompanyRegisteredAddressState { get; set; }

    /// <summary>
    /// 公司注册地址邮编
    /// </summary>
    [JsonProperty("company_registered_address_postal_code")]
    public string CompanyRegisteredAddressPostalCode { get; set; }

    /// <summary>
    /// 公司注册国家代码
    /// </summary>
    [JsonProperty("company_registered_address_country_code")]
    public string CompanyRegisteredAddressCountryCode { get; set; }

    /// <summary>
    /// NAICS 代码
    /// </summary>
    [JsonProperty("company_naics")]
    public string CompanyNaics { get; set; }

    /// <summary>
    /// NAICS 代码描述
    /// </summary>
    [JsonProperty("company_naics_description")]
    public string CompanyNaicsDescription { get; set; }

    /// <summary>
    /// 网站地址（不带 http）
    /// </summary>
    [JsonProperty("company_website_address")]
    public string CompanyWebsiteAddress { get; set; }

    /// <summary>
    /// 公司注册州
    /// </summary>
    [JsonProperty("company_state_of_incorporation")]
    public string CompanyStateOfIncorporation { get; set; }

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
    /// 公司资金发送/接收司法管辖区
    /// </summary>
    [JsonProperty("company_funds_send_receive_jurisdictions")]
    public string CompanyFundsSendReceiveJurisdictions { get; set; }

    /// <summary>
    /// 公司监管状态
    /// </summary>
    [JsonProperty("company_regulated_status")]
    public string CompanyRegulatedStatus { get; set; }

    #endregion

    #region Company Physical Address (公司实际地址)

    /// <summary>
    /// 公司实际地址行1
    /// </summary>
    [JsonProperty("company_physical_address_address_line1")]
    public string CompanyPhysicalAddressAddressLine1 { get; set; }

    /// <summary>
    /// 公司实际地址城市
    /// </summary>
    [JsonProperty("company_physical_address_city")]
    public string CompanyPhysicalAddressCity { get; set; }

    /// <summary>
    /// 公司实际地址州
    /// </summary>
    [JsonProperty("company_physical_address_state")]
    public string CompanyPhysicalAddressState { get; set; }

    /// <summary>
    /// 公司实际地址邮编
    /// </summary>
    [JsonProperty("company_physical_address_postal_code")]
    public string CompanyPhysicalAddressPostalCode { get; set; }

    /// <summary>
    /// 公司实际地址国家代码
    /// </summary>
    [JsonProperty("company_physical_address_country_code")]
    public string CompanyPhysicalAddressCountryCode { get; set; }

    #endregion

    #region Document IDs (文档 ID)

    /// <summary>
    /// 银行对账单文档 ID
    /// </summary>
    [JsonProperty("bank_statement_document_id")]
    public string BankStatementDocumentId { get; set; }

    /// <summary>
    /// 驾驶证背面文档 ID
    /// </summary>
    [JsonProperty("drivers_licence_back_document_id")]
    public string DriversLicenceBackDocumentId { get; set; }

    /// <summary>
    /// 驾驶证正面文档 ID
    /// </summary>
    [JsonProperty("drivers_licence_front_document_id")]
    public string DriversLicenceFrontDocumentId { get; set; }

    /// <summary>
    /// 身份证背面文档 ID
    /// </summary>
    [JsonProperty("identity_card_back_document_id")]
    public string IdentityCardBackDocumentId { get; set; }

    /// <summary>
    /// 身份证正面文档 ID
    /// </summary>
    [JsonProperty("identity_card_front_document_id")]
    public string IdentityCardFrontDocumentId { get; set; }

    /// <summary>
    /// 租赁协议文档 ID
    /// </summary>
    [JsonProperty("lease_agreement_document_id")]
    public string LeaseAgreementDocumentId { get; set; }

    /// <summary>
    /// 护照文档 ID
    /// </summary>
    [JsonProperty("passport_document_id")]
    public string PassportDocumentId { get; set; }

    /// <summary>
    /// 工资单文档 ID
    /// </summary>
    [JsonProperty("payslip_document_id")]
    public string PayslipDocumentId { get; set; }

    /// <summary>
    /// 水电费账单文档 ID
    /// </summary>
    [JsonProperty("utility_bill_document_id")]
    public string UtilityBillDocumentId { get; set; }

    /// <summary>
    /// 账户协议文档 ID
    /// </summary>
    [JsonProperty("account_agreement_document_id")]
    public string AccountAgreementDocumentId { get; set; }

    #endregion

    #region Company Document IDs (公司文档 ID)

    /// <summary>
    /// 公司银行对账单文档 ID
    /// </summary>
    [JsonProperty("company_bank_statement_document_id")]
    public string CompanyBankStatementDocumentId { get; set; }

    /// <summary>
    /// 公司资金银行对账单文档 ID
    /// </summary>
    [JsonProperty("company_funds_bank_statement_document_id")]
    public string CompanyFundsBankStatementDocumentId { get; set; }

    /// <summary>
    /// 公司水电费账单文档 ID
    /// </summary>
    [JsonProperty("company_utility_bill_document_id")]
    public string CompanyUtilityBillDocumentId { get; set; }

    /// <summary>
    /// 公司租赁协议文档 ID
    /// </summary>
    [JsonProperty("company_lease_agreement_document_id")]
    public string CompanyLeaseAgreementDocumentId { get; set; }

    /// <summary>
    /// 公司注册文档 ID
    /// </summary>
    [JsonProperty("company_incorporation_document_id")]
    public string CompanyIncorporationDocumentId { get; set; }

    /// <summary>
    /// 公司章程文档 ID
    /// </summary>
    [JsonProperty("company_articles_of_incorporation_document_id")]
    public string CompanyArticlesOfIncorporationDocumentId { get; set; }

    /// <summary>
    /// 公司实益所有权证书文档 ID
    /// </summary>
    [JsonProperty("company_beneficial_ownership_certificate_document_id")]
    public string CompanyBeneficialOwnershipCertificateDocumentId { get; set; }

    /// <summary>
    /// 公司账户协议文档 ID
    /// </summary>
    [JsonProperty("company_account_agreement_document_id")]
    public string CompanyAccountAgreementDocumentId { get; set; }

    /// <summary>
    /// 公司个人银行对账单文档 ID
    /// </summary>
    [JsonProperty("company_individual_bank_statement_document_id")]
    public string CompanyIndividualBankStatementDocumentId { get; set; }

    /// <summary>
    /// 公司个人水电费账单文档 ID
    /// </summary>
    [JsonProperty("company_individual_utility_bill_document_id")]
    public string CompanyIndividualUtilityBillDocumentId { get; set; }

    /// <summary>
    /// 公司个人租赁协议文档 ID
    /// </summary>
    [JsonProperty("company_individual_lease_agreement_document_id")]
    public string CompanyIndividualLeaseAgreementDocumentId { get; set; }

    /// <summary>
    /// 公司个人证件类型
    /// </summary>
    [JsonProperty("company_individual_id_document_type")]
    public string CompanyIndividualIdDocumentType { get; set; }

    #endregion

    #region Mailing Address (邮寄地址)

    /// <summary>
    /// 邮寄地址行
    /// </summary>
    [JsonProperty("mailing_address_address_line")]
    public string MailingAddressAddressLine { get; set; }

    /// <summary>
    /// 邮寄地址城市
    /// </summary>
    [JsonProperty("mailing_address_city")]
    public string MailingAddressCity { get; set; }

    /// <summary>
    /// 邮寄地址州
    /// </summary>
    [JsonProperty("mailing_address_state")]
    public string MailingAddressState { get; set; }

    /// <summary>
    /// 邮寄地址邮编
    /// </summary>
    [JsonProperty("mailing_address_postal_code")]
    public string MailingAddressPostalCode { get; set; }

    /// <summary>
    /// 邮寄地址国家代码
    /// </summary>
    [JsonProperty("mailing_address_country_code")]
    public string MailingAddressCountryCode { get; set; }

    #endregion
}
