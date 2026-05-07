using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using ClientPlatform.BasicDataManagement.Dto;

namespace ClientPlatform.BasicDataManagement
{
    public interface IDataDictionaryAppService : IApplicationService
    {
        Task<PagedResultDto<DataDictionaryDto>> GetDataDictionaryListAsync(GetDataDictionaryInput input);
        Task<DataDictionaryDto> GetAsync(int id);
        Task<DataDictionaryDto> CreateAsync(CreateDataDictionaryDto input);
        Task<DataDictionaryDto> UpdateAsync(UpdateDataDictionaryDto input);
        Task DeleteAsync(int id);
    }
}
