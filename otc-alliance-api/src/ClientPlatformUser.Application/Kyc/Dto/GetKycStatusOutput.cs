using System;
using ClientPlatform;
using ClientPlatform.Kyc;
using ClientPlatform.Pay;

namespace ClientPlatformUser.Kyc.Dto
{
    public class GetKycStatusOutput
    {
        /// <summary>
        /// KYC 业务状态
        /// </summary>
        public KycBizStatus KycStatus { get; set; }

        /// <summary>
        /// 拒绝原因 (展示给用户)
        /// </summary>
        public string RejectReason { get; set; }

        /// <summary>
        /// 拒绝标签
        /// </summary>
        public string RejectLabels { get; set; }

        /// <summary>
        /// 认证等级
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        /// 验证链接 (如果需要继续认证)
        /// </summary>
        public string VerificationLink { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 证件类型（当姓名已存在时由后端返回，前端直接展示，用户只需填写证件号码）
        /// </summary>
        public string IdDocType { get; set; }
    }
}
