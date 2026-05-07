using System;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    /// <summary>
    /// 上传客户文档响应
    /// </summary>
    public class UploadCustomerDocResponse
    {
        /// <summary>
        /// 文档 ID
        /// </summary>
        [JsonProperty("document_id")]
        public Guid DocumentId { get; set; }
    }
}
