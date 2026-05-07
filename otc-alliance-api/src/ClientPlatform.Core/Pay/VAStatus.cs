using System.ComponentModel;

namespace ClientPlatform
{
    /// <summary>
    /// 钱包状态
    /// </summary>
    public enum VAStatus
    {
        /// <summary>
        /// 未提交（默认状态）
        /// </summary>
        [AmbientValue("NOT_SUBMITTED")]
        NotSubmitted,

        /// <summary>
        /// 待定
        /// </summary>
        [AmbientValue("PENDDING")]
        Pending,

        /// <summary>
        /// 活动
        /// </summary>
        [AmbientValue("ACTIVE")]
        Active,

        /// <summary>
        /// 失败
        /// </summary>
        [AmbientValue("FAILED")]
        Failed,

        /// <summary>
        /// 暂停
        /// </summary>
        [AmbientValue("SUSPENDED")]
        Suspended,

        /// <summary>
        /// 关闭
        /// </summary>
        [AmbientValue("CLOSED")]
        Closed
    }
}
