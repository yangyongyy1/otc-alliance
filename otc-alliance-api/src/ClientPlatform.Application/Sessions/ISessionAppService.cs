using Abp.Application.Services;
using ClientPlatform.Sessions.Dto;
using System.Threading.Tasks;

namespace ClientPlatform.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
