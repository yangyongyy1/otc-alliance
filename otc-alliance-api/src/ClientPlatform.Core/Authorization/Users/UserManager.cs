using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClientPlatform.Authorization.Roles;

namespace ClientPlatform.Authorization.Users;

public class UserManager : AbpUserManager<Role, User>
{
    public UserManager(
      RoleManager roleManager,
      UserStore store,
      IOptions<IdentityOptions> optionsAccessor,
      IPasswordHasher<User> passwordHasher,
      IEnumerable<IUserValidator<User>> userValidators,
      IEnumerable<IPasswordValidator<User>> passwordValidators,
      ILookupNormalizer keyNormalizer,
      IdentityErrorDescriber errors,
      IServiceProvider services,
      ILogger<UserManager<User>> logger,
      IPermissionManager permissionManager,
      IUnitOfWorkManager unitOfWorkManager,
      ICacheManager cacheManager,
      IRepository<OrganizationUnit, long> organizationUnitRepository,
      IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
      IOrganizationUnitSettings organizationUnitSettings,
      ISettingManager settingManager,
      IRepository<UserLogin, long> userLoginRepository)
      : base(
          roleManager,
          store,
          optionsAccessor,
          passwordHasher,
          userValidators,
          passwordValidators,
          keyNormalizer,
          errors,
          services,
          logger,
          permissionManager,
          unitOfWorkManager,
          cacheManager,
          organizationUnitRepository,
          userOrganizationUnitRepository,
          organizationUnitSettings,
          settingManager,
          userLoginRepository)
    {
    }

    public async Task<long> CreateUserAsync(string emailAddress, string defalutPassWord)
    {
        var user = new User
        {
            UserName = emailAddress,
            Name = emailAddress.Split('@')[0], // 使用邮箱前缀作为Name
            Surname = "",
            EmailAddress = emailAddress,
            IsActive = true,
            UserType= SystemUserType.BusinessUser,
            IsEmailConfirmed = true, // 已验证邮箱验证码，直接确认
            Roles = new List<Abp.Authorization.Users.UserRole>()
        };

        user.SetNormalizedNames();
        var identityResult = await CreateAsync(user, defalutPassWord);
        return user.Id;
    }
}
