using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClientPlatform.Authorization.Users;

namespace ClientPlatform.Pay.Entities
{
    /// <summary>
    /// 支付渠道请求/响应日志
    /// 用于完整记录创建客户、创建账户等操作的 API 交互细节
    /// </summary>
    [Table("PayChannelRequestLogs")]
    public class PayChannelRequestLog : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxChannelProviderLength = 32;
        public const int MaxRequestTypeLength = 64;
        public const int MaxStatusLength = 32;

        public int? TenantId { get; set; }

        /// <summary>
        /// 关联的用户 ID
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// 关联的用户实体
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// 渠道提供商 (例如: SunPay)
        /// </summary>
        [Required]
        [StringLength(MaxChannelProviderLength)]
        public string ChannelProvider { get; set; }

        /// <summary>
        /// 请求类型 (例如: CreateCustomer, CreateAccount)
        /// </summary>
        [Required]
        [StringLength(MaxRequestTypeLength)]
        public string RequestType { get; set; }

        /// <summary>
        /// 请求 Payload (JSON)
        /// </summary>
        public string RequestPayload { get; set; }

        /// <summary>
        /// 响应 Payload (JSON)
        /// </summary>
        public string ResponsePayload { get; set; }

        /// <summary>
        /// 状态 (Success/Failed)
        /// </summary>
        [StringLength(MaxStatusLength)]
        public string Status { get; set; }

        /// <summary>
        /// 错误信息 (如有)
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
