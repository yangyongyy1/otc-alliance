using Abp.Application.Services.Dto;
using System;
using ClientPlatform.Orders;

namespace ClientPlatform.Orders.Dot
{
    /// <summary>
    /// 收款订单分页查询入参
    /// </summary>
    public class PagedCollectionOrderResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string PlatformOrderNo { get; set; }
        public string ChannelOrderNo { get; set; }

        public int? AllianceId { get; set; }
        public int? MerchantId { get; set; }

        public string UserInfo { get; set; }
        public string Currency { get; set; }
        public string ChannelCode { get; set; }
        public string PaymentMethod { get; set; }

        public CollectionOrderStatus? OrderStatus { get; set; }
        public CollectionOrderType? OrderType { get; set; }

        public string PayerName { get; set; }
        public string RecipientName { get; set; }
        public string AccountId { get; set; }
        public string Reference { get; set; }
        public string Remark { get; set; }

        public DateTime? CreationTimeStart { get; set; }
        public DateTime? CreationTimeEnd { get; set; }
        public DateTime? ModificationTimeStart { get; set; }
        public DateTime? ModificationTimeEnd { get; set; }
    }
}

