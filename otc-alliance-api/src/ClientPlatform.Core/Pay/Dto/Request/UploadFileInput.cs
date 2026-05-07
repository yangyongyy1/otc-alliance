using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ClientPlatform.Pay.Dto.Request
{
    /// <summary>
    /// 通用文件上传输入参数
    /// </summary>
    public class UploadFileInput : BasePayRequest
    {
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

        /// <summary>
        /// 业务类型 (可选，如果 SunPay 需要)
        /// </summary>
        [JsonIgnore]
        public string BizType { get; set; }
    }
}
