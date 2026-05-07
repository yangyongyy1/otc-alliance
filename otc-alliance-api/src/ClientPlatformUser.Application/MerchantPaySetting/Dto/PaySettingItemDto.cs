namespace ClientPlatformUser.MerchantPaySetting.Dto
{
    /// <summary>
    /// VA 支付单条配置：币种 + 渠道
    /// </summary>
    public class PaySettingItemDto
    {
        public string Currency { get; set; }
        public string ChannelCode { get; set; }
    }
}
