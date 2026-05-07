namespace ClientPlatform
{
    /// <summary>
    /// 支付方式项：显示名 + 平台名（客户端下拉与提交用）
    /// </summary>
    public class PaymentMethodItemDto
    {
        public string DisplayName { get; set; }
        public string PlatformName { get; set; }
    }
}
