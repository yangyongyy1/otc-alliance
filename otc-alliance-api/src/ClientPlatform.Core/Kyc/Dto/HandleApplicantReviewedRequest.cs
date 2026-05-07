using System;

namespace ClientPlatform.Kyc.Dto
{
    /// <summary>
    /// 处理申请人审核回调请求
    /// </summary>
    public class HandleApplicantReviewedRequest
    {
        /// <summary>
        /// 相关性 ID
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// 等级名称
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        /// 外部用户 ID
        /// </summary>
        public string ExternalUserId { get; set; }

        /// <summary>
        /// 回调类型 (applicantCreated, applicantPending, applicantReviewed, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public string ReviewStatus { get; set; }

        /// <summary>
        /// 申请人 ID
        /// </summary>
        public string ApplicantId { get; set; }

        /// <summary>
        /// 检查 ID
        /// </summary>
        public string InspectionId { get; set; }

        /// <summary>
        /// 回调创建时间（字符串）
        /// </summary>
        public string CreatedAt { get; set; }

        /// <summary>
        /// 回调创建时间（毫秒字符串）
        /// </summary>
        public string CreatedAtMs { get; set; }
    }
}
