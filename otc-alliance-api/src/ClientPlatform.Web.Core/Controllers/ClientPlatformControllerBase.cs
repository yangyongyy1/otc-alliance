using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace ClientPlatform.Controllers
{
    public abstract class ClientPlatformControllerBase : AbpController
    {
        protected ClientPlatformControllerBase()
        {
            LocalizationSourceName = ClientPlatformConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
