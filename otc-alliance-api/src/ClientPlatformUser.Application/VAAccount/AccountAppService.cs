using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform.VABusiness;
using ClientPlatformUser.VAAccount.Dto;

namespace ClientPlatformUser.VAAccount
{
    [AbpAuthorize]
    public class AccountAppService : AppServiceBase
    {
        private IRepository<ClientPlatform.VABusiness.VAAccount, int> _vaAccountRepository;

        private IRepository<ClientPlatform.VABusiness.VAAccountIdentity, int> _vaAccountIdentityRepository;

        private IRepository<VAAccountDetail, int> _vaAccountDetailRepository;




       /// <summary>
       /// 
       /// </summary>
       /// <param name="vaAccountRepository"></param>
       /// <param name="vaAccountIdentityRepository"></param>
       /// <param name="vaAccountDetailRepository"></param>
        public AccountAppService(IRepository<ClientPlatform.VABusiness.VAAccount, int> vaAccountRepository,
            IRepository<ClientPlatform.VABusiness.VAAccountIdentity, int> vaAccountIdentityRepository,
            IRepository<VAAccountDetail, int> vaAccountDetailRepository

            )
        {
            _vaAccountRepository = vaAccountRepository;
            _vaAccountIdentityRepository = vaAccountIdentityRepository;
            _vaAccountDetailRepository = vaAccountDetailRepository;
        }


        /// <summary>
        /// 获取账户列表(分页)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<VAAccountDto>> GetAccountListAsync(AccountRequestDto input)
        {
            var clientUser = await GetCurrentClientUserAsync();

            var qry = _vaAccountRepository.GetAll().Where(a => a.UserId == clientUser.Id).OrderByDescending(n => n.CreationTime);

            var count = qry.Count();

            var list = qry.PageBy(input).ToList();

            var mapList = ObjectMapper.Map<List<VAAccountDto>>(list);

            return new PagedResultDto<VAAccountDto>(count, mapList);
        }


        /// <summary>
        /// 获取账户详情
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<VAAccountDetailDto> GetAccountDetailAsync(int accountId)
        {
            var clientUser = await GetCurrentClientUserAsync();
            var detail = await _vaAccountDetailRepository.FirstOrDefaultAsync(a => a.VAAccountId == accountId);
            if (detail == null)
            {
                return null;
            }
            var mapDetail = ObjectMapper.Map<VAAccountDetailDto>(detail);
            return mapDetail;
        }


        /// <summary>
        /// 获取用户进件历史
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<VAAccountIdentityDto>> GetAccountIdentityHistoryAsync(AccountIdentityReqesutDto input)
        {
            var clientUser = await GetCurrentClientUserAsync();

            var qry =  _vaAccountIdentityRepository.GetAll().Where(n => n.UserId == clientUser.Id).OrderByDescending(n => n.CreationTime);

            var count= await qry.CountAsync();


            var page = await qry.PageBy(input).ToListAsync();

            var map = ObjectMapper.Map<List<VAAccountIdentityDto>>(page);

            return new PagedResultDto<VAAccountIdentityDto>(count,map);

        }


        /// <summary>
        /// 账户进件历史详情
        /// </summary>
        /// <param name="accountIdentityId"></param>
        /// <returns></returns>
        public async Task<VAAccountIdentityDetailDto> GetAccountIdentityDetailAsync(int accountIdentityId)
        {
            var clientUser = await GetCurrentClientUserAsync();
            var detail = await _vaAccountIdentityRepository.FirstOrDefaultAsync(n => n.Id == accountIdentityId && n.UserId == clientUser.Id);
            if (detail == null)
            {
                return null;
            }
            var mapDetail = ObjectMapper.Map<VAAccountIdentityDetailDto>(detail);
            return mapDetail;
        }
    }
}
