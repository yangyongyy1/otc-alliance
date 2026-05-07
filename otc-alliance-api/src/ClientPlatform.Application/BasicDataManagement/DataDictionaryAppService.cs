using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientPlatform.Authorization;
using ClientPlatform.BasicDataManagement.Dto;
using ClientPlatform;

namespace ClientPlatform.BasicDataManagement
{
    [AbpAuthorize(PermissionNames.Pages_BasicData_DataDictionary)]
    public class DataDictionaryAppService : AppServiceBase, IDataDictionaryAppService
    {
        private readonly IRepository<DataDictionary, int> _repository;

        public DataDictionaryAppService(IRepository<DataDictionary, int> repository)
        {
            _repository = repository;
        }

        [AbpAuthorize(PermissionNames.Pages_BasicData_DataDictionary_BtnQuery)]
        public async Task<PagedResultDto<DataDictionaryDto>> GetDataDictionaryListAsync(GetDataDictionaryInput input)
        {
            var query = _repository.GetAll()
                .WhereIf(!input.DicKey.IsNullOrWhiteSpace(), x => x.DicKey.Contains(input.DicKey))
                .WhereIf(!input.DicValue.IsNullOrWhiteSpace(), x => x.DicValue.Contains(input.DicValue))
                .WhereIf(input.DicType.HasValue, x => x.DicType == input.DicType.Value);

            var count = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreationTime).PageBy(input).ToListAsync();

            return new PagedResultDto<DataDictionaryDto>(count, ObjectMapper.Map<List<DataDictionaryDto>>(items));
        }

        public async Task<DataDictionaryDto> GetAsync(int id)
        {
            var entity = await _repository.GetAsync(id);
            return ObjectMapper.Map<DataDictionaryDto>(entity);
        }

        [AbpAuthorize(PermissionNames.Pages_BasicData_DataDictionary_BtnAdd)]
        public async Task<DataDictionaryDto> CreateAsync(CreateDataDictionaryDto input)
        {
            var exists = await _repository.FirstOrDefaultAsync(x => x.DicKey == input.DicKey);
            if (exists != null)
            {
                throw new UserFriendlyException("DataDictionary Key already exists");
            }

            var entity = ObjectMapper.Map<DataDictionary>(input);
            await _repository.InsertAsync(entity);
            return ObjectMapper.Map<DataDictionaryDto>(entity);
        }

        [AbpAuthorize(PermissionNames.Pages_BasicData_DataDictionary_BtnEdit)]
        public async Task<DataDictionaryDto> UpdateAsync(UpdateDataDictionaryDto input)
        {
            var entity = await _repository.GetAsync(input.Id);

            var exists = await _repository.FirstOrDefaultAsync(x => x.DicKey == input.DicKey && x.Id != input.Id);
            if (exists != null)
            {
                throw new UserFriendlyException("DataDictionary Key already exists");
            }

            ObjectMapper.Map(input, entity);
            await _repository.UpdateAsync(entity);
            return ObjectMapper.Map<DataDictionaryDto>(entity);
        }

        [AbpAuthorize(PermissionNames.Pages_BasicData_DataDictionary_BtnDelete)]
        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        [AbpAllowAnonymous]
        public async Task<string> GetDataDic(string Key)
        {

            var entity = await _repository.FirstOrDefaultAsync(n => n.DicKey == Key);

            if (entity != null)
            {
                return entity.DicValue;
            }
            return "";
        }

        
    }
}
