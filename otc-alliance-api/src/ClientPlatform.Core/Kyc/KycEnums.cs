using System.ComponentModel;

namespace ClientPlatform
{
    /// <summary>
    /// KYC 业务状态
    /// </summary>
    public enum KycBizStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        NOTSTARTED = -1,

        /// <summary>
        /// 待提交
        /// </summary>
        PENDINGSUBMISSION = 0,

        /// <summary>
        /// 审核中
        /// </summary>
        UNDERREVIEW = 1,

        /// <summary>
        /// 已通过
        /// </summary>
        APPROVED = 2,

        /// <summary>
        /// 已拒绝
        /// </summary>
        REJECTED = 3,

        /// <summary>
        /// 需要重新提交
        /// </summary>
        RESUBMISSIONREQUIRED = 4
    }

    /// <summary>
    /// 用户 KYC 等级
    /// </summary>
    public enum VAUserKYCLevel
    {
        /// <summary>
        /// 用户基础认证
        /// </summary>
        [AmbientValue("Test-basic&liveness")]
        UserBasicAuthentication = 0,

    }

    /// <summary>
    /// KYC 渠道产品类型
    /// </summary>
    public enum KycChannelProductTypes
    {
        /// <summary>
        /// Web SDK 集成
        /// </summary>
        WebSDK = 0,
        /// <summary>
        /// 外部链接
        /// </summary>
        Link = 1,
        /// <summary>
        /// 纯 API 对接
        /// </summary>
        API = 2,

        /// <summary>
        /// Pay用户
        /// </summary>
        Pay = 3,
    }

    /// <summary>
    /// KYC 渠道代码常量
    /// </summary>
    public static class KycChannelCodes
    {
        /// <summary>
        /// Sumsub 渠道
        /// </summary>
        public const string Sumsub = "Sumsub";
    }

    /// <summary>
    /// 商业用户类型
    /// </summary>
    public enum BusinessUserType
    {
        /// <summary>
        /// 个人
        /// </summary>
        Individual = 0,
        /// <summary>
        /// 企业
        /// </summary>
        Company = 1
    }
}
