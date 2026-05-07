using Abp.Application.Services;
using ClientPlatform.MultiTenancy.Dto;

namespace ClientPlatform.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

