using System.Text.Json.Serialization;

namespace ClientPlatform.Pay.Dto.Callback;

/// <summary>
/// SunPay 回调请求基类
/// </summary>
public class SunPayCallbackRequest
{
    /// <summary>
    /// 业务状态：SUCCESS/FAILED
    /// </summary>
    [JsonPropertyName("biz_status")]
    public string BizStatus { get; set; }

    /// <summary>
    /// 业务类型：CreateCustomer/CreateAccount/UpdateCustomer/UpdateAccount
    /// </summary>
    [JsonPropertyName("biz_type")]
    public string BizType { get; set; }

    /// <summary>
    /// 错误信息（失败时）
    /// </summary>
    [JsonPropertyName("msg")]
    public string Message { get; set; }

    /// <summary>
    /// 判断是否成功
    /// </summary>
    public bool IsSuccess => BizStatus?.ToUpper() == "SUCCESS";
}

/// <summary>
/// SunPay 创建客户回调请求
/// </summary>
public class SunPayCreateCustomerCallbackRequest : SunPayCallbackRequest
{
    /// <summary>
    /// 客户数据
    /// </summary>
    [JsonPropertyName("data")]
    public SunPayCustomerCallbackData Data { get; set; }
}

/// <summary>
/// SunPay 客户回调数据
/// </summary>
public class SunPayCustomerCallbackData
{
    /// <summary>
    /// 渠道客户 ID
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 客户状态：ACTIVE/PENDING/FROZEN/DISABLED/FAILED
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// 外部用户 ID（我们传给渠道的用户 ID）
    /// </summary>
    [JsonPropertyName("out_user_id")]
    public string OutUserId { get; set; }

    /// <summary>
    /// 名
    /// </summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    /// <summary>
    /// 中间名
    /// </summary>
    [JsonPropertyName("middle_name")]
    public string MiddleName { get; set; }

    /// <summary>
    /// 姓
    /// </summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    /// <summary>
    /// 国家代码
    /// </summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }

    /// <summary>
    /// 公司名称（企业客户）
    /// </summary>
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; }

    /// <summary>
    /// 客户邮箱
    /// </summary>
    [JsonPropertyName("customer_email")]
    public string CustomerEmail { get; set; }

    /// <summary>
    /// 客户类型：INDIVIDUAL/COMPANY
    /// </summary>
    [JsonPropertyName("customer_type")]
    public string CustomerType { get; set; }

    /// <summary>
    /// KYC 等级
    /// </summary>
    [JsonPropertyName("kyc_level")]
    public string KycLevel { get; set; }

    /// <summary>
    /// 创建时间（Unix 时间戳）
    /// </summary>
    [JsonPropertyName("creation_time")]
    public long CreationTime { get; set; }
}
