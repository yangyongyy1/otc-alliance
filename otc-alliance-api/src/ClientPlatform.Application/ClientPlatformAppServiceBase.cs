using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.UI;
using ClientPlatform.Authorization.Users;
using ClientPlatform.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClientPlatform;

/// <summary>
/// Derive your application services from this class.
/// </summary>
public abstract class ClientPlatformAppServiceBase : ApplicationService
{
    public TenantManager TenantManager { get; set; }

    public UserManager UserManager { get; set; }

    protected ClientPlatformAppServiceBase()
    {
        LocalizationSourceName = ClientPlatformConsts.LocalizationSourceName;
    }

    protected virtual async Task<User> GetCurrentUserAsync()
    {
        var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
        if (user == null)
        {
            throw new Exception("There is no current user!");
        }

        return user;
    }

    protected virtual Task<Tenant> GetCurrentTenantAsync()
    {
        return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
    }

    protected virtual void CheckErrors(IdentityResult identityResult)
    {
        if (identityResult.Succeeded) return;

        // 优先使用 IdentityError.Description（如 UserManager 自定义校验返回的消息），避免显示「发生了一个未知的异常错误」
        var descriptions = identityResult.Errors
            .Where(e => !string.IsNullOrWhiteSpace(e.Description))
            .Select(e => e.Description)
            .ToList();
        if (descriptions.Count > 0)
        {
            throw new UserFriendlyException(string.Join("; ", descriptions));
        }

        identityResult.CheckErrors(LocalizationManager);
    }
}
