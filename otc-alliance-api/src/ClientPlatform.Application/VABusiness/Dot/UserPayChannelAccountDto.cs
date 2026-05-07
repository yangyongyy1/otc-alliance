using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Entities;
using ClientPlatform.UserManagement;

namespace ClientPlatform.VABusiness.Dot
{
    [AutoMapFrom(typeof(UserPayChannelAccount))]
    public class UserPayChannelAccountDto:FullAuditedEntity<Guid>
    {
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
        public string ChannelProvider { get; set; }

        /// <summary>
        /// 渠道账户 ID (在支付渠道方的 Account ID)
        /// </summary>
        public string ChannelAccountId { get; set; }

        /// <summary>
        /// 引用 ID (我们传给渠道的用户标识，用于回调时匹配)
        /// 通常为 UserId 转字符串
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// 账户币种 (例如: USD, EUR)
        /// </summary>
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// 账户名 (开户人/公司名称)
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 银行账号 / IBAN
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 账户状态 (例如: Pending, Active, Rejected, Closed)
        /// </summary>
        public VAStatus Status { get; set; }

        /// <summary>
        /// 拒绝原因 (如果状态为 Rejected)
        /// </summary>
        public string RejectionReason { get; set; }
    }
}
