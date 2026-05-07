using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using ClientPlatform.Authorization.Users;
using ClientPlatform.UserManagement;

namespace ClientPlatform.Pay.Entities
{
    /// <summary>
    /// 用户支付渠道账户 (VA 账户)
    /// 用于存储用户在支付渠道创建的虚拟账户信息
    /// </summary>
    [Table("PayChannelAccounts")]
    public class UserPayChannelAccount : FullAuditedEntity<Guid>
    {
        public const int MaxChannelProviderLength = 32;
        public const int MaxChannelIdLength = 64;
        public const int MaxCurrencyLength = 10;
        public const int MaxStatusLength = 32;
        public const int MaxCommonLength = 128;

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
        /// 关联的渠道客户档案 ID
        /// </summary>
        [Required]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// 关联的渠道客户档案实体
        /// </summary>
        [ForeignKey("CustomerId")]
        public virtual UserPayChannelCustomer Customer { get; set; }

        /// <summary>
        /// 渠道提供商名称 (例如: "SunPay")
        /// </summary>
        [Required]
        [StringLength(MaxChannelProviderLength)]
        public string ChannelProvider { get; set; }

        /// <summary>
        /// 渠道账户 ID (在支付渠道方的 Account ID)
        /// </summary>
        [StringLength(MaxChannelIdLength)]
        public string ChannelAccountId { get; set; }

        /// <summary>
        /// 引用 ID (我们传给渠道的用户标识，用于回调时匹配)
        /// 通常为 UserId 转字符串
        /// </summary>
        [StringLength(MaxChannelIdLength)]
        public string ReferenceId { get; set; }

        /// <summary>
        /// 账户币种 (例如: USD, EUR)
        /// </summary>
        [Required]
        [StringLength(MaxCurrencyLength)]
        public string Currency { get; set; }

        /// <summary>
        /// 账户名 (开户人/公司名称)
        /// </summary>
        [StringLength(MaxCommonLength)]
        public string AccountName { get; set; }

        /// <summary>
        /// 银行账号 / IBAN
        /// </summary>
        [StringLength(MaxCommonLength)]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        [StringLength(MaxCommonLength)]
        public string BankName { get; set; }

        /// <summary>
        /// 账户状态 (例如: Pending, Active, Rejected, Closed)
        /// </summary>
        public VAStatus Status { get; set; }

        /// <summary>
        /// 拒绝原因 (如果状态为 Rejected)
        /// </summary>
        public string RejectionReason { get; set; }

        /// <summary>
        /// 原始数据 JSON (存储创建账户时返回的完整信息)
        /// </summary>
        public string RawData { get; set; }
    }
}
