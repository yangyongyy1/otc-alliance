using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 支付请求基类，包含商户认证信息（不参与 JSON 序列化）
/// </summary>
public abstract class BasePayRequest
{
    /// <summary>
    /// 商户 Key（不参与 JSON 序列化，仅用于生成签名头）
    /// </summary>
    [JsonIgnore]
    public string MerchantKey { get; set; }

    /// <summary>
    /// 商户 Secret（不参与 JSON 序列化，仅用于生成签名）
    /// </summary>
    [JsonIgnore]
    public string MerchantSecret { get; set; }
}

