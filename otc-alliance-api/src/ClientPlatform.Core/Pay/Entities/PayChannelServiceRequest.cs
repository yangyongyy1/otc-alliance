using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClientPlatform.UserManagement;

namespace ClientPlatform.Pay.Entities
{
    /// <summary>
    /// 支付渠道服务请求记录表
    /// <para>用于记录用户向支付渠道发起的所有请求（如进件、开户等），包含请求参数、响应结果及状态。</para>
    /// </summary>
    [Table("PayChannelServiceRequests")]
    public class PayChannelServiceRequest : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// 用户ID
        /// <para>关联 ClientUser 表，标识该请求所属的用户。</para>
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 关联的用户实体
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ClientUser User { get; set; }

        /// <summary>
        /// 币种
        /// <para>标识该请求针对的货币类型（如 USD, EUR）。</para>
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        /// <summary>
        /// 支付渠道提供商标识
        /// <para>如 SunPay, Wise 等，用于区分不同的支付渠道。</para>
        /// </summary>
        [Required]
        [StringLength(32)]
        public string ChannelProvider { get; set; }

        /// <summary>
        /// 请求载荷
        /// <para>JSON格式存储发送给渠道的原始请求数据，用于重试或记录。</para>
        /// </summary>
        public string RequestPayload { get; set; }

        /// <summary>
        /// 请求状态
        /// <para>标识当前请求的处理进度（如 Pending, Success, Failed）。</para>
        /// </summary>
        public PayChannelServiceRequestStatus Status { get; set; }

        /// <summary>
        /// 失败步骤
        /// <para>记录请求在哪个环节（如创建客户、创建账户）失败。</para>
        /// </summary>
        public PayChannelServiceRequestFailStep FailStep { get; set; }

        /// <summary>
        /// 失败原因
        /// <para>记录详细的错误信息或渠道返回的错误描述。</para>
        /// </summary>
        public string FailReason { get; set; }

        /// <summary>
        /// 渠道客户ID
        /// <para>记录在支付渠道侧生成的唯一客户标识。</para>
        /// </summary>
        [StringLength(64)]
        public string CustomerId { get; set; }

        /// <summary>
        /// 渠道账户ID
        /// <para>记录在支付渠道侧生成的唯一账户标识。</para>
        /// </summary>
        [StringLength(64)]
        public string AccountId { get; set; }

        /// <summary>
        /// 客户信息相关接口的原始响应数据
        /// <para>通常为 JSON 格式，用于调试或记录。</para>
        /// </summary>
        public string CustomerResponse { get; set; }

        /// <summary>
        /// 账户信息相关接口的原始响应数据
        /// <para>通常为 JSON 格式，用于调试或记录。</para>
        /// </summary>
        public string AccountResponse { get; set; }

        /// <summary>
        /// 重试次数
        /// <para>记录该请求已被重试的次数。</para>
        /// </summary>
        public int RetryCount { get; set; }
    }
}
