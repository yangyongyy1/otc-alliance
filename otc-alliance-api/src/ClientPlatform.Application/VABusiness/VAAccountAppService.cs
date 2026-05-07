using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Authorization;
using ClientPlatform.Pay.Entities;
using ClientPlatform.UserManagement;
using ClientPlatform.VABusiness.Dot;

namespace ClientPlatform.VABusiness
{
    /// <summary>
    /// VA账户管理
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_VABusiness_AccountManagement)]
    [DisableAuditing]
    public class VAAccountAppService : AppServiceBase
    {
        private readonly IRepository<UserPayChannelAccount, Guid> _vaAccountRepository;
        private readonly IRepository<Alliance, int> _allianceRepository;
        private readonly IRepository<Merchant, int> _merchantRepository;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<DynamicForm, int> _dynamicFormRepository;
        private ILogger _logger;
        private readonly IRepository<PayChannelAccountPaymentMethod, Guid> _payChannelAccountPaymentMethodRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaAccountRepository"></param>
        /// <param name="allianceRepository"></param>
        /// <param name="merchantRepository"></param>
        /// <param name="clientUserRepository"></param>
        /// <param name="dynamicFormRepository"></param>
        /// <param name="logger"></param>
        public VAAccountAppService(
            IRepository<UserPayChannelAccount, Guid> vaAccountRepository,
            IRepository<Alliance, int> allianceRepository,
            IRepository<Merchant, int> merchantRepository,
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<DynamicForm, int> dynamicFormRepository,
            ILogger logger,
            IRepository<PayChannelAccountPaymentMethod, Guid> payChannelAccountPaymentMethodRepository)
        {
            _vaAccountRepository = vaAccountRepository;
            _allianceRepository = allianceRepository;
            _merchantRepository = merchantRepository;
            _clientUserRepository = clientUserRepository;
            _logger = logger;
            _dynamicFormRepository = dynamicFormRepository;
            _payChannelAccountPaymentMethodRepository = payChannelAccountPaymentMethodRepository;
        }

        /// <summary>
        /// 获取VA账户列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_VABusiness_AccountManagement_BtnQuery)]
        public async Task<PagedResultDto<UserPayChannelAccountDto>> GetAllVAAccountListAsync(PagedVAAccountResultRequestDto input)
        {
            var qry = _vaAccountRepository.GetAllIncluding(n => n.User.Merchant.Alliance)
                       .WhereIf(input.AllianceId.HasValue, n => n.User.AllianceId == input.AllianceId.Value)
                       .WhereIf(input.UserId.HasValue, n => n.UserId == input.UserId.Value)
                       .WhereIf(!input.AccountId.IsNullOrWhiteSpace(), n => n.ChannelAccountId.Contains(input.AccountId))
                       .WhereIf(input.MerchantId.HasValue, n => n.User.MerchantId == input.MerchantId.Value)
                       .WhereIf(!input.Currency.IsNullOrWhiteSpace(), n => n.Currency.Contains(input.Currency))
                       .WhereIf(input.AccountUserType.HasValue, n => n.User.UserType == input.AccountUserType.Value)
                       .WhereIf(!input.AccountName.IsNullOrWhiteSpace(), n => n.AccountName.Contains(input.AccountName))
                       .WhereIf(!input.Email.IsNullOrWhiteSpace(), n => n.User.Email.Contains(input.Email))
                       .WhereIf(input.Status.HasValue, n => n.Status == input.Status.Value)
                       .WhereIf(input.CreationTimeStart.HasValue, n => n.CreationTime >= input.CreationTimeStart.Value)
                       .WhereIf(input.CreationTimeEnd.HasValue, n => n.CreationTime <= input.CreationTimeEnd.Value)
                       .WhereIf(input.ModificationTimeStart.HasValue, n => n.LastModificationTime >= input.ModificationTimeStart.Value)
                       .WhereIf(input.ModificationTimeEnd.HasValue, n => n.LastModificationTime <= input.ModificationTimeEnd.Value)
                       .Where(n=>n.Status== VAStatus.Active || n.Status== VAStatus.Closed)
                       .OrderBy(input.Sorting);

            var count = await qry.CountAsync();

            var pagedResult = await qry.PageBy(input).ToListAsync();

            var mapResult = ObjectMapper.Map<UserPayChannelAccountDto[]>(pagedResult);

            return new PagedResultDto<UserPayChannelAccountDto>(count, mapResult);
        }


        /// <summary>
        /// 获取VA账户详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_VABusiness_AccountManagement_BtnViewDetails)]
        public async Task<VAAccountDetailDto> GetVAAccountByIdAsync(Guid id)
        {
            var vaAccount = await _payChannelAccountPaymentMethodRepository.GetAllIncluding(n=>n.Account).FirstOrDefaultAsync(n=>n.AccountId==id);
            var mapResult = ObjectMapper.Map<VAAccountDetailDto>(vaAccount);
            return mapResult;
        }


        /// <summary>
        /// 获取账户币种
        /// </summary>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_VABusiness_AccountManagement_BtnQuery)]
        public async Task<List<string>> GetAccountCurrenciesAsync()
        {
            var currencies = await _vaAccountRepository.GetAll()
                .Select(n => n.Currency)
                .Distinct()
                .ToListAsync();
            return currencies;
        }
    }
}

