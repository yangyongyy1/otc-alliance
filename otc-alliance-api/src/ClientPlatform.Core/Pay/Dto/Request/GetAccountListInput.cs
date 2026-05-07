using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 获取账户列表输入参数
/// </summary>
public class GetAccountListInput : BasePayRequest
{
    /// <summary>
    /// 客户 ID
    /// </summary>
    [JsonProperty("customer_id")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 账户 ID (可选)
    /// </summary>
    [JsonProperty("account_id")]
    public string AccountId { get; set; }

    /// <summary>
    /// 币种 (可选)
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    [JsonProperty("page_index")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    [JsonProperty("page_size")]
    public int Limit { get; set; } = 20;
}
