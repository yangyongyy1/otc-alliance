using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 获取账户必填项输入参数
/// </summary>
public class GetAccountRequiredFieldsInput : BasePayRequest
{
    /// <summary>
    /// 币种 (例如: EUR)
    /// </summary>
    [Required]
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// 客户 ID (可选，系统会自动查找)
    /// </summary>
    [JsonProperty("customer_id")]
    public string CustomerId { get; set; }
}
