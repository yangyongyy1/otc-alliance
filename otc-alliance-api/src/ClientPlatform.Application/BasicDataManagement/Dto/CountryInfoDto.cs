using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.BasicDataManagement.Dto
{
    /// <summary>
    /// 
    /// </summary>
    public class SearchAndPagedDto : PagedAndSortedResultRequestDto
    {
        public string CCA2 { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }

        public string Region { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>

    [AutoMapFrom(typeof(CountryInfo))]
    [AutoMapTo(typeof(CountryInfo))]
    public class CountryInfoDto : FullAuditedEntityDto<Guid>
    {

        public new int? Id { get; set; }

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

        [StringLength(30)]
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
