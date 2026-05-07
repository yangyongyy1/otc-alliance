using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.BasicDataManagement
{

    /// <summary>
    /// 国家地区信息
    /// </summary>
    public class CountryInfo : FullAuditedEntity<int>
    {
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string CNName { get; set; }

        [StringLength(10)]

        public string CCA2 { get; set; }

        [StringLength(10)]

        public string CCA3 { get; set; }

        [StringLength(10)]

        public string CCN3 { get; set; }


        [StringLength(100)]
        public string Currency { get; set; }

        [StringLength(150)]
        public string AreaPhoneCode { get; set; }

        [StringLength(50)]

        public string Region { get; set; }

        [StringLength(50)]

        public string SubRegion { get; set; }

        [StringLength(50)]

        public string CnRegion { get; set; }

        [StringLength(50)]

        public string CnSubRegion { get; set; }


    }
}
