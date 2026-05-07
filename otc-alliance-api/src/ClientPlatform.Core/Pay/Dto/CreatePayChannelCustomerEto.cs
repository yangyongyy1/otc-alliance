using System;
using Abp.Events.Bus;

namespace ClientPlatform.Pay.Dto
{
    /// <summary>
    /// 创建支付渠道客户 MQ 消息传输对象
    /// </summary>
    public class CreatePayChannelCustomerEto : EventData
    {
        /// <summary>
        /// 用户 ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// KYC 申请人 ID
        /// </summary>
        public Guid KycApplicantId { get; set; }

        /// <summary>
        /// 关联的请求 ID (如果是由 CreateAccount 触发)
        /// </summary>
        public Guid? RequestId { get; set; }
    }
}
