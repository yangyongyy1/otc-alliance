using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClientPlatform.Authorization.Users;
using ClientPlatform.UserManagement;

namespace ClientPlatform.Pay.Entities
{
    /// <summary>
    /// 用户支付渠道客户档案
    /// 用于存储用户在不同支付渠道（如 SunPay）的客户 ID 和状态
    /// </summary>
    [Table("PayChannelCustomers")]
    public class UserPayChannelCustomer : FullAuditedEntity<Guid>
    {
        public const int MaxChannelProviderLength = 32;
        public const int MaxChannelIdLength = 64;
        public const int MaxStatusLength = 32;
        public const int MaxEntityTypeLength = 32;

        /// <summary>
        /// 关联的用户 ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 关联的用户实体
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ClientUser User { get; set; }

        /// <summary>
        /// 渠道提供商名称 (例如: "SunPay")
        /// </summary>
        [Required]
        [StringLength(MaxChannelProviderLength)]
        public string ChannelProvider { get; set; }

        /// <summary>
        /// 渠道客户 ID (在支付渠道方的唯一标识)
        /// </summary>
        [StringLength(MaxChannelIdLength)]
        public string ChannelCustomerId { get; set; }

        /// <summary>
        /// 引用 ID (我们传给渠道的用户标识，用于回调时匹配)
        /// 通常为 UserId 转字符串或其他业务唯一标识
        /// </summary>
        [StringLength(MaxChannelIdLength)]
        public string ReferenceId { get; set; }

        /// <summary>
        /// 状态 (例如: Created, Active, Suspended)
        /// </summary>
        public PayChannelCustomerStatus Status { get; set; }

        /// <summary>
        /// 客户实体类型 (例如: INDIVIDUAL, COMPANY)
        /// </summary>
        public PayChannelCustomerEntityType EntityType { get; set; }

        /// <summary>
        /// 请求数据 (发送给渠道的 CreateCustomer 请求参数 JSON)
        /// </summary>
        public string RequestPayload { get; set; }

        /// <summary>
        /// 响应数据 (渠道返回的 CreateCustomer 响应 JSON)
        /// </summary>
        public string ResponsePayload { get; set; }

        /// <summary>
        /// 回调数据 (渠道回调的完整 JSON payload)
        /// </summary>
        public string CallbackPayload { get; set; }

        /// <summary>
        /// 原始数据 JSON (存储创建客户时返回的完整响应，用于备查)
        /// 注：为保持向后兼容保留，新代码应使用 RequestPayload/ResponsePayload/CallbackPayload
        /// </summary>
        public string RawData { get; set; }
    }
}
