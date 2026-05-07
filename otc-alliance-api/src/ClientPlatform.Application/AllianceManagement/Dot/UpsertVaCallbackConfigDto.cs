using System.Collections.Generic;

namespace ClientPlatform.AllianceManagement.Dot;

/// <summary>
/// 配置商户 VA 回调（转发到 SunPay /api/v3/VA/CallbackConfig）
/// </summary>
public class UpsertVaCallbackConfigDto
{
    /// <summary>
    /// 商户 ID
    /// </summary>
    public int MerchantId { get; set; }

    /// <summary>
    /// 当 ApiPath 为相对路径（以 / 开头）时必填，用于拼接完整回调地址
    /// </summary>
    public string CallbackBaseUrl { get; set; }

    /// <summary>
    /// 配置数组（同名会更新）
    /// </summary>
    public List<CommonBusinessApiCallBackConfigDto> CommonBusinessApiCallBackConfig { get; set; } = new();
}

public class CommonBusinessApiCallBackConfigDto
{
    public string ApiName { get; set; }

    /// <summary>
    /// Crypto / Cash / VA（不传默认 VA）
    /// </summary>
    public string ApiConfigType { get; set; }

    /// <summary>
    /// 相对路径或完整 URL
    /// </summary>
    public string ApiPath { get; set; }

    public List<ApiConfigValueDto> ApiConfigValue { get; set; } = new();
}

public class ApiConfigValueDto
{
    public string PowerType { get; set; }

    /// <summary>
    /// 功能编号数组（字符串形式，例如 \"0\"、\"1\"）
    /// </summary>
    public List<string> Functions { get; set; } = new();
}

