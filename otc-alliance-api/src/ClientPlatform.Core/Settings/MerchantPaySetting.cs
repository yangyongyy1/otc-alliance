using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.Settings
{
    public class MerchantPaySetting : FullAuditedEntity<Guid>
    {

        /// <summary>
        /// 商户ID
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 商户名称
        /// </summary>
        public string MerchantName { get; set; }

        /// <summary>
        public MerchantPaySettingType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [StringLength(10)]
        public string Currency { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        [StringLength(50)]
        public string ChannelCode { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public OpenClose Status { get; set; }
        
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }
    }
}
