using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;


namespace ClientPlatform.AllianceManagement
{
    public class MerchantSubCode : FullAuditedEntity<int>
    {
        public int MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; }
        public string SubCode { get; set; }
        public string SubDescription { get; set; }
    }
}