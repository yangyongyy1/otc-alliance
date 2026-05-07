using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using ClientPlatform;
using ClientPlatform.Orders;

namespace ClientPlatform.Orders.Dot
{
    /// <summary>
    /// 收款订单列表 DTO（后台管理使用）
    /// </summary>
    [AutoMapFrom(typeof(CollectionOrder))]
    public class CollectionOrderDto : FullAuditedEntityDto<Guid>
    {
        public string PlatformOrderNo { get; set; }
        public string ChannelOrderNo { get; set; }

        public int AllianceId { get; set; }
        public string AllianceName { get; set; }

        public int MerchantId { get; set; }
        public string MerchantName { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public AccountUserType UserType { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public string ChannelCode { get; set; }
        public string PaymentMethod { get; set; }

        public CollectionOrderType OrderType { get; set; }
        public CollectionOrderStatus OrderStatus { get; set; }

        public string PayerName { get; set; }
        public string RecipientName { get; set; }

        public Guid AccountId { get; set; }
        public string Reference { get; set; }
        public string Remark { get; set; }
    }
}

