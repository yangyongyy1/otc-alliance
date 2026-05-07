using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform.Pay.Entities;

namespace ClientPlatform.VABusiness.Dot
{
    [AutoMapFrom(typeof(PayChannelAccountPaymentMethod))]
    public class VAAccountDetailDto:FullAuditedEntity<Guid>
    {

        /// <summary>
        /// 关联的账户 ID
        /// </summary>
        [Required]
        public Guid AccountId { get; set; }

        /// <summary>
        /// 关联的账户实体
        /// </summary>
        [ForeignKey("AccountId")]
        public virtual UserPayChannelAccount Account { get; set; }

        /// <summary>
        /// 支付方式类型
        /// EUR: SEPA
        /// USD: ACH, Fedwire, SWIFT
        public string PaymentType { get; set; }

        // ========== 账户详情 ==========

        /// <summary>
        /// 账户持有人姓名
        /// </summary>
        public string AccountHolderName { get; set; }

        /// <summary>
        /// 账户号码 (IBAN for SEPA, Account Number for ACH/Fedwire/SWIFT)
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// 账户路由号码 (ACH/Fedwire)
        /// </summary>
        public string AccountRoutingNumber { get; set; }

        /// <summary>
        /// 备注/参考号/Memo
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// SWIFT BIC 代码
        /// </summary>
        public string SwiftBic { get; set; }

        /// <summary>
        /// 中介机构 SWIFT BIC (SWIFT 专用)
        /// </summary>
        public string IntermediarySwiftBic { get; set; }

        // ========== 机构信息 ==========

        /// <summary>
        /// 机构名称（银行名称）
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// 机构地址第一行
        /// </summary>
        public string InstitutionAddressLine1 { get; set; }

        /// <summary>
        /// 机构城市
        /// </summary>
        public string InstitutionCity { get; set; }

        /// <summary>
        /// 机构州/省
        /// </summary>
        public string InstitutionState { get; set; }

        /// <summary>
        /// 机构邮政编码
        /// </summary>
        public string InstitutionPostalCode { get; set; }

        /// <summary>
        /// 机构国家代码
        /// </summary>
        [StringLength(10)]
        public string InstitutionCountryCode { get; set; }

        // ========== 账户持有人地址 (Fedwire/SWIFT) ==========

        /// <summary>
        /// 账户持有人地址第一行
        /// </summary>
        public string HolderAddressLine1 { get; set; }

        /// <summary>
        /// 账户持有人城市
        /// </summary>
        public string HolderCity { get; set; }

        /// <summary>
        /// 账户持有人州/省
        /// </summary>
        public string HolderState { get; set; }

        /// <summary>
        /// 账户持有人邮政编码
        /// </summary>
        public string HolderPostalCode { get; set; }

        /// <summary>
        /// 账户持有人国家代码
        /// </summary>
        [StringLength(10)]
        public string HolderCountryCode { get; set; }
    }
}
