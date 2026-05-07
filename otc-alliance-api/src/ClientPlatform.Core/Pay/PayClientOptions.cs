using System.Collections.Generic;

namespace ClientPlatform.Pay;

/// <summary>
/// 支付客户端配置选项
/// </summary>
public class PayClientOptions
{
    /// <summary>
    /// API 基础地址 (例如: https://sandbox-oapi.sunpay.pro)
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// 商户配置列表
    /// </summary>
    public List<PayMerchantOption> Merchants { get; set; } = new List<PayMerchantOption>();

    /// <summary>
    /// HTTP 请求头配置
    /// </summary>
    public PayClientHeaderOptions Headers { get; set; } = new PayClientHeaderOptions();
}

/// <summary>
/// 支付客户端 HTTP 请求头配置
/// </summary>
public class PayClientHeaderOptions
{
    /// <summary>
    /// 商户 Key 头名称 (默认: SunPay-Key)
    /// </summary>
    public string KeyHeaderName { get; set; } = "SunPay-Key";

    /// <summary>
    /// 时间戳头名称 (默认: SunPay-Timestamp)
    /// </summary>
    public string TimestampHeaderName { get; set; } = "SunPay-Timestamp";

    /// <summary>
    /// 随机字符串头名称 (默认: SunPay-Nonce)
    /// </summary>
    public string NonceHeaderName { get; set; } = "SunPay-Nonce";

    /// <summary>
    /// 签名头名称 (默认: SunPay-Sign)
    /// </summary>
    public string SignHeaderName { get; set; } = "SunPay-Sign";
}

/// <summary>
/// 商户配置项
/// </summary>
public class PayMerchantOption
{
    /// <summary>
    /// 邀请码 (用于区分商户)
    /// </summary>
    public string InvitationCode { get; set; }

    /// <summary>
    /// 商户 Key (公钥)
    /// </summary>
    public string MerchantKey { get; set; }

    /// <summary>
    /// 商户 Secret (私钥/密钥)
    /// </summary>
    public string MerchantSecret { get; set; }
}
