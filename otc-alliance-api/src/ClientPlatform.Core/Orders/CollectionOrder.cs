using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;

namespace ClientPlatform.Orders
{
    /// <summary>
    /// 本地支付收款订单实体（数据库实体）
    /// </summary>
    public class CollectionOrder : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// 平台订单号
        /// </summary>
        [StringLength(50)]
        public string PlatformOrderNo { get; set; }

        /// <summary>
        /// 渠道订单号
        /// </summary>
        [StringLength(50)]
        public string ChannelOrderNo { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        /// <summary>
        /// 联盟名称
        /// </summary>
        [StringLength(50)]
        public string AllianceName { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 商户名称
        /// </summary>
        [StringLength(50)]
        public string MerchantName { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        [StringLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// 用户类型（个人/企业）
        /// </summary>
        public AccountUserType UserType { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [StringLength(10)]
        public string Currency { get; set; }

        /// <summary>
        /// 渠道Code
        /// </summary>
        [StringLength(50)]
        public string ChannelCode { get; set; }

        //支付方式
        /// <summary>
        /// 支付方式
        /// </summary>
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public CollectionOrderType OrderType { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public CollectionOrderStatus OrderStatus { get; set; }

        /// <summary>
        /// 付款人姓名
        /// </summary>
        [StringLength(50)]
        public string PayerName { get; set; }

        /// <summary>
        /// 收款人姓名
        /// </summary>
        [StringLength(50)]
        public string RecipientName { get; set; }

        /// <summary>   
        /// 关联的账户ID
        /// </summary>
        public Guid AccountId { get; set; }
      
        /// <summary>
        /// 交易参考
        /// </summary>
        [StringLength(256)]
        public string Reference { get; set; }

       
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(256)]
        public string Remark { get; set; }
    }
}
