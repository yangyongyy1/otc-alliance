using Abp.Application.Services;
using ClientPlatform.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace ClientPlatform.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}
