using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// SunPay VA 回调配置请求（POST /api/v3/VA/CallbackConfig）
/// </summary>
public class UpsertBusinessApiCallBackConfigInput : BasePayRequest
{
    /// <summary>
    /// 当 apiPath 为相对路径（以 / 开头）时必填，用于拼接完整回调地址
    /// </summary>
    [JsonProperty("callbackBaseUrl")]
    public string CallbackBaseUrl { get; set; }

    /// <summary>
    /// 回调配置数组（同名会更新）
    /// </summary>
    [JsonProperty("commonBusinessApiCallBackConfig")]
    public List<CommonBusinessApiCallBackConfigItem> CommonBusinessApiCallBackConfig { get; set; } = new();
}

/// <summary>
/// 单条回调配置
/// </summary>
public class CommonBusinessApiCallBackConfigItem
{
    /// <summary>
    /// 配置名称（同名会更新）
    /// </summary>
    [JsonProperty("apiName")]
    public string ApiName { get; set; }

    /// <summary>
    /// 配置类型：Crypto / Cash / VA（不传默认 VA）
    /// </summary>
    [JsonProperty("apiConfigType")]
    public string ApiConfigType { get; set; } = "VA";

    /// <summary>
    /// 相对路径或完整 URL
    /// </summary>
    [JsonProperty("apiPath")]
    public string ApiPath { get; set; }

    /// <summary>
    /// 回调能力配置
    /// </summary>
    [JsonProperty("apiConfigValue")]
    public List<ApiConfigValueItem> ApiConfigValue { get; set; } = new();
}

/// <summary>
/// 回调能力配置项
/// </summary>
public class ApiConfigValueItem
{
    /// <summary>
    /// 能力类型，例如 va、LocalPayments
    /// </summary>
    [JsonProperty("PowerType")]
    public string PowerType { get; set; }

    /// <summary>
    /// 功能编号数组（字符串形式，例如 \"0\"、\"1\"）
    /// </summary>
    [JsonProperty("Functions")]
    public List<string> Functions { get; set; } = new();
}

