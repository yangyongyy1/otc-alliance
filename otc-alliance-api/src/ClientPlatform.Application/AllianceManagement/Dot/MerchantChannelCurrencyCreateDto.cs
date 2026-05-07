using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.AllianceManagement.Dot
{
    public class MerchantChannelCurrencyCreateDto
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
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        public OpenClose OpenClose { get; set; }
    }

    [AutoMapFrom(typeof(MerchantChannelCurrency))]
    public class MerchantChannelCurrencyDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 商家ID
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 渠道编码
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        public OpenClose OpenClose { get; set; }
    }
}
