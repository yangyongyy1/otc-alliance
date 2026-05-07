using System;

namespace ClientPlatform.Kyc.Jobs
{
    /// <summary>
    /// KYC 验证完成回调任务参数
    /// </summary>
    public class CustomerKycVerificationCompletedCallBackJobArgs
    {
        /// <summary>
        /// 申请人 ID
        /// </summary>
        public Guid id { get; set; }
    }
}
