using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    /// <summary>
    /// 通用文件上传响应
    /// </summary>
    public class UploadFileResponse
    {
        /// <summary>
        /// 文件 ID
        /// </summary>
        [JsonProperty("file_id")]
        public string FileId { get; set; }

        /// <summary>
        /// 文件 URL (如果有)
        /// </summary>
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }
    }
}
