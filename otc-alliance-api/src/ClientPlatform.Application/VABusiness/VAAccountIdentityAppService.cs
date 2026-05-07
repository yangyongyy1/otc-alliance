using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ClientPlatform.Authorization;
using ClientPlatform.Pay.Entities;
using ClientPlatform.VABusiness.Dot;

namespace ClientPlatform.VABusiness
{
    /// <summary>
    /// VA账户身份认证管理
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_VABusiness_ApplicationManagement)]
    public class VAAccountIdentityAppService : AppServiceBase
    {
        private readonly IRepository<PayChannelServiceRequest, Guid> _payChannelServiceRequestRepository;
        private ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payChannelServiceRequestRepository"></param>
        /// <param name="logger"></param>
        public VAAccountIdentityAppService(
            IRepository<PayChannelServiceRequest, Guid> payChannelServiceRequestRepository,
            ILogger logger)
        {
            _payChannelServiceRequestRepository = payChannelServiceRequestRepository;
            _logger = logger;
        }
        
        /// <summary>
        /// 获取支付渠道服务请求列表（分页）
        /// </summary>
        /// <param name="input">分页查询参数</param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_VABusiness_ApplicationManagement_BtnQuery)]
        public async Task<PagedResultDto<PayChannelServiceRequestDto>> GetPayChannelServiceRequestListAsync(PagedPayChannelServiceRequestResultRequestDto input)
        {
            _logger.Info("GetPayChannelServiceRequestListAsync start");

            var qry = _payChannelServiceRequestRepository.GetAllIncluding(n=>n.User.Merchant.Alliance)
                       .WhereIf(input.UserId.HasValue, n => n.UserId == input.UserId.Value)
                       .WhereIf(!input.Currency.IsNullOrWhiteSpace(), n => n.Currency.Contains(input.Currency))
                       .WhereIf(!input.ChannelProvider.IsNullOrWhiteSpace(), n => n.ChannelProvider.Contains(input.ChannelProvider))
                       .WhereIf(input.Status.HasValue, n => n.Status == input.Status.Value)
                       .WhereIf(input.FailStep.HasValue, n => n.FailStep == input.FailStep.Value)
                       .WhereIf(!input.CustomerId.IsNullOrWhiteSpace(), n => n.CustomerId.Contains(input.CustomerId))
                       .WhereIf(!input.AccountId.IsNullOrWhiteSpace(), n => n.AccountId.Contains(input.AccountId))
                       .WhereIf(input.CreationTimeStart.HasValue, n => n.CreationTime >= input.CreationTimeStart.Value)
                       .WhereIf(input.CreationTimeEnd.HasValue, n => n.CreationTime <= input.CreationTimeEnd.Value)
                       .WhereIf(input.ModificationTimeStart.HasValue, n => n.LastModificationTime >= input.ModificationTimeStart.Value)
                       .WhereIf(input.ModificationTimeEnd.HasValue, n => n.LastModificationTime <= input.ModificationTimeEnd.Value)
                       .OrderBy(input.Sorting);

            var count = await qry.CountAsync();

            var pagedResult = await qry.PageBy(input).ToListAsync();

            var mapResult = ObjectMapper.Map<PayChannelServiceRequestDto[]>(pagedResult);

            return new PagedResultDto<PayChannelServiceRequestDto>(count, mapResult);
        }

        /// <summary>
        /// 获取支付渠道服务请求详情
        /// </summary>
        /// <param name="id">请求ID</param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_VABusiness_ApplicationManagement_BtnViewDetails)]
        public async Task<PayChannelServiceRequestDto> GetPayChannelServiceRequestByIdAsync(Guid id)
        {
            var entity = await _payChannelServiceRequestRepository.GetAsync(id);
            var mapResult = ObjectMapper.Map<PayChannelServiceRequestDto>(entity);
            return mapResult;
        }

    }
}

