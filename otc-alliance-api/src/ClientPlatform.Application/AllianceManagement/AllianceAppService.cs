using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform.AllianceManagement.Dot;
using ClientPlatform.Authorization;
using Utility;

namespace ClientPlatform.AllianceManagement
{
    /// <summary>
    /// 联盟管理
    /// </summary>
    /// 
    [AbpAuthorize(PermissionNames.Pages_AllianceManagement_AllianceList)]
    public class AllianceAppService: AppServiceBase
    {
        private readonly IRepository<Alliance, int> _allianceRepository;
        private ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allianceRepository"></param>
        /// <param name="logger"></param>
        private readonly IRepository<Merchant, int> _merchantRepository;

        public AllianceAppService(IRepository<Alliance, int> allianceRepository, IRepository<Merchant, int> merchantRepository, ILogger logger)
        {
            _allianceRepository = allianceRepository;
            _merchantRepository = merchantRepository;
            _logger = logger;
        }


        /// <summary>
        /// 获取联盟列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_AllianceList_BtnQuery)]
        public async Task<PagedResultDto<AllianceDto>> GetAllianceAllListAsync(PagedAllianceResultRequestDto input)
        {
            _logger.Info("GetAllianceAllListAsync start");
            
            var qry = _allianceRepository.GetAll()
                       .WhereIf(!input.Name.IsNullOrWhiteSpace(), n => n.Name.Contains(input.Name))
                       .WhereIf(!input.SerialNumber.IsNullOrWhiteSpace(), n => n.SerialNumber.Contains(input.SerialNumber))
                       .WhereIf(input.CreationTimeStart.HasValue, n => n.CreationTime >= input.CreationTimeStart.Value)
                       .WhereIf(input.CreationTimeEnd.HasValue, n => n.CreationTime <= input.CreationTimeEnd.Value)
                       .WhereIf(input.ModificationTimeStart.HasValue, n => n.LastModificationTime >= input.ModificationTimeStart.Value)
                       .WhereIf(input.ModificationTimeEnd.HasValue, n => n.LastModificationTime <= input.ModificationTimeEnd.Value)
                       .OrderBy(input.Sorting);

            var count = await qry.CountAsync();

            var pagedResult = await qry.PageBy(input).ToListAsync();

            var mapResult = ObjectMapper.Map<AllianceDto[]>(pagedResult);

            return new PagedResultDto<AllianceDto>(count, mapResult);
                            
        }


        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_AllianceList_BtnSettings)]

        public async Task<Alliance> GetAllianceSettingAsync(int id)
        {
            var entity = await _allianceRepository.GetAsync(id);
            return entity;
        }

        /// <summary>
        /// 创建联盟
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_AllianceList_BtnAdd)]
        [Audited]
        public async Task CreateAllianceAsync(CreateOrUpdateAllianceDto input)
        {

            var isSameNameRecord = await _allianceRepository.GetAll().AnyAsync(n => n.Name == input.Name);
            if (isSameNameRecord)
            {
                throw new Abp.UI.UserFriendlyException("联盟名称已存在");
            }

            var serialNumber = SerialNumberHelper.GenerateSerial(16);
            while (true)
            {
                var isExists = await _allianceRepository.GetAll().AnyAsync(n => n.SerialNumber == serialNumber);
                if (!isExists)
                {
                    break;
                }
                else
                {
                    serialNumber = SerialNumberHelper.GenerateSerial(16);
                }

            }

            Alliance entity = new Alliance();
            entity.Name = input.Name;
            entity.SerialNumber = serialNumber;

            await _allianceRepository.InsertAsync(entity);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Abp.UI.UserFriendlyException"></exception>
        [Audited]
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_AllianceList_BtnSettings)]
        public async Task  SetAllianceSettingAsync(SettingAllianceDto input)
        {
            var entity = await _allianceRepository.GetAsync(input.Id);
            if (entity == null)
            {
                throw new Abp.UI.UserFriendlyException("联盟不存在");
            }
            entity.LogoUrl = input.LogoUrl;
            entity.UserAgreementLink = input.UserAgreementLink;
            entity.WebSiteUrl = input.WebSiteUrl;
            entity.CopyrightInfo = input.CopyrightInfo;
            entity.Disclaimer = input.Disclaimer;
            entity.WebSiteIcon = input.WebSiteIcon;
            entity.ThemeColor = input.ThemeColor;
            entity.ServiceEmail = input.ServiceEmail;
            await _allianceRepository.UpdateAsync(entity);
        }

        /// <summary>
        /// 删除联盟
        /// </summary>
        /// <param name="id">联盟主键</param>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_AllianceList_BtnDelete)]
        [Audited]
        public async Task DeleteAllianceAsync(int id)
        {
            var entity = await _allianceRepository.FirstOrDefaultAsync(id);
            if (entity == null)
                throw new Abp.UI.UserFriendlyException("联盟不存在");

            var hasMerchants = await _merchantRepository.GetAll().AnyAsync(m => m.AllianceId == id);
            if (hasMerchants)
                throw new Abp.UI.UserFriendlyException("该联盟下存在商户，无法删除");

            await _allianceRepository.DeleteAsync(entity);
        }

        /// <summary>
        /// 获取联盟下拉
        /// </summary>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<List<AllianceDto>> GetAllianceItems()
        { 
            var result = await _allianceRepository.GetAll().Select(n => new AllianceDto
            {
                Id = n.Id,
                Name = n.Name
            }).ToListAsync();

            return result;
        }

        
    }
}
