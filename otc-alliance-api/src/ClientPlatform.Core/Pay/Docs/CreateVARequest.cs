// using Abp.Runtime.Validation;
// // using CoinAPI.REST.V1;
// // using java.sql;
// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// // using VGPAY.Cash.Channels.Paywho.Dto;
// // using VGPAY.CustomFilters;

// namespace VGPAY.API.VACashPay.Request
// {
//     public class CreateVARequest : IShouldNormalize
//     {
//         /// <summary>
//         /// customer_id
//         /// </summary> 
//         [Required]
//         public Guid customer_id { get; set; }

//         /// <summary>
//         /// currency
//         /// </summary>
//         [Required]
//         [StringLength(3)]
//         public string currency { get; set; }

//         [StringLength(100)]
//         public string first_name { get; set; }
//         [StringLength(100)]
//         public string middle_name { get; set; }
//         [StringLength(100)]
//         public string last_name { get; set; }
//         //[StringLength(100)]
//         //public string account_name { get; set; }
//         /// <summary>
//         /// must be valid email address
//         /// </summary>
//         [StringLength(255)]
//         [EmailAddress(ErrorMessage = "Invalid Email")]
//         public string email { get; set; }
//         /// <summary>
//         /// //if value is prefilled must be in valid international format eg. "+386 40040040" (without spaces)
//         /// </summary>
//         [StringLength(100)]
//         public string phone { get; set; }
//         [StringLength(255)]
//         public string address_line { get; set; }

//         [StringLength(100)]
//         public string zip_code { get; set; }
//         [StringLength(100)]
//         public string city { get; set; }
//         [StringLength(100)]
//         public string state { get; set; }

//         [StringLength(20)]
//         public string country_code { get; set; }

//         public DateTime? birth_date { get; set; }


//         /// <summary>
//         /// 公司联系人
//         /// </summary>
//         [StringLength(100)]
//         public string company_contact { get; set; }

//         /// <summary>
//         /// 公司名称
//         /// </summary>
//         [StringLength(100)]
//         public string company_name { get; set; }

//         /// <summary>
//         /// 公司注册号码
//         /// </summary>
//         [StringLength(100)]
//         public string registration_number { get; set; }


//         /// <summary>
//         /// 公司交易地址 欧洲地区
//         /// </summary>
//         [StringLength(500)]
//         public string trading_address { get; set; }

//         /// <summary>
//         /// 公司交易城市 欧洲地区
//         /// </summary>
//         [StringLength(100)]
//         public string trading_city { get; set; }

//         /// <summary>
//         /// 公司交易国家 欧洲地区
//         /// </summary>
//         [StringLength(10)]
//         public string trading_country { get; set; }


//         /// <summary>
//         /// 公司类型
//         /// </summary>
//         [StringLength(100)]
//         public string company_type { get; set; }

//         /// <summary>
//         /// 邮编
//         /// </summary>
//         [StringLength(100)]
//         public string post_code { get; set; }

//         /// <summary>
//         /// 证件类型
//         /// </summary>
//         [StringLength(50)]
//         public string id_document_type { get; set; }

//         /// <summary>
//         /// 证件号码
//         /// </summary>
//         [StringLength(100)]
//         public string id_number { get; set; }

//         /// <summary>
//         /// 证件签发日期 
//         /// </summary>
//         public DateTime? id_issue_date { get; set; }

//         /// <summary>
//         /// 证件有效日期
//         /// </summary>
//         public DateTime? id_expiration_date { get; set; }

//         /// <summary>
//         /// 公司交易地址 邮编
//         /// </summary>
//         [StringLength(500)]
//         public string trading_postal_code { get; set; }
//         /// <summary>
//         /// IP address from where the user signed up on your platform
//         /// </summary>
//         [StringLength(100)]
//         public string ip_address { get; set; }
//         [StringLength(3)]
//         public string birth_country { get; set; }
//         /// <summary>
//         /// CompanyRegistrationDate
//         /// </summary>
//         public DateTime? company_registration_date { get; set; }

//         /// <summary>
//         /// Trading Company Name
//         /// </summary>
//         [StringLength(100)]
//         public string company_trading_name { get; set; }

//         /// <summary>
//         /// 公司资金来源证明
//         /// </summary>
//         public string company_proof_of_source_of_funds { get; set; }

//         #region businessPerson  企业个人/代表信息字段

//         [StringLength(100)]
//         public string business_person_city { get; set; }
//         [StringLength(50)]
//         public string business_person_address_line { get; set; }
//         [StringLength(100)]
//         public string business_person_postal_code { get; set; }
//         [StringLength(100)]
//         public string business_person_first_name { get; set; }
//         [StringLength(100)]
//         public string business_person_middle_name { get; set; }
//         [StringLength(100)]
//         public string business_person_last_name { get; set; }
//         public DateTime? business_person_birth_date { get; set; }
//         [StringLength(200)]
//         public string business_person_phone { get; set; }

//         [StringLength(500)]
//         [EmailAddress(ErrorMessage = "Invalid business person email")]
//         public string business_person_email { get; set; }
//         /// <summary>
//         /// Type of the Business Person. Can be CONTACT, DIRECTOR or UBO. Multiple choice field
//         /// </summary>

//         [StringLength(20)]
//         public string business_person_types { get; set; }
//         [StringLength(2)]
//         public string nationality { get; set; }
//         [StringLength(35)]
//         public string trading_state { get; set; }
//         [StringLength(2)]
//         public string business_person_country { get; set; }
//         [StringLength(35)]
//         public string business_person_state { get; set; }
//         /// <summary>
//         /// Sector in which business operates
//         /// </summary>
//         [StringLength(35)]
//         public string sector { get; set; }

//         /// <summary>
//         /// Amount of shares hold by the Business Person (in %)
//         /// </summary>
//         public float? business_person_ownership { get; set; }

//         /// <summary>
//         /// 企业代表 国籍
//         /// </summary>
//         public string business_person_citizenship { get; set; }

//         /// <summary>
//         /// 企业代表证件Id
//         /// </summary>
//         public string business_person_id_number { get; set; }

//         /// <summary>
//         /// 企业代表证件签发日期
//         /// </summary>
//         public DateTime? business_person_id_issue_date { get; set; }

//         /// <summary>
//         /// 企业代表证件有效日期
//         /// </summary>
//         public DateTime? business_person_id_expiration_date { get; set; }

//         /// <summary>
//         /// 企业代表角色
//         /// </summary>
//         public string business_person_role { get; set; }

//         /// <summary>
//         /// 企业代表是否是签名人
//         /// </summary>
//         public string business_person_is_signer { get; set; }

//         /// <summary>
//         /// 经营区域
//         /// </summary>
//         public string regions_of_operation { get; set; }

//         #endregion

//         #region 新增

//         /// <summary>
//         /// 税务参考号码
//         /// </summary>
//         public string tax_reference_number { get; set; }

//         /// <summary>
//         /// 公民身份国家
//         /// </summary>
//         public string citizenship { get; set; }

//         /// <summary>
//         /// 就业状态
//         /// </summary>
//         public string employment_status { get; set; }

//         /// <summary>
//         /// 雇主名称
//         /// </summary>
//         public string employer_name { get; set; }

//         /// <summary>
//         /// 职业
//         /// </summary>
//         public string occupation { get; set; }

//         /// <summary>
//         /// 主要资金来源
//         /// </summary>
//         public string primary_source_of_funds { get; set; }

//         /// <summary>
//         /// 当资金来源为 OTHER 时必须填写
//         /// </summary>
//         public string primary_source_of_funds_description { get; set; }

//         /// <summary>
//         /// 总资产
//         /// </summary>
//         public string total_assets { get; set; }

//         /// <summary>
//         /// 法币价值（美元计）
//         /// </summary>
//         public string usd_value_of_fiat { get; set; }

//         /// <summary>
//         /// 加密货币价值（美元计）
//         /// </summary>
//         public string usd_value_of_crypto { get; set; }

//         /// <summary>
//         /// 每月存款金额
//         /// </summary>
//         public string monthly_deposits { get; set; }

//         /// <summary>
//         /// 每月加密货币存款金额
//         /// </summary>
//         public string monthly_crypto_deposits { get; set; }

//         /// <summary>
//         /// 每月投资存款金额
//         /// </summary>
//         public string monthly_investment_deposit { get; set; }

//         /// <summary>
//         /// 每月加密货币投资存款金额
//         /// </summary>
//         public string monthly_crypto_investment_deposit { get; set; }

//         /// <summary>
//         /// 每月提款金额
//         /// </summary>
//         public string monthly_withdrawals { get; set; }

//         /// <summary>
//         /// 每月加密货币提款金额 
//         /// </summary>
//         public string monthly_crypto_withdrawals { get; set; }

//         /// <summary>
//         /// 每月加密货币提款金额 
//         /// </summary>
//         public string monthly_investment_withdrawal { get; set; }

//         /// <summary>
//         /// 每月加密货币投资提款金额 
//         /// </summary>
//         public string monthly_crypto_investment_withdrawal { get; set; }

//         /// <summary>
//         /// 预计每月 SWIFT 转账量
//         /// </summary>
//         public string monthly_swift_volume { get; set; }

//         /// <summary>
//         /// 客户支付的国际支付总数
//         /// </summary>
//         public string number_of_international_payments { get; set; }

//         /// <summary>
//         /// 客户支付的本地付款总数
//         /// </summary>
//         public string number_of_local_payments { get; set; }

//         /// <summary>
//         /// 参与的活动
//         /// </summary>
//         public string funds_send_receive_jurisdictions { get; set; }

//         /// <summary>
//         /// 供应商和对手方
//         /// </summary>
//         public string vendors_and_counterparties { get; set; }

//         /// <summary>
//         /// 当就业状态为 OTHER 时必须填写
//         /// </summary>
//         public string employment_description { get; set; }



//         /// <summary>
//         /// 企业所从事的活动
//         /// </summary>
//         public string engage_in_activities { get; set; }

//         /// <summary>
//         /// 企业税务参考号码
//         /// </summary>
//         public string company_tax_reference_number { get; set; }

//         /// <summary>
//         /// 企业实体类型
//         /// </summary>
//         public string company_entity_type { get; set; }

//         /// <summary>
//         /// 企业实体类型描述
//         /// </summary>
//         public string company_entity_type_description { get; set; }

//         /// <summary>
//         /// 公司注册地址单元
//         /// </summary>
//         public string company_registered_address_unit_number { get; set; }

//         /// <summary>
//         /// 公司注册地址行1
//         /// </summary>
//         public string company_registered_address_address_line1 { get; set; }

//         /// <summary>
//         /// 公司注册地址城市
//         /// </summary>
//         public string company_registered_address_city { get; set; }

//         /// <summary>
//         /// 公司注册地址州
//         /// </summary>
//         public string company_registered_address_state { get; set; }

//         /// <summary>
//         /// 公司注册地址邮编
//         /// </summary>
//         public string company_registered_address_postal_code { get; set; }

//         /// <summary>
//         /// 公司注册国家
//         /// </summary>
//         public string company_registered_address_country_code { get; set; }

//         /// <summary>
//         /// Naics 代码
//         /// </summary>
//         public string company_naics { get; set; }

//         /// <summary>
//         /// Naics 代码 描述
//         /// </summary>
//         public string company_naics_description { get; set; }

//         /// <summary>
//         /// 网站 不带http
//         /// </summary>
//         public string company_website_address { get; set; }

//         /// <summary>
//         /// 公司注冊州
//         /// </summary>
//         public string company_state_of_incorporation { get; set; }

//         /// <summary>
//         /// 公司主要业务
//         /// </summary>
//         public string company_primary_business { get; set; }

//         /// <summary>
//         /// 公司主要业务描述
//         /// </summary>
//         public string company_description_of_business_nature { get; set; }

//         /// <summary>
//         /// 公司业务司法范围
//         /// </summary>
//         public string company_business_jurisdictions { get; set; }

//         /// <summary>
//         /// 公司业务司法范围区
//         /// </summary>
//         public string company_funds_send_receive_jurisdictions { get; set; }

//         /// <summary>
//         /// 公司监管状态
//         /// </summary>
//         public string company_regulated_status { get; set; }

//         /// <summary>
//         /// 
//         /// </summary>
//         public string business_person_id_type { get; set; }

//         /// <summary>
//         /// 
//         /// </summary>
//         public string business_person_tax_reference_number { get; set; }

//         #endregion

//         #region physical address
//         /// <summary>
//         /// 公司注册地址行1
//         /// </summary>
//         public string company_physical_address_address_line1 { get; set; }

//         /// <summary>
//         /// 公司注册地址城市
//         /// </summary>
//         public string company_physical_address_city { get; set; }

//         /// <summary>
//         /// 公司注册地址州
//         /// </summary>
//         public string company_physical_address_state { get; set; }

//         /// <summary>
//         /// 公司注册地址邮编
//         /// </summary>
//         public string company_physical_address_postal_code { get; set; }

//         /// <summary>
//         /// 公司注册国家
//         /// </summary>
//         public string company_physical_address_country_code { get; set; }
//         #endregion
//         public string bank_statement_document_id { get; set; }
//         public string bank_statement_document { get; set; }
//         public string drivers_licence_back_document_id { get; set; }
//         public string drivers_licence_back_document { get; set; }

//         public string drivers_licence_front_document_id { get; set; }
//         public string drivers_licence_front_document { get; set; }
//         public string identity_card_back_document_id { get; set; }
//         public string identity_card_back_document { get; set; }
//         public string identity_card_front_document_id { get; set; }
//         public string identity_card_front_document { get; set; }
//         public string lease_agreement_document_id { get; set; }
//         public string lease_agreement_document { get; set; }
//         public string passport_document_id { get; set; }
//         public string passport_document { get; set; }
//         public string payslip_document_id { get; set; }
//         public string payslip_document { get; set; }
//         public string utility_bill_document_id { get; set; }
//         public string utility_bill_document { get; set; }
//         #region company document  企业文档相关
//         public string company_bank_statement_document_id { get; set; }
//         public string company_bank_statement_document { get; set; }
//         //  public string company_funds_bank_statement_document_id { get; set; }

//         public string company_source_of_funds_bank_statement_document_id { get; set; }
//         public string company_source_of_funds_bank_statement_document { get; set; }

//         public string company_source_of_funds_financial_statement_document_id { get; set; }
//         public string company_source_of_funds_financial_statement_document { get; set; }
//         public string company_source_of_funds_term_sheet_document_id { get; set; }

//         public string company_source_of_funds_term_sheet_document { get; set; }
//         public string company_utility_bill_document_id { get; set; }
//         public string company_utility_bill_document { get; set; }
//         public string company_lease_agreement_document_id { get; set; }
//         public string company_lease_agreement_document { get; set; }
//         public string company_incorporation_document_id { get; set; }
//         public string company_incorporation_document { get; set; }
//         public string company_articles_of_incorporation_document_id { get; set; }
//         public string company_articles_of_incorporation_document { get; set; }
//         public string company_beneficial_ownership_certificate_document_id { get; set; }
//         public string company_beneficial_ownership_certificate_document { get; set; }
//         public string company_account_agreement_document_id { get; set; }
//         public string company_account_agreement_document { get; set; }
//         public string company_individual_bank_statement_document_id { get; set; }
//         public string company_individual_bank_statement_document { get; set; }
//         public string company_individual_utility_bill_document_id { get; set; }
//         public string company_individual_utility_bill_document { get; set; }
//         public string company_individual_lease_agreement_document_id { get; set; }
//         public string company_individual_lease_agreement_document { get; set; }
//         public string account_agreement_document_id { get; set; }
//         public string account_agreement_document { get; set; }
//         public string company_regulatory_license_document_id { get; set; }
//         public string company_regulatory_license_document { get; set; }
//         public string company_proof_of_ownership_document_id { get; set; }
//         public string company_proof_of_ownership_document { get; set; }
//         public string company_proof_of_registration_document_id { get; set; }
//         public string company_proof_of_registration_document { get; set; }
//         /// <summary>
//         /// 企业税务参考号码文档
//         /// </summary>
//         public string company_tax_reference_number_document_id { get; set; }
//         public string company_tax_reference_number_document { get; set; }
//         public string company_agreement_document_id { get; set; }
//         public string company_agreement_document { get; set; }


//         /// <summary>
//         /// infinituspay 渠道地址证明
//         /// </summary>
//         public string proof_of_address_document_id { get; set; }
//         public string proof_of_address_document { get; set; }

//         public string company_registration_document_id { get; set; }

//         public string company_registration_document { get; set; }

//         public string company_wolfsberg_questionnaire_document_id { get; set; }

//         public string company_wolfsberg_questionnaire_document { get; set; }

//         public string company_msb_flow_form_document_id { get; set; }

//         public string company_msb_flow_form_document { get; set; }

//         #endregion

//         public string mailing_address_address_line { get; set; }
//         public string mailing_address_city { get; set; }
//         public string mailing_address_state { get; set; }
//         public string mailing_address_postal_code { get; set; }
//         public string mailing_address_country_code { get; set; }

//         public string company_individual_id_document_type { get; set; }

//         /// <summary>
//         /// 公司联系人
//         /// </summary>
//         [StringLength(100)]
//         public string company_contact_first_name { get; set; }

//         /// <summary>
//         /// 公司联系人
//         /// </summary>
//         [StringLength(100)]
//         public string company_contact_last_name { get; set; }


//         /// <summary>
//         /// 公司联系人电话
//         /// </summary>
//         [StringLength(100)]
//         public string company_contact_phone { get; set; }

//         /// <summary>
//         /// 公司联系人邮箱
//         /// </summary>
//         [StringLength(100)]
//         public string company_contact_email { get; set; }

//         /// <summary>
//         /// 公司联系人FirstName
//         /// </summary>
//         public string CompanyContactFirstName { get; set; }

//         /// <summary>
//         /// 公司联系人LastName
//         /// </summary>
//         public string CompanyContactLastName { get; set; }

//         /// <summary>
//         /// 公司联系人电话
//         /// </summary>
//         public string CompanyContactPhone { get; set; }

//         /// <summary>
//         /// 公司联系人邮箱
//         /// </summary>
//         public string CompanyContactEmail { get; set; }


//         /// <summary>
//         /// 企业经营国家
//         /// </summary>
//         public List<string> countries_of_operation { get; set; }



//         public void Normalize()
//         {
//             first_name = first_name?.Trim();
//             middle_name = middle_name?.Trim();
//             last_name = last_name?.Trim();
//             company_name = company_name?.Trim();
//             id_document_type = id_document_type?.Trim();
//             id_number = id_number?.Trim();
//             registration_number = registration_number?.Trim();
//         }
//     }


// }
