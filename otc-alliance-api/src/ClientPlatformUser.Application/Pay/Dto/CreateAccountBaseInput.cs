using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ClientPlatformUser
{
    public class CreateAccountBaseInput
    {
        /// <summary>
        /// 客户 ID
        /// </summary>
        [Required]
        [JsonProperty("customer_id")]
        public string CustomerId { get; set; }

        /// <summary>
        /// 国家代码 (例如: GB)
        /// </summary>
        [Required]
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
