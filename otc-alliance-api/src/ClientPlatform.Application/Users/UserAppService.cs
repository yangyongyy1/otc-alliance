using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClientPlatform.Authorization;
using ClientPlatform.Authorization.Roles;
using ClientPlatform.Authorization.Users;
using ClientPlatform.Roles.Dto;
using ClientPlatform.Users.Dto;

namespace ClientPlatform.Users;

//[AbpAuthorize(PermissionNames.Pages_Users)]
public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
{
    private readonly UserManager _userManager;
    private readonly RoleManager _roleManager;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IAbpSession _abpSession;
    private readonly LogInManager _logInManager;
    private readonly IRepository<User, long> _abpUserRepository;

    public UserAppService(
        IRepository<User, long> repository,
        UserManager userManager,
        RoleManager roleManager,
        IRepository<Role> roleRepository,
        IPasswordHasher<User> passwordHasher,
        IAbpSession abpSession,
        LogInManager logInManager,
        IRepository<User, long> abpUserRepository)
        : base(repository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _abpSession = abpSession;
        _logInManager = logInManager;
        _abpUserRepository = abpUserRepository;

    }

    /// <summary>
    /// 直接按 UserType=PlatformUser 插入用户，不经过 UserManager.CreateAsync，避免同邮箱存在 BusinessUser 时报「邮箱被占」。
    /// </summary>
    public override async Task<UserDto> CreateAsync(CreateUserDto input)
    {
        var normalizedUserName = _userManager.KeyNormalizer?.NormalizeName(input.UserName) ?? input.UserName?.ToUpperInvariant();
        var normalizedEmail = _userManager.KeyNormalizer?.NormalizeEmail(input.EmailAddress) ?? input.EmailAddress?.ToUpperInvariant();

        var existsByUserName = await _abpUserRepository.FirstOrDefaultAsync(u =>
            u.NormalizedUserName == normalizedUserName
            && u.UserType != SystemUserType.BusinessUser
            && !u.IsDeleted);
        if (existsByUserName != null)
        {
            throw new UserFriendlyException("用户名已存在");
        }

        var existsByEmail = await _abpUserRepository.FirstOrDefaultAsync(u =>
            u.NormalizedEmailAddress == normalizedEmail
            && u.UserType != SystemUserType.BusinessUser
            && !u.IsDeleted);
        if (existsByEmail != null)
        {
            throw new UserFriendlyException("邮箱已被使用");
        }

        var user = new User
        {
            AuthenticationSource = "",
            EmailAddress = input.EmailAddress,
            IsActive = true,
            IsDeleted = false,
            IsEmailConfirmed = true,
            IsLockoutEnabled = false,
            IsPhoneNumberConfirmed = false,
            Name = input.Name ?? input.UserName ?? "",
            Surname = input.Surname ?? "",
            PasswordResetCode = "",
            PhoneNumber = "",
            TenantId = 1,
            UserName = input.UserName,
            EmailConfirmationCode = "",
            UserType = SystemUserType.PlatformUser,
            Roles = new List<Abp.Authorization.Users.UserRole>()
        };
        user.SetNormalizedNames();
        user.Password = _passwordHasher.HashPassword(user, input.Password);

        await _abpUserRepository.InsertAsync(user);
        await CurrentUnitOfWork.SaveChangesAsync();

        await _userManager.SetRolesAsync(user, input.RoleNames);
        return MapToEntityDto(user);
    }

    public override async Task<UserDto> UpdateAsync(UserDto input)
    {
        CheckUpdatePermission();

        var user = await _userManager.GetUserByIdAsync(input.Id);

        MapToEntity(input, user);

        CheckErrors(await _userManager.UpdateAsync(user));

        if (input.RoleNames != null)
        {
            CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
        }

        return await GetAsync(input);
    }

    public override async Task DeleteAsync(EntityDto<long> input)
    {
        var user = await _userManager.GetUserByIdAsync(input.Id);
        await _userManager.DeleteAsync(user);
    }

    //[AbpAuthorize(PermissionNames.Pages_Users_Activation)]
    public async Task Activate(EntityDto<long> user)
    {
        await Repository.UpdateAsync(user.Id, async (entity) =>
        {
            entity.IsActive = true;
        });
    }

    //[AbpAuthorize(PermissionNames.Pages_Users_Activation)]
    public async Task DeActivate(EntityDto<long> user)
    {
        await Repository.UpdateAsync(user.Id, async (entity) =>
        {
            entity.IsActive = false;
        });
    }

    public async Task<ListResultDto<RoleDto>> GetRoles()
    {
        var roles = await _roleRepository.GetAllListAsync();
        return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
    }


    /// <summary>
    /// 用户时区设置
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [AbpAuthorize]
    public async Task SetUserTimeZoneValue(int value)
    {
        var user = await Repository.GetAsync(AbpSession.UserId.Value);
        user.TimeZoneValue = value;
        await Repository.UpdateAsync(user);
    }

    public async Task ChangeLanguage(ChangeUserLanguageDto input)
    {
        await SettingManager.ChangeSettingForUserAsync(
            AbpSession.ToUserIdentifier(),
            LocalizationSettingNames.DefaultLanguage,
            input.LanguageName
        );
    }

    protected override User MapToEntity(CreateUserDto createInput)
    {
        var user = ObjectMapper.Map<User>(createInput);
        user.SetNormalizedNames();
        return user;
    }

    protected override void MapToEntity(UserDto input, User user)
    {
        ObjectMapper.Map(input, user);
        user.SetNormalizedNames();
    }

    protected override UserDto MapToEntityDto(User user)
    {
        var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

        var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

        var userDto = base.MapToEntityDto(user);
        userDto.RoleNames = roles.ToArray();

        return userDto;
    }

    protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
    {
        return Repository.GetAllIncluding(x => x.Roles)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .Where(n=>n.UserType!= SystemUserType.BusinessUser)
            ;
    }

    protected override async Task<User> GetEntityByIdAsync(long id)
    {
        var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            throw new EntityNotFoundException(typeof(User), id);
        }

        return user;
    }

    protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
    {
        return query.OrderBy(input.Sorting);
    }

    protected virtual void CheckErrors(IdentityResult identityResult)
    {
        identityResult.CheckErrors(LocalizationManager);
    }

    public async Task<bool> ChangePassword(ChangePasswordDto input)
    {
        await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

        var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());
        if (user == null)
        {
            throw new Exception("There is no current user!");
        }

        if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
        {
            CheckErrors(await _userManager.ChangePasswordAsync(user, input.NewPassword));
        }
        else
        {
            CheckErrors(IdentityResult.Failed(new IdentityError
            {
                Description = "Incorrect password."
            }));
        }

        return true;
    }

    public async Task<bool> ResetPassword(ResetPasswordDto input)
    {
        if (_abpSession.UserId == null)
        {
            throw new UserFriendlyException("Please log in before attempting to reset password.");
        }

        var currentUser = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
        var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
        if (loginAsync.Result != AbpLoginResultType.Success)
        {
            throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
        }

        if (currentUser.IsDeleted || !currentUser.IsActive)
        {
            return false;
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        if (!roles.Contains(StaticRoleNames.Tenants.Admin))
        {
            throw new UserFriendlyException("Only administrators may reset passwords.");
        }

        var user = await _userManager.GetUserByIdAsync(input.UserId);
        if (user != null)
        {
            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return true;
    }
}

