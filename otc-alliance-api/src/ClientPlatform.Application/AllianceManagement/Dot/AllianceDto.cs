using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using Abp.Runtime.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.AllianceManagement.Dot
{
    [AutoMapFrom(typeof(Alliance))]
    public class AllianceDto : FullAuditedEntity<int>
    {

        /// <summary>
        /// 名称
        /// </summary>
        /// 
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 联盟标识
        /// </summary>
        [StringLength(12)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [StringLength(50)]
        public string ProductName { get; set; }


        /// <summary>
        /// LOGO
        /// </summary>
        [StringLength(250)]
        public string LogoUrl { get; set; }


        /// <summary>
        /// 用户协议URL 
        /// </summary>
        public string UserAgreementLink { get; set; }
    }

    public class CreateOrUpdateAllianceDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        /// 
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [StringLength(50)]
        public string ProductName { get; set; }
    }

    public class SettingAllianceDto
    {
        /// <summary>
        /// LOGO
        /// </summary>
        [StringLength(250)]
        public string LogoUrl { get; set; }


        public string UserAgreementLink { get; set; }

        public int Id { get; set; }


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

    public class PagedAllianceResultRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
    {
        public string Name { get; set; }

        public string SerialNumber { get; set; }

        public DateTime? CreationTimeStart { get; set; }

        public DateTime? CreationTimeEnd { get; set; }

        public DateTime? ModificationTimeStart { get; set; }

        public DateTime? ModificationTimeEnd { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Sorting))
            {
                Sorting = "CreationTime DESC";
            }
        }
    }
}
