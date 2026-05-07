namespace ClientPlatform.Pay
{
    /// <summary>
    /// 支付渠道客户状态枚举
    /// </summary>
    public enum PayChannelCustomerStatus
    {
        /// <summary>
        /// 未创建
        /// </summary>
        NotCreated = -1,

        /// <summary>
        /// 待审核/审核中
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 正常/活跃
        /// </summary>
        Active = 1,

        /// <summary>
        /// 冻结
        /// </summary>
        Frozen = 2,

        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 3,

        /// <summary>
        /// 失败/未通过
        /// </summary>
        Failed = 4
    }
}
