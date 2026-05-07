using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 获取客户必填项输入参数
/// </summary>
public class GetCustomerRequiredFieldsInput : BasePayRequest
{
    /// <summary>
    /// 客户类型 (例如: INDIVIDUAL)
    /// </summary>
    [Required]
    [JsonProperty("customer_type")]
    public string CustomerType { get; set; }

    /// <summary>
    /// 国家代码 (例如: GB)
    /// </summary>
    [Required]
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }
}
