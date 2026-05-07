using System;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response;

/// <summary>
/// SunPay 本地支付（PAYIN 收银台）创建订单响应数据
/// 对应 PayApiResponse&lt;T&gt; 中的 data 字段
/// </summary>
public class SunPayLocalPayCreateResponse
{
    /// <summary>
    /// SunPay 渠道订单号
    /// </summary>
    [JsonProperty("order_no")]
    public string OrderNo { get; set; }

    /// <summary>
    /// 商户订单号（请求时的 out_order_no）
    /// </summary>
    [JsonProperty("out_order_no")]
    public string OutOrderNo { get; set; }

    /// <summary>
    /// 订单状态（字符串，如：PREPLACE、SUCCESS 等）
    /// </summary>
    [JsonProperty("order_status")]
    public string OrderStatus { get; set; }

    /// <summary>
    /// 收银台支付链接（前端需要跳转的 URL）
    /// </summary>
    [JsonProperty("payment_url")]
    public string PaymentUrl { get; set; }

    /// <summary>
    /// 商户侧用户标识
    /// </summary>
    [JsonProperty("out_user_id")]
    public string OutUserId { get; set; }

    /// <summary>
    /// 支付金额
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 币种
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// 支付方式（如：CARD）
    /// </summary>
    [JsonProperty("payment_type")]
    public string PaymentType { get; set; }

    /// <summary>
    /// 付款人姓名
    /// </summary>
    [JsonProperty("customer_name")]
    public string CustomerName { get; set; }

    /// <summary>
    /// 付款人邮箱
    /// </summary>
    [JsonProperty("email_address")]
    public string EmailAddress { get; set; }

    /// <summary>
    /// 订单名称 / 备注
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 支付成功后跳转地址
    /// </summary>
    [JsonProperty("redirect_url")]
    public string RedirectUrl { get; set; }

    /// <summary>
    /// 支付取消后跳转地址
    /// </summary>
    [JsonProperty("cancel_url")]
    public string CancelUrl { get; set; }

    /// <summary>
    /// 过期时间（秒）
    /// </summary>
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 过期时间（Unix 毫秒时间戳）
    /// </summary>
    [JsonProperty("expires_at")]
    public long? ExpiresAt { get; set; }
}

