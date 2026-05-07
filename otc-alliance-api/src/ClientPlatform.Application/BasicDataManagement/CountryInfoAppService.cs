using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClientPlatform.Authorization;
using ClientPlatform.BasicDataManagement.Dto;

namespace ClientPlatform.BasicDataManagement
{

    /// <summary>
    /// 
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_BasicData_CountriesAndRegions)]
    public class CountryInfoAppService:AppServiceBase
    {
        private IRepository<CountryInfo, int> _repository;

        private readonly IHttpClientFactory _httpClientFactory;


        private static readonly Dictionary<string, string> RegionTranslations = new()
        {
            ["Asia"] = "亚洲",
            ["Europe"] = "欧洲",
            ["Africa"] = "非洲",
            ["Oceania"] = "大洋洲",
            ["Americas"] = "美洲",
            ["Antarctic"] = "南极洲"
        };

        private static readonly Dictionary<string, string> SubregionTranslations = new()
        {
            ["Eastern Asia"] = "东亚",
            ["Southeast Asia"] = "东南亚",
            ["Southern Asia"] = "南亚",
            ["Western Asia"] = "西亚",
            ["Central Asia"] = "中亚",

            ["Northern Europe"] = "北欧",
            ["Southern Europe"] = "南欧",
            ["Western Europe"] = "西欧",
            ["Eastern Europe"] = "东欧",

            ["Northern Africa"] = "北非",
            ["Middle Africa"] = "中非",
            ["Southern Africa"] = "南部非洲",
            ["Eastern Africa"] = "东非",
            ["Western Africa"] = "西非",

            ["Melanesia"] = "美拉尼西亚",
            ["Micronesia"] = "密克罗尼西亚",
            ["Polynesia"] = "波利尼西亚",
            ["Australia and New Zealand"] = "澳大利亚和新西兰",

            ["Caribbean"] = "加勒比",
            ["South America"] = "南美洲",
            ["North America"] = "北美洲",
            ["Central America"] = "中美洲",
            ["Antarctica"] = "南极洲"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="httpClientFactory"></param>
        public CountryInfoAppService(IRepository<CountryInfo, int> repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<CountryInfoDto>> GetCountryInfoListAsync(SearchAndPagedDto input)
        {
            var query = _repository.GetAll().WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name) || x.CNName.Contains(input.Name))
                .WhereIf(!input.CCA2.IsNullOrWhiteSpace(), x => x.CCA2 == input.CCA2 || x.CCA3 == input.CCA2 || x.CCN3 == input.CCA2)
                .WhereIf(!input.Currency.IsNullOrWhiteSpace(), x => x.Currency.Contains(input.Currency))
                .WhereIf(!input.Region.IsNullOrWhiteSpace(), x => x.Region.Contains(input.Region) || x.SubRegion.Contains(input.Region) || x.CnRegion.Contains(input.Region) || x.CnSubRegion.Contains(input.Region))
                .OrderByDescending(x => x.CreationTime);


            var count = await query.CountAsync();
            var items = await query.PageBy(input).ToListAsync();


            return new PagedResultDto<CountryInfoDto>(count, ObjectMapper.Map<List<CountryInfoDto>>(items));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CountryInfoDto> GetAsync(int id)
        {
            var entity = await _repository.GetAsync(id);
            return ObjectMapper.Map<CountryInfoDto>(entity);
        }


        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AbpAuthorize(PermissionNames.Pages_BasicData_CountriesAndRegions_BtnEdit)]
        public async Task<CountryInfoDto> CreateAsync(CountryInfoDto input)
        {
            var entity = ObjectMapper.Map<CountryInfo>(input);
            var isExist = await _repository.FirstOrDefaultAsync(x => x.Name == entity.Name || x.Name == entity.CNName || x.CCA2 == entity.CCA2 || x.CCA3 == entity.CCA3 || x.CCN3 == entity.CCN3);
            if (isExist != null)
            {
                throw new Exception("CountryInfo already exists");
            }
            //entity.Id = Guid.NewGuid();
            await _repository.InsertAsync(entity);
            return ObjectMapper.Map<CountryInfoDto>(entity);
        }

        [AbpAuthorize(PermissionNames.Pages_BasicData_CountriesAndRegions_BtnEdit)]

        public async Task<CountryInfoDto> UpdateAsync(CountryInfoDto input)
        {
            var entity = await _repository.GetAsync(input.Id.Value);

            var isExist = await _repository.FirstOrDefaultAsync(x => x.Name == input.Name || x.Name == input.CNName || x.CCA2 == input.CCA2 || x.CCA3 == input.CCA3 || x.CCN3 == input.CCN3);

            if (isExist != null && isExist.Id != entity.Id)
            {
                throw new Exception("CountryInfo already exists");
            }
            ObjectMapper.Map(input, entity);

            await _repository.UpdateAsync(entity);
            return ObjectMapper.Map<CountryInfoDto>(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AbpAuthorize(PermissionNames.Pages_BasicData_CountriesAndRegions_BtnDelete)]
        public async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetAsync(id);
            if (entity == null)
            {
                throw new Exception("CountryInfo not found");
            }
            await _repository.DeleteAsync(entity);
        }

        /// <summary>
        /// 初始化国家地区信息
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Abp.UI.UserFriendlyException"></exception>
        [AbpAuthorize(PermissionNames.Pages_BasicData_CountriesAndRegions_BtnInitData)]
        public async Task SyncCountriesAsync()
        {
            var url = "https://raw.githubusercontent.com/mledoze/countries/master/dist/countries.json";
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var countries = JsonSerializer.Deserialize<List<JsonElement>>(json);

            if (countries == null) throw new Abp.UI.UserFriendlyException("Failed to deserialize country data.");

            foreach (var c in countries)
            {
                try
                {
                    var name = c.GetProperty("name").GetProperty("common").GetString();
                    var cnName = c.TryGetProperty("translations", out var trans) &&
                                 trans.TryGetProperty("zho", out var zho) &&
                                 zho.TryGetProperty("common", out var zhName)
                                 ? zhName.GetString()
                                 : null;

                    var cca2 = c.GetProperty("cca2").GetString();
                    var cca3 = c.GetProperty("cca3").GetString();
                    var ccn3 = c.GetProperty("ccn3").GetString();

                    var currencies = c.TryGetProperty("currencies", out var curDict);

                    var currenciesList = curDict.EnumerateObject().Select(cur => new
                    {
                        code = cur.Name,
                        name = cur.Value.TryGetProperty("name", out var n) ? n.GetString() : null,
                        symbol = cur.Value.TryGetProperty("symbol", out var s) ? s.GetString() : null
                    }).ToList();

                    var currencyString = string.Join(",", currenciesList.Select(x => x.code));


                    var timezones = c.TryGetProperty("callingCodes", out var tz)
                        ? string.Join(",", tz.EnumerateArray().Select(x => x.GetString()))
                        : "";

                    var region = c.TryGetProperty("region", out var reg) ? reg.GetString() : null;
                    var cnRegion = RegionTranslations.TryGetValue(region ?? "", out var regionZh) ? regionZh : "";

                    var subregion = c.TryGetProperty("subregion", out var subreg) ? subreg.GetString() : null;
                    var cnSubregion = SubregionTranslations.TryGetValue(subregion ?? "", out var subZh) ? subZh : "";

                    if (timezones.Length > 100)
                    {
                        timezones = ""; // 跳过不完整的数据
                    }

                    var entity = new CountryInfo
                    {
                        Name = name,
                        CNName = cnName,
                        CCA2 = cca2,
                        CCA3 = cca3,
                        CCN3 = ccn3,
                        Currency = currencyString,
                        AreaPhoneCode = timezones,
                        Region = region,
                        CnRegion = cnRegion,
                        SubRegion = subregion,
                        CnSubRegion = cnSubregion
                    };

                    var isEixist = await _repository.FirstOrDefaultAsync(x => x.Name == entity.Name || x.Name == entity.CNName || x.CCA2 == entity.CCA2 || x.CCA3 == entity.CCA3 || x.CCN3 == entity.CCN3);
                    if (isEixist != null)
                    {
                        // 如果已存在，则更新
                        isEixist.Name = entity.Name;
                        isEixist.CNName = entity.CNName;
                        isEixist.CCA2 = entity.CCA2;
                        isEixist.CCA3 = entity.CCA3;
                        isEixist.CCN3 = entity.CCN3;
                        isEixist.Currency = entity.Currency;
                        isEixist.AreaPhoneCode = entity.AreaPhoneCode;
                        isEixist.Region = entity.Region;
                        isEixist.Currency = currencyString;
                        isEixist.SubRegion = entity.SubRegion;
                        isEixist.CnRegion = entity.CnRegion;
                        isEixist.CnSubRegion = entity.CnSubRegion;
                        await _repository.UpdateAsync(isEixist);
                    }
                    else
                    {
                        // 如果不存在，则插入
                        await _repository.InsertAsync(entity);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }


        /// <summary>
        /// 获取国家列表下拉数据
        /// </summary>
        /// <returns></returns>
        public async Task<List<CountryInfoDto>> GetAllForSelectItemAsync()
        {
            var list = await _repository.GetAll().Select(n => new CountryInfoDto
            {
                Name = n.Name,
                CNName = n.CNName,
                CCA2 = n.CCA2,
                CCA3 = n.CCA3,
                CCN3 = n.CCN3,
                Region = n.Region,
                SubRegion = n.SubRegion,
                CnRegion = n.CnRegion,
                CnSubRegion = n.CnSubRegion

            }).
            ToListAsync();
            return list;
        }

    }
}
