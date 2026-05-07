using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response;

/// <summary>
/// 支付 API 通用响应包装类
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class PayApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [JsonProperty("is_success")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 状态码 (200 表示成功)
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 消息提示
    /// </summary>
    [JsonProperty("msg")]
    public string Msg { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonProperty("data")]
    public T Data { get; set; }

    /// <summary>
    /// 签名 (用于验证响应完整性)
    /// </summary>
    [JsonProperty("sign")]
    public string Sign { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    /// <summary>
    /// 随机串
    /// </summary>
    [JsonProperty("nonce")]
    public string Nonce { get; set; }
}
