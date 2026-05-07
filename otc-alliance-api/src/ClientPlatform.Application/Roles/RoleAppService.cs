using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ClientPlatform.Authorization;
using ClientPlatform.Authorization.Roles;
using ClientPlatform.Authorization.Users;
using ClientPlatform.Roles.Dto;

namespace ClientPlatform.Roles;

//[AbpAuthorize(PermissionNames.Pages_Roles)]
public class RoleAppService : AsyncCrudAppService<Role, RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>, IRoleAppService
{
    private readonly RoleManager _roleManager;
    private readonly UserManager _userManager;
    private readonly IRepository<UserRole, long> _userRoleRepository;

    public RoleAppService(IRepository<Role> repository, RoleManager roleManager, UserManager userManager,
        IRepository<UserRole, long> userRoleRepository
        )
        : base(repository)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _userRoleRepository = userRoleRepository;
    }

    public override async Task<RoleDto> CreateAsync(CreateRoleDto input)
    {
        CheckCreatePermission();

        var role = ObjectMapper.Map<Role>(input);
        role.SetNormalizedName();

        CheckErrors(await _roleManager.CreateAsync(role));

        var permissionNames = input.GrantedPermissions ?? new List<string>();
        var grantedPermissions = PermissionManager
            .GetAllPermissions()
            .Where(p => permissionNames.Contains(p.Name))
            .ToList();

        await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

        return MapToEntityDto(role);
    }

    public async Task<ListResultDto<RoleListDto>> GetRolesAsync(GetRolesInput input)
    {
        var roles = await _roleManager
            .Roles
            .WhereIf(
                !input.Permission.IsNullOrWhiteSpace(),
                r => r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
            )
            .ToListAsync();

        return new ListResultDto<RoleListDto>(ObjectMapper.Map<List<RoleListDto>>(roles));
    }

    public override async Task<RoleDto> UpdateAsync(RoleDto input)
    {
        CheckUpdatePermission();

        var role = await _roleManager.GetRoleByIdAsync(input.Id);

        ObjectMapper.Map(input, role);

        CheckErrors(await _roleManager.UpdateAsync(role));

        var permissionNames = input.GrantedPermissions ?? new List<string>();
        var grantedPermissions = PermissionManager
            .GetAllPermissions()
            .Where(p => permissionNames.Contains(p.Name))
            .ToList();

        await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

        return MapToEntityDto(role);
    }

    public override async Task DeleteAsync(EntityDto<int> input)
    {
        CheckDeletePermission();

        var role = await _roleManager.FindByIdAsync(input.Id.ToString());
        var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);

        foreach (var user in users)
        {
            CheckErrors(await _userManager.RemoveFromRoleAsync(user, role.NormalizedName));
        }

        CheckErrors(await _roleManager.DeleteAsync(role));
    }

    public Task<ListResultDto<PermissionDto>> GetAllPermissions()
    {
        var permissions = PermissionManager.GetAllPermissions();

        return Task.FromResult(new ListResultDto<PermissionDto>(
            ObjectMapper.Map<List<PermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList()
        ));
    }

    protected override IQueryable<Role> CreateFilteredQuery(PagedRoleResultRequestDto input)
    {
        return Repository.GetAllIncluding(x => x.Permissions)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword)
            || x.DisplayName.Contains(input.Keyword)
            || x.Description.Contains(input.Keyword));
    }

    protected override async Task<Role> GetEntityByIdAsync(int id)
    {
        return await Repository.GetAllIncluding(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == id);
    }

    protected override IQueryable<Role> ApplySorting(IQueryable<Role> query, PagedRoleResultRequestDto input)
    {
        return query.OrderBy(input.Sorting);
    }

    protected virtual void CheckErrors(IdentityResult identityResult)
    {
        identityResult.CheckErrors(LocalizationManager);
    }

    public async Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input)
    {
        var permissions = PermissionManager.GetAllPermissions();
        var role = await _roleManager.GetRoleByIdAsync(input.Id);
        var grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
        var roleEditDto = ObjectMapper.Map<RoleEditDto>(role);

        return new GetRoleForEditOutput
        {
            Role = roleEditDto,
            Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList(),
            GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList()
        };
    }


    public async Task<ListResultDto<FlatPermissionWithLevelDto>> GetAllPermissionsWithLevel()
    {
        //var permissions = PermissionManager.GetAllPermissions();
        var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
        var userrole = await _userRoleRepository.FirstOrDefaultAsync(r => r.UserId == user.Id);
        var role = await _roleManager.GetRoleByIdAsync(userrole.RoleId);
        var grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
        var rootPermissions = grantedPermissions.Where(p => p.Parent == null);



        var result = new List<FlatPermissionWithLevelDto>();

        foreach (var rootPermission in rootPermissions)
        {
            var level = 0;
            AddPermission(rootPermission, grantedPermissions, result, level);
        }

        return new ListResultDto<FlatPermissionWithLevelDto>
        {
            Items = result
        };
    }

    private void AddPermission(Permission permission, IReadOnlyList<Permission> allPermissions, List<FlatPermissionWithLevelDto> result, int level)
    {
        var flatPermission = permission.MapTo<FlatPermissionWithLevelDto>();
        flatPermission.Level = level;
        result.Add(flatPermission);

        if (permission.Children == null)
        {
            return;
        }

        var children = allPermissions.Where(p => p.Parent != null && p.Parent.Name == permission.Name).ToList();

        foreach (var childPermission in children)
        {
            AddPermission(childPermission, allPermissions, result, level + 1);
        }
    }
}

