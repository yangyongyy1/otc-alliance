using Abp.Authorization;
using ClientPlatform.Authorization.Roles;
using ClientPlatform.Authorization.Users;

namespace ClientPlatform.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
