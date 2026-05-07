using Abp.Events.Bus;
using System;

namespace ClientPlatform.Kyc.Events
{
    /// <summary>
    /// KYC 验证完成事件
    /// 当用户 KYC 状态更新为完成（通过或拒绝）时触发
    /// </summary>
    public class KycVerificationCompletedEvent : EventData
    {
        public int UserId { get; set; }
        public Guid KycApplicantId { get; set; }
        public bool IsApproved { get; set; }

        public KycVerificationCompletedEvent(int userId, Guid kycApplicantId, bool isApproved)
        {
            UserId = userId;
            KycApplicantId = kycApplicantId;
            IsApproved = isApproved;
        }
    }
}
