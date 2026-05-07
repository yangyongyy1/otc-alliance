using Abp.Application.Services;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform.Authorization.Users;

namespace ClientPlatform
{
    /// <summary>
    ///
    /// </summary>
    ///
    [AbpAuthorize]
    public class AppServiceBase : ApplicationService
    {
        private readonly string DefaultCacheName = ClientPlatformConsts.CacheNamespace;

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
            UserManager= IocManager.Instance.Resolve<UserManager>();
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
    }
}
