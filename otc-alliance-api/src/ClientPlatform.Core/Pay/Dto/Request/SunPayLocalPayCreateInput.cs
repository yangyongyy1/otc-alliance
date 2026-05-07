using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// SunPay 本地支付（PAYIN 收银台）创建订单请求参数
/// 对应 SunPay 文档中的收银台下单入参
/// </summary>
public class SunPayLocalPayCreateInput : BasePayRequest
{
    /// <summary>
    /// 商户订单号（你方平台生成的订单号）
    /// </summary>
    [JsonProperty("out_order_no")]
    public string OutOrderNo { get; set; }

    /// <summary>
    /// 商户侧用户标识（你方用户 ID）
    /// </summary>
    [JsonProperty("out_user_id")]
    public string OutUserId { get; set; }

    /// <summary>
    /// 支付金额
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 币种（如：USD、AED）
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// 支付方式（如：CARD、LOCAL_BANK_TRANSFER 等）
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
    /// 订单名称 / 备注（例如产品名称）
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
    /// 异步回调地址（SunPay 支付结果通知）
    /// </summary>
    [JsonProperty("webhook_url")]
    public string WebhookUrl { get; set; }

    /// <summary>
    /// 交易参考号（客户端传入，原样带给 SunPay）
    /// </summary>
    [JsonProperty("reference")]
    public string Reference { get; set; }
}

