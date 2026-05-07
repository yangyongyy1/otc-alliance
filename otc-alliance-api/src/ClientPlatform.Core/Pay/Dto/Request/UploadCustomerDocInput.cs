using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 上传客户文档输入参数
/// </summary>
public class UploadCustomerDocInput : BasePayRequest
{
    /// <summary>
    /// 客户 ID
    /// </summary>
    /// <summary>
    /// 客户 ID
    /// </summary>
    [JsonProperty("customer_id")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 文档类型 (例如: PASSPORT)
    /// </summary>
    [Required]
    [JsonProperty("document_type")]
    public string DocumentType { get; set; }

    /// <summary>
    /// 文件流
    /// </summary>
    [JsonIgnore]
    public Stream FileStream { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonIgnore]
    public string FileName { get; set; }
}
