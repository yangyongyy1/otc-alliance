using System;
using Abp.Application.Services.Dto;

namespace ClientPlatform.AllianceManagement.Dot
{
    public class SearchMerchantSubCodeDto : PagedAndSortedResultRequestDto
    {
        public int MerchantId { get; set; }
        public string SubCode { get; set; }
        public string SubDescription { get; set; }

        public DateTime? CreationTimeStart { get; set; }
        public DateTime? CreationTimeEnd { get; set; }

        public DateTime? LastModificationTimeStart { get; set; }
        public DateTime? LastModificationTimeEnd { get; set; }
    }
}