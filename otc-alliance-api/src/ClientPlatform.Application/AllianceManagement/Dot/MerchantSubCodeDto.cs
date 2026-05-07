using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;

namespace ClientPlatform.AllianceManagement.Dot
{
    [AutoMapFrom(typeof(MerchantSubCode))]
    public class MerchantSubCodeDto : FullAuditedEntity<int>
    {
        public int MerchantId { get; set; }

        public virtual MerchantDto Merchant { get; set; }
        public string SubCode { get; set; }
        public string SubDescription { get; set; }
    }
}