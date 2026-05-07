using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    /// <summary>
    /// 获取账户列表响应
    /// </summary>
    public class GetAccountsResponse
    {
        /// <summary>
        /// 账户 ID
        /// </summary>
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// 虚拟银行账户 (IBAN)
        /// </summary>
        [JsonProperty("iban")]
        public string Iban { get; set; }

        /// <summary>
        /// 银行识别码 (BIC)
        /// </summary>
        [JsonProperty("swift_bic")]
        public string SwiftBic { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// 银行所在国家
        /// </summary>
        [JsonProperty("bank_country")]
        public string BankCountry { get; set; }

        /// <summary>
        /// 银行地址
        /// </summary>
        [JsonProperty("bank_address")]
        public string BankAddress { get; set; }

        /// <summary>
        /// 账户号
        /// </summary>
        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        [JsonProperty("bank_name")]
        public string BankName { get; set; }

        /// <summary>
        /// 可用余额
        /// </summary>
        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        /// <summary>
        /// 冻结金额
        /// </summary>
        [JsonProperty("freeze_balance")]
        public decimal FreezeBalance { get; set; }

        /// <summary>
        /// 路由代码条目列表
        /// </summary>
        [JsonProperty("routing_code_entries")]
        public List<RoutingCodeEntry> RoutingCodeEntries { get; set; }

        /// <summary>
        /// 银行账户持有人姓名
        /// </summary>
        [JsonProperty("bank_account_holder_name")]
        public string BankAccountHolderName { get; set; }

        /// <summary>
        /// 存款指引信息 (包含 ACH / Fedwire / Swift 等数据)
        /// </summary>
        [JsonProperty("deposit_instructions")]
        public DepositInstructions DepositInstructions { get; set; }
    }

    /// <summary>
    /// 路由代码条目
    /// </summary>
    public class RoutingCodeEntry
    {
        /// <summary>
        /// 路由代码键
        /// </summary>
        [JsonProperty("routing_code_key")]
        public string RoutingCodeKey { get; set; }

        /// <summary>
        /// 路由代码值
        /// </summary>
        [JsonProperty("routing_code_value")]
        public string RoutingCodeValue { get; set; }
    }

    /// <summary>
    /// 存款指引
    /// </summary>
    public class DepositInstructions
    {
        /// <summary>
        /// ACH 存款指引
        /// </summary>
        [JsonProperty("ach")]
        public ACH_Extend Ach { get; set; }

        /// <summary>
        /// Fedwire 存款指引
        /// </summary>
        [JsonProperty("fedwire")]
        public FEDWIRE_Extend Fedwire { get; set; }

        /// <summary>
        /// SWIFT 存款指引
        /// </summary>
        [JsonProperty("swift")]
        public SWIFT_Extend Swift { get; set; }
    }

    /// <summary>
    /// ACH 扩展信息
    /// </summary>
    public class ACH_Extend
    {
        /// <summary>
        /// 账户详情
        /// </summary>
        [JsonProperty("account_detail")]
        public RailAccountDetail AccountDetail { get; set; }

        /// <summary>
        /// 机构信息
        /// </summary>
        [JsonProperty("institution_information")]
        public RailInstitutionAddress InstitutionInformation { get; set; }
    }

    /// <summary>
    /// Fedwire 扩展信息
    /// </summary>
    public class FEDWIRE_Extend
    {
        /// <summary>
        /// 账户详情
        /// </summary>
        [JsonProperty("account_detail")]
        public RailAccountDetail AccountDetail { get; set; }

        /// <summary>
        /// 机构信息
        /// </summary>
        [JsonProperty("institution_information")]
        public RailInstitutionAddress InstitutionInformation { get; set; }

        /// <summary>
        /// 账户持有人地址
        /// </summary>
        [JsonProperty("account_holder_address")]
        public RailAccountHolderAddress AccountHolderAddress { get; set; }
    }

    /// <summary>
    /// SWIFT 扩展信息
    /// </summary>
    public class SWIFT_Extend
    {
        /// <summary>
        /// 账户详情
        /// </summary>
        [JsonProperty("account_detail")]
        public RailAccountDetail AccountDetail { get; set; }

        /// <summary>
        /// 机构信息
        /// </summary>
        [JsonProperty("institution_information")]
        public RailInstitutionAddress InstitutionInformation { get; set; }

        /// <summary>
        /// 账户持有人地址
        /// </summary>
        [JsonProperty("account_holder_address")]
        public RailAccountHolderAddress AccountHolderAddress { get; set; }
    }

    /// <summary>
    /// 银行机构地址信息
    /// </summary>
    public class RailInstitutionAddress
    {
        /// <summary>
        /// 机构名称
        /// </summary>
        [JsonProperty("institution_name")]
        public string InstitutionName { get; set; }

        /// <summary>
        /// 地址行 1
        /// </summary>
        [JsonProperty("address_line1")]
        public string AddressLine1 { get; set; }

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
        /// 邮政编码
        /// </summary>
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
    }

    /// <summary>
    /// 账户持有人地址信息
    /// </summary>
    public class RailAccountHolderAddress
    {
        /// <summary>
        /// 地址行 1
        /// </summary>
        [JsonProperty("address_line1")]
        public string AddressLine1 { get; set; }

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
        /// 邮政编码
        /// </summary>
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
    }

    /// <summary>
    /// 账户详情
    /// </summary>
    public class RailAccountDetail
    {
        /// <summary>
        /// 账户持有人姓名
        /// </summary>
        [JsonProperty("account_holder_name")]
        public string AccountHolderName { get; set; }

        /// <summary>
        /// 账户号
        /// </summary>
        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 账户路由号码
        /// </summary>
        [JsonProperty("account_routing_number")]
        public string AccountRoutingNumber { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty("memo")]
        public string Memo { get; set; }

        /// <summary>
        /// 国际银行账户号码 (IBAN)
        /// </summary>
        [JsonProperty("iban")]
        public string Iban { get; set; }

        /// <summary>
        /// 银行识别码 (SWIFT/BIC)
        /// </summary>
        [JsonProperty("swift_bic")]
        public string SwiftBic { get; set; }

        /// <summary>
        /// 中介机构 SWIFT/BIC
        /// </summary>
        [JsonProperty("intermediary_institution_swift_bic")]
        public string IntermediaryInstitutionSwiftBic { get; set; }
    }
}
