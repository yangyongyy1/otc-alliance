using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 查询客户输入参数
/// </summary>
public class QueryCustomerInput : BasePayRequest
{
    /// <summary>
    /// 客户 ID (可选)
    /// </summary>
    [JsonProperty("customer_id")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 外部用户 ID (可选)
    /// </summary>
    [JsonProperty("out_user_id")]
    public string OutUserId { get; set; }
}
