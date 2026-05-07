using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatformUser.Users.Dto
{
    /// <summary>
    /// 国家地区信息
    /// </summary>
    public class CountryInfoDto
    {
        /// <summary>
        /// 国家名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 中文名称
        /// </summary>
        public string CNName { get; set; }

        /// <summary>
        /// 国家编码（ISO 3166-1 alpha-2）
        /// </summary>
        public string CCA2 { get; set; }

        /// <summary>
        /// 国家编码(ISO 3166-1 alpha-3)
        /// </summary>
        public string CCA3 { get; set; }

        /// <summary>
        /// 国家编码(ISO 3166-1 numeric)   
        /// </summary>
        public string CCN3 { get; set; }

        /// <summary>
        /// 支持法币
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 区号代码
        /// </summary>
        public string AreaPhoneCode { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        public string Region { get; set; }


        /// <summary>
        /// 子区域
        /// </summary>
        public string SubRegion { get; set; }

        /// <summary>
        /// 区域（中文）
        /// </summary>
        public string CnRegion { get; set; }


        /// <summary>
        /// 子区域
        /// </summary>
        public string CnSubRegion { get; set; }


    }

    /// <summary>
    /// 国家地区下拉数据
    /// </summary>
    public class CountryInfoSelectItemDto
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
}
