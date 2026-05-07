using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.AllianceManagement
{
    public class Alliance : FullAuditedEntity<int>
    {
        /// <summary>
        /// 名称
        /// </summary>
        /// 
        [StringLength(50)]
        public string Name { get; set; }

        //产品名称
        [StringLength(50)]
        public string ProductName { get; set; }

        /// <summary>
        /// 联盟标识
        /// </summary>
        [StringLength(20)]
        public string SerialNumber { get; set; }


        /// <summary>
        /// LOGO
        /// </summary>
        [StringLength(250)]
        public string LogoUrl { get; set; }


        /// <summary>
        /// 用户协议URL 
        /// </summary>
        public string UserAgreementLink { get; set; }

        /// <summary>
        /// 站点名称
        /// </summary>
        public string WebSiteUrl { get; set; }

        /// <summary>
        /// 产权信息
        /// </summary>
        public string CopyrightInfo { get; set; }

        /// <summary>
        /// 免责声明
        /// </summary>

        public string Disclaimer { get; set; }


        /// <summary>
        /// 主题颜色
        /// </summary>

        [StringLength(20)]

        public string ThemeColor { get; set; }

        /// <summary>
        /// 服务邮箱
        /// </summary>
        public string ServiceEmail { get; set; }

        /// <summary>
        /// 站点IOCN
        /// </summary>
        public string WebSiteIcon { get; set; }
    }
}
