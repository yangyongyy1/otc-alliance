using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using ClientPlatform.Orders;

namespace ClientPlatform.Orders.Dot
{
    /// <summary>
    /// 收款订单明细 DTO
    /// </summary>
    [AutoMapFrom(typeof(CollectionOrderDetail))]
    public class CollectionOrderDetailDto : FullAuditedEntityDto<Guid>
    {
        public Guid CollectionOrderId { get; set; }

        public string PayerName { get; set; }
        public string PayerCurrency { get; set; }
        public string PayerIban { get; set; }
        public string PayerSwiftBic { get; set; }

        public string RecipientName { get; set; }
        public string RecipientCurrency { get; set; }
        public string RecipientAccountHolderName { get; set; }
        public string RecipientAccountNumber { get; set; }
        public string RecipientIban { get; set; }
        public string RecipientSwiftBic { get; set; }
        public string RecipientBankName { get; set; }
        public string RecipientBankAddress { get; set; }
        public string RecipientBankCountry { get; set; }
        public string SortCode { get; set; }

        public string RequestInfo { get; set; }
        public string ResponseInfo { get; set; }
        public string Callback { get; set; }
    }
}

