using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClientPlatform.Pay.Dto.Callback;

/// <summary>
/// SunPay 创建账户回调请求
/// </summary>
public class SunPayCreateAccountCallbackRequest : SunPayCallbackRequest
{
    /// <summary>
    /// 账户数据
    /// </summary>
    [JsonPropertyName("data")]
    public SunPayAccountCallbackData Data { get; set; }
}

/// <summary>
/// SunPay 账户回调数据
/// </summary>
public class SunPayAccountCallbackData
{
    /// <summary>
    /// 渠道客户 ID
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 币种
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// 外部用户 ID（我们传给渠道的用户 ID）
    /// </summary>
    [JsonPropertyName("out_user_id")]
    public string OutUserId { get; set; }

    /// <summary>
    /// 渠道账户 ID
    /// </summary>
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; }

    /// <summary>
    /// IBAN (EUR 账户)
    /// </summary>
    [JsonPropertyName("iban")]
    public string Iban { get; set; }

    /// <summary>
    /// SWIFT BIC 代码
    /// </summary>
    [JsonPropertyName("swift_bic")]
    public string SwiftBic { get; set; }

    /// <summary>
    /// 路由代码
    /// </summary>
    [JsonPropertyName("rounting_code")]
    public string RoutingCode { get; set; }

    /// <summary>
    /// 银行账号
    /// </summary>
    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; }

    /// <summary>
    /// 银行账户持有人姓名
    /// </summary>
    [JsonPropertyName("bank_account_holder_name")]
    public string BankAccountHolderName { get; set; }

    /// <summary>
    /// 账户状态：ACTIVE/PENDING/FAILED/SUSPENDED/CLOSED
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// 银行国家
    /// </summary>
    [JsonPropertyName("bank_country")]
    public string BankCountry { get; set; }

    /// <summary>
    /// 银行地址
    /// </summary>
    [JsonPropertyName("bank_address")]
    public string BankAddress { get; set; }

    /// <summary>
    /// 银行名称
    /// </summary>
    [JsonPropertyName("bank_name")]
    public string BankName { get; set; }

    /// <summary>
    /// 余额
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    /// <summary>
    /// 冻结余额
    /// </summary>
    [JsonPropertyName("freeze_balance")]
    public decimal FreezeBalance { get; set; }

    /// <summary>
    /// 路由代码条目列表
    /// </summary>
    [JsonPropertyName("routing_code_entries")]
    public List<object> RoutingCodeEntries { get; set; }

    /// <summary>
    /// 存款指令（USD 账户）
    /// </summary>
    [JsonPropertyName("deposit_instructions")]
    public DepositInstructions DepositInstructions { get; set; }
}

/// <summary>
/// 存款指令（USD 账户特有）
/// </summary>
public class DepositInstructions
{
    /// <summary>
    /// ACH 支付方式详情
    /// </summary>
    [JsonPropertyName("ach")]
    public PaymentMethodDetail Ach { get; set; }

    /// <summary>
    /// Fedwire 支付方式详情
    /// </summary>
    [JsonPropertyName("fedwire")]
    public PaymentMethodDetail Fedwire { get; set; }

    /// <summary>
    /// SWIFT 支付方式详情
    /// </summary>
    [JsonPropertyName("swift")]
    public PaymentMethodDetail Swift { get; set; }
}

/// <summary>
/// 支付方式详情
/// </summary>
public class PaymentMethodDetail
{
    /// <summary>
    /// 账户详情
    /// </summary>
    [JsonPropertyName("account_detail")]
    public AccountDetail AccountDetail { get; set; }

    /// <summary>
    /// 机构信息
    /// </summary>
    [JsonPropertyName("institution_information")]
    public InstitutionInformation InstitutionInformation { get; set; }

    /// <summary>
    /// 账户持有人地址（Fedwire/SWIFT）
    /// </summary>
    [JsonPropertyName("account_holder_address")]
    public Address AccountHolderAddress { get; set; }
}

/// <summary>
/// 账户详情
/// </summary>
public class AccountDetail
{
    /// <summary>
    /// 账户持有人姓名
    /// </summary>
    [JsonPropertyName("account_holder_name")]
    public string AccountHolderName { get; set; }

    /// <summary>
    /// 账户号码
    /// </summary>
    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; }

    /// <summary>
    /// 账户路由号码 (ACH/Fedwire)
    /// </summary>
    [JsonPropertyName("account_routing_number")]
    public string AccountRoutingNumber { get; set; }

    /// <summary>
    /// 备注/参考号
    /// </summary>
    [JsonPropertyName("memo")]
    public string Memo { get; set; }

    /// <summary>
    /// SWIFT BIC 代码
    /// </summary>
    [JsonPropertyName("swift_bic")]
    public string SwiftBic { get; set; }

    /// <summary>
    /// 中介机构 SWIFT BIC
    /// </summary>
    [JsonPropertyName("intermediary_institution_swift_bic")]
    public string IntermediaryInstitutionSwiftBic { get; set; }
}

/// <summary>
/// 机构信息
/// </summary>
public class InstitutionInformation
{
    /// <summary>
    /// 机构名称
    /// </summary>
    [JsonPropertyName("institution_name")]
    public string InstitutionName { get; set; }

    /// <summary>
    /// 地址第一行
    /// </summary>
    [JsonPropertyName("address_line1")]
    public string AddressLine1 { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [JsonPropertyName("city")]
    public string City { get; set; }

    /// <summary>
    /// 州/省
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    /// <summary>
    /// 国家代码
    /// </summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }
}

/// <summary>
/// 地址信息
/// </summary>
public class Address
{
    /// <summary>
    /// 地址第一行
    /// </summary>
    [JsonPropertyName("address_line1")]
    public string AddressLine1 { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [JsonPropertyName("city")]
    public string City { get; set; }

    /// <summary>
    /// 州/省
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    /// <summary>
    /// 国家代码
    /// </summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }
}
