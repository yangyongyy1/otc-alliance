using Abp.Application.Services;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.Authorization.Users;
using ClientPlatform.UserManagement;

namespace ClientPlatformUser
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    [AbpAuthorize]
    public  class AppServiceBase: ApplicationService
    {
        

        

        private readonly string DefaultCacheName = ClientPlatformConsts.CacheNamespace;

        private IRepository<ClientUser, int> _repository;

        
        /// <summary>
        /// 
        /// </summary>

        protected UserManager UserManager { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected AppServiceBase()
        {
            LocalizationSourceName = ClientPlatformConsts.LocalizationSourceName;
            _repository=IocManager.Instance.Resolve<IRepository<ClientUser, int>>();
        }

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }
            return user;
        }

        protected virtual async Task<ClientUser> GetCurrentClientUserAsync()
        {
            var userId = AbpSession.GetUserId();
            var clientUser = await _repository.FirstOrDefaultAsync(u => u.AbpUserId == userId);
            if (clientUser == null)
            {
                throw new Exception("There is no current client user!");
            }
            return clientUser;
        }

    }
}
