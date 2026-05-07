

using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClientPlatform.VABusiness
{
    /// <summary>
    /// 账户身份认证信息（KYC）
    /// </summary>
    public class VAAccountIdentity : FullAuditedAggregateRoot<int>
    {
        /// <summary>
        /// 账户 ID（关联 Account 表）
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }


        /// <summary>
        /// 商户ID
        /// </summary>
        public int MerchantId { get; set; }


        /// <summary>
        /// 币种
        /// </summary>
        public string Currency {  get; set; }


        public IdentityStatus? Status { get; set; }


        /// <summary>
        /// 账户类型
        /// </summary>
        public AccountUserType AccountUserType {  get; set; }

        /// <summary>
        /// 进件到渠道的表单信息
        /// </summary>
        public string FormJson { get; set; }
    }
}

