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
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Authorization;
using ClientPlatform.UserManagement.Dot;
using ClientPlatform.Common;
using ClientPlatform.Kyc;

namespace ClientPlatform.UserManagement
{
    /// <summary>
    /// 客户端用户管理
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_UserManagement_UserList)]
    public class ClientUserAppService : AppServiceBase
    {
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<Alliance, int> _allianceRepository;
        private readonly IRepository<Merchant, int> _merchantRepository;
        private readonly IRepository<KycApplicant, Guid> _kycApplicantRepository;
        private ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        public ClientUserAppService(
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<Alliance, int> allianceRepository,
            IRepository<Merchant, int> merchantRepository,
            IRepository<KycApplicant, Guid> kycApplicantRepository,
            ILogger logger)
        {
            _clientUserRepository = clientUserRepository;
            _allianceRepository = allianceRepository;
            _merchantRepository = merchantRepository;
            _kycApplicantRepository = kycApplicantRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取客户端用户列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_UserManagement_UserList_BtnQuery)]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<PagedResultDto<ClientUserDto>> GetAllClientUserListAsync(PagedClientUserResultRequestDto input)
        {
            if (input.Sorting.StartsWith("name"))
            {
                input.Sorting = input.Sorting.Replace("name", "firstName");
            }
            var qry = _clientUserRepository.GetAllIncluding(n => n.Alliance, n => n.Merchant)
                       .WhereIf(!input.UserName.IsNullOrWhiteSpace(), n => n.UserName.Contains(input.UserName))
                       .WhereIf(!input.Email.IsNullOrWhiteSpace(), n => n.Email.Contains(input.Email))
                       .WhereIf(!input.InviteCode.IsNullOrWhiteSpace(), n => n.InviteCode.Contains(input.InviteCode))
                       .WhereIf(input.AllianceId.HasValue, n => n.AllianceId == input.AllianceId.Value)
                       .WhereIf(input.MerchantId.HasValue, n => n.MerchantId == input.MerchantId.Value)
                       .WhereIf(input.UserAuthStatus.HasValue, n => n.UserAuthStatus == input.UserAuthStatus.Value)
                       .WhereIf(input.UserStatus.HasValue, n => n.UserStatus == input.UserStatus.Value)
                       .WhereIf(input.UserType.HasValue, n => n.UserType == input.UserType.Value)
                       .WhereIf(input.AuthStandardLevel.HasValue, n => _kycApplicantRepository.GetAll().Any(k => k.UserId == n.Id && k.AuthStandardLevel == input.AuthStandardLevel.Value))
                       .WhereIf(input.CreationTimeStart.HasValue, n => n.CreationTime >= input.CreationTimeStart.Value)
                       .WhereIf(input.CreationTimeEnd.HasValue, n => n.CreationTime <= input.CreationTimeEnd.Value)
                       .WhereIf(input.ModificationTimeStart.HasValue, n => n.LastModificationTime >= input.ModificationTimeStart.Value)
                       .WhereIf(input.ModificationTimeEnd.HasValue, n => n.LastModificationTime <= input.ModificationTimeEnd.Value)
                       .WhereIf(!input.KycLevelName.IsNullOrEmpty(),n=>n.KycLevelCompleted==input.KycLevelName)
                       .ApplyCustomFilters(input.CustomFilters)
                       .OrderBy(input.Sorting);

            var count = await qry.CountAsync();

            var pagedResult = await qry.PageBy(input).ToListAsync();

            //var userIds = pagedResult.Select(u => u.Id).ToList();
            //var kycLevelDict = userIds.Count > 0
            //    ? await _kycApplicantRepository.GetAll()
            //        .Where(k => userIds.Contains(k.UserId))
            //        .GroupBy(k => k.UserId)
            //        .Select(g => new { UserId = g.Key, AuthStandardLevel = g.OrderByDescending(x => x.CreationTime).First().AuthStandardLevel })
            //        .ToDictionaryAsync(x => x.UserId, x => (AuthStandardLevel?)x.AuthStandardLevel)
            //    : new Dictionary<int, AuthStandardLevel?>();

            var mapResult = ObjectMapper.Map<ClientUserDto[]>(pagedResult);
            //foreach (var dto in mapResult)
            //{
            //    if (kycLevelDict.TryGetValue(dto.Id, out var level))
            //    {
            //        dto.AuthStandardLevel = level;
            //    }
            //}

            return new PagedResultDto<ClientUserDto>(count, mapResult);
        }

        

        /// <summary>
        /// 获取客户端用户下拉
        /// </summary>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_UserManagement_UserList_BtnQuery)]
        public async Task<List<ClientUserDto>> GetClientUserItems()
        {
            var result = await _clientUserRepository.GetAll().Select(n => new ClientUserDto
            {
                Id = n.Id,
                UserName = n.UserName
            }).ToListAsync();

            return result;
        }
    }
}

