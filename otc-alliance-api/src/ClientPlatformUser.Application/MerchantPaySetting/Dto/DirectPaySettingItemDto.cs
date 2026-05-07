namespace ClientPlatformUser.MerchantPaySetting.Dto
{
    /// <summary>
    /// Direct 支付单条配置：币种 + 支付方式（包含显示名与平台名）
    /// </summary>
    public class DirectPaySettingItemDto
    {
        public string Currency { get; set; }
        /// <summary>
        /// 支付方式平台名（用于提交），等同于 DataDictionary 中的 platformName
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// 支付方式显示名（给前端展示用）
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 平台名（与 PaymentMethod 相同，便于客户端按统一字段名使用）
        /// </summary>
        public string PlatformName { get; set; }
    }
}
