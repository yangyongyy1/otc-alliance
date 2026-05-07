using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPlatform.Pay.Entities
{
    /// <summary>
    /// 支付渠道交易记录
    /// 存储来自 SunPay 等渠道的交易回调数据 (如 Collection, CollectionSettlement)
    /// </summary>
    [Table("PayChannelTransactions")]
    public class PayChannelTransaction : FullAuditedEntity<Guid>
    {
        // --- 关联信息 ---

        /// <summary>
        /// 关联的支付渠道账户 ID (本地系统 FK)
        /// </summary>
        [Required]
        public Guid ChannelAccountId { get; set; }

        [ForeignKey("ChannelAccountId")]
        public virtual UserPayChannelAccount ChannelAccount { get; set; }

        /// <summary>
        /// 渠道名称 (e.g. "SunPay")
        /// </summary>
        [Required]
        [StringLength(32)]
        public string ChannelProvider { get; set; }

        // --- 核心交易信息 ---

        /// <summary>
        /// 交易订单号 (SunPay order_no) - 唯一索引
        /// </summary>
        [Required]
        [StringLength(64)]
        public string OrderNo { get; set; }

        /// <summary>
        /// 外部订单号 (SunPay out_order_no)
        /// </summary>
        [StringLength(64)]
        public string OutOrderNo { get; set; }

        /// <summary>
        /// 外部用户ID (SunPay out_user_id)
        /// </summary>
        [StringLength(64)]
        public string OutUserId { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 交易币种
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        /// <summary>
        /// 附言/参考号 (SunPay reference)
        /// </summary>
        [StringLength(256)]
        public string Reference { get; set; }

        /// <summary>
        /// 支付类型 (SunPay payment_type, e.g. "SEPA")
        /// </summary>
        [StringLength(32)]
        public string PaymentType { get; set; }

        /// <summary>
        /// 交易状态 (SunPay status, e.g. "SUCCESS")
        /// </summary>
        [StringLength(32)]
        public string Status { get; set; }

        /// <summary>
        /// 业务类型 (Top Level biz_type, e.g. "Collection")
        /// </summary>
        [StringLength(32)]
        public string BizType { get; set; }

        /// <summary>
        /// 业务状态 (Top Level biz_status, e.g. "SUCCESS")
        /// </summary>
        [StringLength(32)]
        public string BizStatus { get; set; }

        // --- 结算信息 (Settlement Fields) [NEW] ---

        [StringLength(32)]
        public string SettlementStatus { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? SettlementAmount { get; set; }

        [StringLength(10)]
        public string SettlementCurrency { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? SettlementFee { get; set; }

        [StringLength(10)]
        public string SettlementFeeCurrency { get; set; }

        public DateTime? SettlementTime { get; set; }

        // --- Sender (发送方) 信息 ---

        [StringLength(256)]
        public string SenderName { get; set; }

        [StringLength(64)]
        public string SenderAccountNumber { get; set; }

        [StringLength(64)]
        public string SenderIban { get; set; }

        [StringLength(32)]
        public string SenderSwiftBic { get; set; }


        // --- Recipient (接收方) 信息 ---

        /// <summary>
        /// 接收方 Account ID (SunPay 侧的 ID)
        /// </summary>
        [StringLength(64)]
        public string RecipientAccountId { get; set; }

        [StringLength(10)]
        public string RecipientCurrency { get; set; }

        [StringLength(64)]
        public string RecipientIban { get; set; }

        [StringLength(32)]
        public string RecipientSwiftBic { get; set; }

        [StringLength(10)]
        public string RecipientBankCountry { get; set; }

        [StringLength(256)]
        public string RecipientBankAddress { get; set; }

        [StringLength(64)]
        public string RecipientAccountNumber { get; set; }

        [StringLength(256)]
        public string RecipientBankName { get; set; }

        [StringLength(256)]
        public string RecipientBankAccountHolderName { get; set; }


        // --- 时间信息 ---

        /// <summary>
        /// 交易创建时间 (SunPay creation_time戳转时间)
        /// </summary>
        public DateTime? TransactionCreationTime { get; set; }

        /// <summary>
        /// 交易完成时间 (SunPay completion_time戳转时间)
        /// </summary>
        public DateTime? TransactionCompletionTime { get; set; }
    }
}
