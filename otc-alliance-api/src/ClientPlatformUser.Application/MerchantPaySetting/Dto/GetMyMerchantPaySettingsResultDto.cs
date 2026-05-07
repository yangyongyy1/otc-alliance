using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClientPlatformUser.MerchantPaySetting.Dto
{
    /// <summary>
    /// 获取当前用户商户支付设置返回值：DirectPay /  分别对应直连与 VA 配置列表，无配置时为空数组。
    /// </summary>
    public class GetMyMerchantPaySettingsResultDto
    {
        [JsonProperty("DirectPay")]
        public List<DirectPaySettingItemDto> DirectPay { get; set; } = new List<DirectPaySettingItemDto>();

        [JsonProperty("Pay")]
        public List<PaySettingItemDto> Pay { get; set; } = new List<PaySettingItemDto>();
    }
}
