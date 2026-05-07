using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.AllianceManagement
{
    /// <summary>
    /// 商家渠道币种配置
    /// </summary>
    public class MerchantChannelCurrency : FullAuditedEntity<int>
    {
        /// <summary>
        /// 商家ID
        /// </summary>
        public int MerchantId { get; set; }



        /// <summary>
        /// 渠道编码
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 商户 ID
        /// </summary>
        public string Currency { get; set; }

        public OpenClose OpenClose { get; set; }
    }
}
