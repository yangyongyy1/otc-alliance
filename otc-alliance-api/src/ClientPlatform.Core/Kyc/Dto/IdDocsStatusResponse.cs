using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClientPlatform.Kyc.Dto
{
    /// <summary>
    /// Sumsub GetIdDocsStatus API 响应
    /// 字典结构，键为文档类型（APPLICANT_DATA, IDENTITY, QUESTIONNAIRE 等）
    /// </summary>
    public class IdDocsStatusResponse : Dictionary<string, IdDocStatus>
    {
        // 自定义属性：标识API调用是否成功
        public bool Success { get; set; }
    }

    /// <summary>
    /// ID 文档状态信息
    /// </summary>
    public class IdDocStatus
    {
        [JsonProperty("reviewResult")]
        public IdDocReviewResult ReviewResult { get; set; }

        [JsonProperty("imageIds")]
        public List<long> ImageIds { get; set; }

        // 以下字段主要用于 IDENTITY 类型
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("idDocType")]
        public string IdDocType { get; set; }

        [JsonProperty("imageReviewResults")]
        public Dictionary<string, IdDocImageReviewResult> ImageReviewResults { get; set; }

        [JsonProperty("forbidden")]
        public bool? Forbidden { get; set; }

        [JsonProperty("masked")]
        public bool? Masked { get; set; }

        [JsonProperty("digital")]
        public bool? Digital { get; set; }

        [JsonProperty("imageStatuses")]
        public List<string> ImageStatuses { get; set; }

        [JsonProperty("attemptId")]
        public string AttemptId { get; set; }
    }

    /// <summary>
    /// ID 文档审核结果
    /// </summary>
    public class IdDocReviewResult
    {
        [JsonProperty("reviewAnswer")]
        public string ReviewAnswer { get; set; }
    }

    /// <summary>
    /// 图片审核结果
    /// </summary>
    public class IdDocImageReviewResult
    {
        [JsonProperty("reviewAnswer")]
        public string ReviewAnswer { get; set; }

        [JsonProperty("clientComment")]
        public string ClientComment { get; set; }
    }
}
