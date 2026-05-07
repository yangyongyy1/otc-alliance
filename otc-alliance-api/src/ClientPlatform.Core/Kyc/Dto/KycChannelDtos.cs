using System.Collections.Generic;

namespace ClientPlatform.Kyc.Dto
{
    /// <summary>
    /// 创建申请人请求参数
    /// </summary>
    public class CreateApplicantRequest
    {
        /// <summary>
        /// 外部用户 ID (系统内部的用户唯一标识)
        /// </summary>
        public string ExternalUserId { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 申请人类型 (individual 或 company)
        /// </summary>
        public string ApplicantType { get; set; }

        /// <summary>
        /// 认证等级名称
        /// </summary>
        public string LevelName { get; set; }
        
        // 可根据需要添加更多字段 (FixedInfo 等)
    }

    /// <summary>
    /// 创建申请人响应结果
    /// </summary>
    public class CreateApplicantResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 申请人 ID (提供商返回)
        /// </summary>
        public string ApplicantId { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 获取 WebSDK 链接请求参数
    /// </summary>
    public class GetWebSdkLinkRequest
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 认证等级名称
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        /// 链接有效期 (秒)
        /// </summary>
        public int TtlInSecs { get; set; }
    }

    /// <summary>
    /// 获取 WebSDK 链接响应结果
    /// </summary>
    public class GetWebSdkLinkResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 认证 URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }
    }
}
