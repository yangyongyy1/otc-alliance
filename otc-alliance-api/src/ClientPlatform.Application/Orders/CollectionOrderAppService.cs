using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ClientPlatform.Authorization;
using ClientPlatform.Orders.Dot;

namespace ClientPlatform.Orders
{
    /// <summary>
    /// 收款订单管理（后台订单管理：入金订单 / VA 订单）
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_OrderManagement)]
    public class CollectionOrderAppService : AppServiceBase
    {
        private readonly IRepository<CollectionOrder, Guid> _orderRepository;
        private readonly IRepository<CollectionOrderDetail, Guid> _orderDetailRepository;

        public CollectionOrderAppService(
            IRepository<CollectionOrder, Guid> orderRepository,
            IRepository<CollectionOrderDetail, Guid> orderDetailRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
        }

        /// <summary>
        /// 入金订单列表（CollectionOrderType = Direct）
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_OrderManagement_DepositOrders_BtnQuery)]
        public async Task<PagedResultDto<CollectionOrderDto>> GetDepositOrderListAsync(PagedCollectionOrderResultRequestDto input)
        {
            return await GetOrderListInternalAsync(input, CollectionOrderType.Direct);
        }

        /// <summary>
        /// VA 订单列表（CollectionOrderType = VA）
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_OrderManagement_VAOrders_BtnQuery)]
        public async Task<PagedResultDto<CollectionOrderDto>> GetVAOrderListAsync(PagedCollectionOrderResultRequestDto input)
        {
            return await GetOrderListInternalAsync(input, CollectionOrderType.VA);
        }

        /// <summary>
        /// 订单详情（主表 + 明细）
        /// </summary>
        [AbpAuthorize(
            PermissionNames.Pages_OrderManagement_DepositOrders_BtnViewDetails,
            PermissionNames.Pages_OrderManagement_VAOrders_BtnViewDetails)]
        public async Task<object> GetOrderDetailAsync(Guid id)
        {
            var order = await _orderRepository.FirstOrDefaultAsync(id);
            if (order == null)
                throw new Abp.UI.UserFriendlyException("订单不存在");

            var detail = await _orderDetailRepository.FirstOrDefaultAsync(d => d.CollectionOrderId == order.Id);

            var orderDto = ObjectMapper.Map<CollectionOrderDto>(order);
            CollectionOrderDetailDto detailDto = null;
            if (detail != null)
            {
                detailDto = ObjectMapper.Map<CollectionOrderDetailDto>(detail);
            }

            return new
            {
                Order = orderDto,
                Detail = detailDto
            };
        }

        private async Task<PagedResultDto<CollectionOrderDto>> GetOrderListInternalAsync(
            PagedCollectionOrderResultRequestDto input,
            CollectionOrderType orderType)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = "CreationTime desc";
            }

            var query = _orderRepository.GetAll()
                .Where(x => x.OrderType == orderType)
                .WhereIf(!input.PlatformOrderNo.IsNullOrWhiteSpace(), x => x.PlatformOrderNo.Contains(input.PlatformOrderNo))
                .WhereIf(!input.ChannelOrderNo.IsNullOrWhiteSpace(), x => x.ChannelOrderNo.Contains(input.ChannelOrderNo))
                .WhereIf(input.AllianceId.HasValue, x => x.AllianceId == input.AllianceId.Value)
                .WhereIf(input.MerchantId.HasValue, x => x.MerchantId == input.MerchantId.Value)
                .WhereIf(!input.UserInfo.IsNullOrWhiteSpace(), x => x.UserEmail.Contains(input.UserInfo) || x.UserName.Contains(input.UserInfo))
                .WhereIf(!input.Currency.IsNullOrWhiteSpace(), x => x.Currency == input.Currency)
                .WhereIf(!input.ChannelCode.IsNullOrWhiteSpace(), x => x.ChannelCode == input.ChannelCode)
                .WhereIf(!input.PaymentMethod.IsNullOrWhiteSpace(), x => x.PaymentMethod == input.PaymentMethod)
                .WhereIf(input.OrderStatus.HasValue, x => x.OrderStatus == input.OrderStatus.Value)
                .WhereIf(input.OrderType.HasValue, x => x.OrderType == input.OrderType.Value)
                .WhereIf(!input.PayerName.IsNullOrWhiteSpace(), x => x.PayerName.Contains(input.PayerName))
                .WhereIf(!input.RecipientName.IsNullOrWhiteSpace(), x => x.RecipientName.Contains(input.RecipientName))
                .WhereIf(!input.AccountId.IsNullOrWhiteSpace(), x => x.AccountId.ToString().Contains(input.AccountId))
                .WhereIf(!input.Reference.IsNullOrWhiteSpace(), x => x.Reference.Contains(input.Reference))
                .WhereIf(!input.Remark.IsNullOrWhiteSpace(), x => x.Remark.Contains(input.Remark))
                .WhereIf(input.CreationTimeStart.HasValue, x => x.CreationTime >= input.CreationTimeStart.Value)
                .WhereIf(input.CreationTimeEnd.HasValue, x => x.CreationTime <= input.CreationTimeEnd.Value)
                .WhereIf(input.ModificationTimeStart.HasValue, x => x.LastModificationTime >= input.ModificationTimeStart.Value)
                .WhereIf(input.ModificationTimeEnd.HasValue, x => x.LastModificationTime <= input.ModificationTimeEnd.Value);

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(input.Sorting).PageBy(input).ToListAsync();

            var dtos = ObjectMapper.Map<CollectionOrderDto[]>(items);
            return new PagedResultDto<CollectionOrderDto>(totalCount, dtos);
        }
    }
}

