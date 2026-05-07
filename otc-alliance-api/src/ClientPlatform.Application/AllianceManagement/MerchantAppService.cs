using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Timing;
using AutoMapper.Internal.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using ClientPlatform.AllianceManagement.Dot;
using ClientPlatform.Authorization;
using ClientPlatform.Authorization.Users;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.Settings;
using Newtonsoft.Json.Linq;

namespace ClientPlatform.AllianceManagement
{
    /// <summary>
    /// 
    /// </summary>
    public class MerchantAppService : AppServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        public IRepository<Merchant, int> _merchantRepository { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IRepository<Alliance, int> _allianceRepository { get; set; }

        public IRepository<MerchantChannelCurrency, int> _merchantChannelCurrencyReposiotory { get; set; }

        public IRepository<MerchantPaySetting, Guid> _merchantPaySettingRepository { get; set; }

        public IRepository<User, long> _userRepository { get; set; }

        public IRepository<MerchantSubCode, int> _merchantSubCodeRepository { get; set; }

        private readonly IRepository<DataDictionary, int> _dataDictionaryRepository;
        private readonly MerchantPayClient _merchantPayClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="merchantRepository"></param>
        /// <param name="allianceRepository"></param>
        public MerchantAppService(IRepository<Merchant, int> merchantRepository,
        IRepository<Alliance, int> allianceRepository,
        IRepository<MerchantChannelCurrency, int> merchantChannelCurrencyReposiotory,
         IRepository<MerchantPaySetting, Guid> merchantPaySettingRepository,
          IRepository<User, long> userRepository,
          IRepository<MerchantSubCode, int> merchantSubCodeRepository,
          MerchantPayClient merchantPayClient,
          IRepository<DataDictionary, int> dataDictionaryRepository)
        {
            _merchantRepository = merchantRepository;
            _allianceRepository = allianceRepository;
            _merchantChannelCurrencyReposiotory = merchantChannelCurrencyReposiotory;
            _merchantPaySettingRepository = merchantPaySettingRepository;
            _userRepository = userRepository;
            _merchantSubCodeRepository = merchantSubCodeRepository;
            _merchantPayClient = merchantPayClient;
            _dataDictionaryRepository = dataDictionaryRepository;
        }


        /// <summary>
        /// 获取商户列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<MerchantDto>> GetMerchantListAsync(MerchantPagedAndSortRequestDto input)
        {
            var qry = _merchantRepository.GetAllIncluding(n => n.Alliance)
                      .WhereIf(!input.Name.IsNullOrWhiteSpace(), n => n.Name.Contains(input.Name))
                      .WhereIf(input.AllianceId.HasValue, n => n.AllianceId == input.AllianceId)
                      .WhereIf(input.CreationTimeStart.HasValue, n => n.CreationTime >= input.CreationTimeStart.Value)
                      .WhereIf(input.CreationTimeEnd.HasValue, n => n.CreationTime <= input.CreationTimeEnd.Value)
                      .WhereIf(input.ModificationTimeStart.HasValue, n => n.LastModificationTime >= input.ModificationTimeStart.Value)
                      .WhereIf(input.ModificationTimeEnd.HasValue, n => n.LastModificationTime <= input.ModificationTimeEnd.Value)
                      .OrderBy(input.Sorting);

            var count = await qry.CountAsync();

            var pagedResult = await qry.PageBy(input).ToListAsync();

            var dtos = ObjectMapper.Map<MerchantDto[]>(pagedResult);

            return new PagedResultDto<MerchantDto>(count, dtos);

        }

        /// <summary>
        /// 创建商户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Abp.UI.UserFriendlyException"></exception>
        public async Task CreateMerchantAsync(CreateMerchantDto input)
        {
            var entity = ObjectMapper.Map<Merchant>(input);

            var isAny = await _merchantRepository.FirstOrDefaultAsync(n => n.Name == input.Name.Trim());

            if (isAny != null)
            {
                throw new Abp.UI.UserFriendlyException("商户名已存在");
            }

            var relationCode = SerialNumberHelper.GenerateSerial(6);
            while (true)
            {
                var isExists = await _merchantRepository.GetAll().AnyAsync(n => n.RelationCode == relationCode);
                if (!isExists)
                {
                    break;
                }
                else
                {
                    relationCode = SerialNumberHelper.GenerateSerial(6);
                }
            }
            entity.RelationCode = relationCode;
            await _merchantRepository.InsertAsync(entity);

            // 创建商户后，按字典表 alliancesApiCallBack 初始化 SunPay VA 回调配置
            await TryInitSunPayVaCallbackConfigAsync(entity);
        }

        /// <summary>
        /// 创建商户后，读取字典表 alliancesApiCallBack 并配置 SunPay VA 回调
        /// </summary>
        private async Task TryInitSunPayVaCallbackConfigAsync(Merchant merchant)
        {
            try
            {
                var dic = await _dataDictionaryRepository.FirstOrDefaultAsync(x => x.DicKey == "alliancesApiCallBack");
                if (dic == null || dic.DicValue.IsNullOrWhiteSpace())
                {
                    Logger.Warn("alliancesApiCallBack dictionary not found or empty, skip init callback config.");
                    return;
                }

                var root = JObject.Parse(dic.DicValue);
                var domain = root["domain"]?.ToString() ?? string.Empty;
                var configs = root["commonBusinessApiCallBackConfig"] as JArray;
                if (configs == null || !configs.Any())
                {
                    Logger.Warn("alliancesApiCallBack.commonBusinessApiCallBackConfig is empty, skip init callback config.");
                    return;
                }

                var req = new UpsertBusinessApiCallBackConfigInput
                {
                    CallbackBaseUrl = domain,
                    CommonBusinessApiCallBackConfig = configs
                        .Select(item => new CommonBusinessApiCallBackConfigItem
                        {
                            ApiName = item["apiName"]?.ToString(),
                            ApiConfigType = item["apiConfigType"]?.ToString().IsNullOrWhiteSpace() == true
                                ? "VA"
                                : item["apiConfigType"]?.ToString(),
                            ApiPath = item["apiPath"]?.ToString(),
                            ApiConfigValue = (item["apiConfigValue"] as JArray ?? new JArray())
                                .Select(v => new ApiConfigValueItem
                                {
                                    PowerType = v["PowerType"]?.ToString(),
                                    // 字典里 Functions 是数字数组，这里统一转成字符串数组（SunPay 侧按字符串存储）
                                    Functions = (v["Functions"] as JArray ?? new JArray())
                                        .Select(x => x?.ToString() ?? string.Empty)
                                        .Where(x => !x.IsNullOrWhiteSpace())
                                        .ToList()
                                }).ToList()
                        })
                        .ToList()
                };

                // 如果存在相对路径（以 / 开头），则必须有 domain
                var hasRelativePath = req.CommonBusinessApiCallBackConfig.Any(x => (x.ApiPath ?? string.Empty).StartsWith("/"));
                if (hasRelativePath && req.CallbackBaseUrl.IsNullOrWhiteSpace())
                {
                    Logger.Warn("alliancesApiCallBack.domain is empty but apiPath has relative path, skip init callback config.");
                    return;
                }

                req.MerchantKey = merchant.BusinessID;
                req.MerchantSecret = merchant.Key;

                var resp = await _merchantPayClient.UpsertBusinessApiCallBackConfigAsync(req);
                if (!resp.IsSuccess)
                {
                    Logger.Warn($"Init SunPay VA callback config failed. Code={resp.Code}, Msg={resp.Msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Init SunPay VA callback config exception.", ex);
            }
        }

        /// <summary>
        /// 设置商户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task SettingMerchantAsync(SettingMerchantDto input)
        {
            var entity = await _merchantRepository.FirstOrDefaultAsync(n => n.Id == input.Id);

            if (entity == null)
            {
                throw new Abp.UI.UserFriendlyException("非法请求");
            }

            entity.AuthType = input.AuthType;
            entity.AuthStandard = input.AuthStandard;

            await _merchantRepository.UpdateAsync(entity);
        }

        /// <summary>
        /// 获取单个商户（用于详情页等）
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList)]
        public async Task<MerchantDto> GetMerchantAsync(int id)
        {
            var entity = await _merchantRepository.GetAllIncluding(n => n.Alliance).FirstOrDefaultAsync(n => n.Id == id);
            if (entity == null)
                throw new Abp.UI.UserFriendlyException("商户不存在");
            return ObjectMapper.Map<MerchantDto>(entity);
        }

        /// <summary>
        /// 更新商户功能设置（PaySettings，枚举值逗号分隔）
        /// </summary>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnFunctionSettings)]
        public async Task UpdateMerchantPaySettingsAsync(UpdateMerchantPaySettingsDto input)
        {
            var entity = await _merchantRepository.FirstOrDefaultAsync(n => n.Id == input.Id);
            if (entity == null)
                throw new Abp.UI.UserFriendlyException("商户不存在");
            entity.PaySettings = input.PaySettings ?? string.Empty;
            await _merchantRepository.UpdateAsync(entity);
        }

        /// <summary>
        /// 获取商户下拉
        /// </summary>
        /// <param name="authType"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<List<MerchantDto>> GetMerchantItmes() => await _merchantRepository.GetAll().Select(n => new MerchantDto { Id = n.Id, Name = n.Name }).ToListAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task CreateMerchantAdvSetting(List<MerchantChannelCurrencyCreateDto> input)
        {
            var merchantSeeting = await _merchantChannelCurrencyReposiotory.GetAll().Where(n => n.MerchantId == input.First().MerchantId).ToListAsync();
            var merchant = await _merchantRepository.GetAsync(input.First().MerchantId);
            if (merchantSeeting.Any())
            {
                foreach (var item in merchantSeeting)
                {
                    _merchantChannelCurrencyReposiotory.HardDelete(item);
                }
            }

            foreach (var item in input)
            {
                var entity = new MerchantChannelCurrency
                {
                    MerchantId = item.MerchantId,
                    ChannelCode = item.ChannelCode,
                    Currency = item.Currency,
                    OpenClose = item.OpenClose
                };
                await _merchantChannelCurrencyReposiotory.InsertAsync(entity);
            }
            merchant.LastModificationTime = Clock.Now;
            await _merchantRepository.UpdateAsync(merchant);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        public async Task<List<MerchantChannelCurrencyDto>> GetMerchantAdvSetting(int merchantId)
        {
            var qry = await _merchantChannelCurrencyReposiotory.GetAll().Where(n => n.MerchantId == merchantId).ToListAsync();
            return ObjectMapper.Map<List<MerchantChannelCurrencyDto>>(qry);
        }

        /// <summary>
        /// 获取商户支付设置列表（支付设置详情页）
        /// Direct 来自 MerchantPaySetting，VA 来自 MerchantChannelCurrency
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnPaySettings)]
        public async Task<List<MerchantPaySettingItemDto>> GetMerchantPaySettingsAsync(int merchantId)
        {
            var result = new List<MerchantPaySettingItemDto>();

            // Direct 来自 MerchantPaySetting
            var directList = await _merchantPaySettingRepository.GetAll()
                .Where(n => n.MerchantId == merchantId && n.Type == MerchantPaySettingType.Direct)
                .OrderBy(n => n.CreationTime)
                .ToListAsync();
            foreach (var e in directList)
            {
                result.Add(new MerchantPaySettingItemDto
                {
                    Id = "MPS-" + e.Id,
                    Type = MerchantPaySettingType.Direct,
                    Currency = e.Currency,
                    PaymentMethod = e.PaymentMethod,
                    ChannelCode = e.ChannelCode,
                    Status = e.Status,
                    CreationTime = e.CreationTime,
                    LastModificationTime = e.LastModificationTime,
                    Operator = e.Operator
                });
            }

            // VA 来自 MerchantChannelCurrency，关联查出操作人（最后修改人优先，否则为创建人）
            var vaList = await _merchantChannelCurrencyReposiotory.GetAll()
                .Where(n => n.MerchantId == merchantId)
                .OrderBy(n => n.CreationTime)
                .ToListAsync();
            var vaUserIds = vaList
                .Select(x => x.CreatorUserId)
                .Union(vaList.Select(x => x.LastModifierUserId))
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();
            var vaUserDict = vaUserIds.Count > 0
                ? await _userRepository.GetAll()
                    .Where(u => vaUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.UserName)
                : new Dictionary<long, string>();
            foreach (var e in vaList)
            {
                var operatorName = e.LastModifierUserId.HasValue && vaUserDict.TryGetValue(e.LastModifierUserId.Value, out var modifier)
                    ? modifier
                    : (e.CreatorUserId.HasValue && vaUserDict.TryGetValue(e.CreatorUserId.Value, out var creator) ? creator : null);
                result.Add(new MerchantPaySettingItemDto
                {
                    Id = "MCC-" + e.Id,
                    Type = MerchantPaySettingType.VA,
                    Currency = e.Currency,
                    PaymentMethod = null,
                    ChannelCode = e.ChannelCode,
                    Status = e.OpenClose,
                    CreationTime = e.CreationTime,
                    LastModificationTime = e.LastModificationTime,
                    Operator = operatorName
                });
            }

            return result.OrderBy(x => x.Type).ThenBy(x => x.CreationTime).ToList();
        }

        /// <summary>
        /// 配置商户 VA 回调（同名更新）
        /// 转发到 SunPay：POST /api/v3/VA/CallbackConfig
        /// </summary>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnPaySettings)]
        public async Task<PayApiResponse<object>> UpsertVaCallbackConfigAsync(UpsertVaCallbackConfigDto input)
        {
            // 1. 校验商户
            var merchant = await _merchantRepository.FirstOrDefaultAsync(x => x.Id == input.MerchantId);
            if (merchant == null)
            {
                throw new Abp.UI.UserFriendlyException("商户不存在");
            }

            // 2. 基础校验：如果存在相对路径，则必须传 callbackBaseUrl
            var hasRelativePath = input.CommonBusinessApiCallBackConfig != null
                                  && input.CommonBusinessApiCallBackConfig.Any(x => (x.ApiPath ?? string.Empty).StartsWith("/"));
            if (hasRelativePath && input.CallbackBaseUrl.IsNullOrWhiteSpace())
            {
                throw new Abp.UI.UserFriendlyException("callbackBaseUrl 不能为空");
            }

            // 3. 构建请求并调用 Pay API
            var req = new UpsertBusinessApiCallBackConfigInput
            {
                CallbackBaseUrl = input.CallbackBaseUrl ?? string.Empty,
                CommonBusinessApiCallBackConfig = (input.CommonBusinessApiCallBackConfig ?? new List<CommonBusinessApiCallBackConfigDto>())
                    .Select(x => new CommonBusinessApiCallBackConfigItem
                    {
                        ApiName = x.ApiName,
                        ApiConfigType = x.ApiConfigType.IsNullOrWhiteSpace() ? "VA" : x.ApiConfigType,
                        ApiPath = x.ApiPath,
                        ApiConfigValue = (x.ApiConfigValue ?? new List<ApiConfigValueDto>())
                            .Select(v => new ApiConfigValueItem
                            {
                                PowerType = v.PowerType,
                                Functions = v.Functions ?? new List<string>()
                            }).ToList()
                    }).ToList()
            };

            // SunPay 使用 BusinessID / Key 作为 MerchantKey / MerchantSecret
            req.MerchantKey = merchant.BusinessID;
            req.MerchantSecret = merchant.Key;

            return await _merchantPayClient.UpsertBusinessApiCallBackConfigAsync(req);
        }

        /// <summary>
        /// 新增或修改商户支付设置
        /// Direct 写入 MerchantPaySetting，VA 写入 MerchantChannelCurrency
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnPaySettings)]
        public async Task<MerchantPaySettingItemDto> CreateOrUpdateMerchantPaySettingAsync(CreateOrUpdateMerchantPaySettingDto input)
        {
            var merchant = await _merchantRepository.FirstOrDefaultAsync(input.MerchantId);
            if (merchant == null)
                throw new Abp.UI.UserFriendlyException("商户不存在");

            var currentUser = await GetCurrentUserAsync();
            var operatorName = currentUser?.UserName ?? string.Empty;

            if (input.Type == MerchantPaySettingType.VA)
            {
                // VA 数据存入 MerchantChannelCurrency
                int mccIdVal = 0;
                var isUpdate = !string.IsNullOrWhiteSpace(input.Id) && input.Id.StartsWith("MCC-") && int.TryParse(input.Id.Substring(4), out mccIdVal);

                if (isUpdate)
                {
                    var entity = await _merchantChannelCurrencyReposiotory.FirstOrDefaultAsync(mccIdVal);
                    if (entity == null || entity.MerchantId != input.MerchantId)
                        throw new Abp.UI.UserFriendlyException("支付设置不存在或无权操作");
                    entity.Currency = input.Currency;
                    entity.ChannelCode = input.ChannelCode;
                    entity.OpenClose = input.Status;
                    await _merchantChannelCurrencyReposiotory.UpdateAsync(entity);
                    merchant.LastModificationTime = Clock.Now;
                    await _merchantRepository.UpdateAsync(merchant);
                    return new MerchantPaySettingItemDto
                    {
                        Id = "MCC-" + entity.Id,
                        Type = MerchantPaySettingType.VA,
                        Currency = entity.Currency,
                        ChannelCode = entity.ChannelCode,
                        Status = entity.OpenClose,
                        CreationTime = entity.CreationTime,
                        LastModificationTime = entity.LastModificationTime,
                        Operator = null
                    };
                }
                else
                {
                    var exists = await _merchantChannelCurrencyReposiotory.GetAll()
                        .AnyAsync(x => x.MerchantId == input.MerchantId && x.Currency == input.Currency && x.ChannelCode == input.ChannelCode);
                    if (exists)
                        throw new Abp.UI.UserFriendlyException("该商户下已存在相同的币种与渠道组合，请勿重复添加");

                    var entity = new MerchantChannelCurrency
                    {
                        MerchantId = input.MerchantId,
                        Currency = input.Currency,
                        ChannelCode = input.ChannelCode,
                        OpenClose = input.Status
                    };
                    await _merchantChannelCurrencyReposiotory.InsertAsync(entity);
                    merchant.LastModificationTime = Clock.Now;
                    await _merchantRepository.UpdateAsync(merchant);
                    return new MerchantPaySettingItemDto
                    {
                        Id = "MCC-" + entity.Id,
                        Type = MerchantPaySettingType.VA,
                        Currency = entity.Currency,
                        ChannelCode = entity.ChannelCode,
                        Status = entity.OpenClose,
                        CreationTime = entity.CreationTime,
                        LastModificationTime = entity.LastModificationTime,
                        Operator = null
                    };
                }
            }
            else
            {
                // Direct 数据存入 MerchantPaySetting
                Guid mpsIdVal = default;
                var isUpdate = !string.IsNullOrWhiteSpace(input.Id) && input.Id.StartsWith("MPS-") && Guid.TryParse(input.Id.Substring(4), out mpsIdVal);

                if (isUpdate)
                {
                    var entity = await _merchantPaySettingRepository.FirstOrDefaultAsync(mpsIdVal);
                    if (entity == null || entity.MerchantId != input.MerchantId)
                        throw new Abp.UI.UserFriendlyException("支付设置不存在或无权操作");
                    entity.Currency = input.Currency;
                    entity.PaymentMethod = input.PaymentMethod;
                    entity.ChannelCode = input.ChannelCode;
                    entity.Status = input.Status;
                    entity.Operator = operatorName;
                    await _merchantPaySettingRepository.UpdateAsync(entity);
                    return new MerchantPaySettingItemDto
                    {
                        Id = "MPS-" + entity.Id,
                        Type = MerchantPaySettingType.Direct,
                        Currency = entity.Currency,
                        PaymentMethod = entity.PaymentMethod,
                        ChannelCode = entity.ChannelCode,
                        Status = entity.Status,
                        CreationTime = entity.CreationTime,
                        LastModificationTime = entity.LastModificationTime,
                        Operator = entity.Operator
                    };
                }
                else
                {
                    var exists = await _merchantPaySettingRepository.GetAll()
                        .AnyAsync(x => x.MerchantId == input.MerchantId && x.Type == MerchantPaySettingType.Direct && x.Currency == input.Currency && x.PaymentMethod == input.PaymentMethod);
                    if (exists)
                        throw new Abp.UI.UserFriendlyException("该商户下已存在相同的币种与支付方式组合，请勿重复添加");

                    var entity = new MerchantPaySetting
                    {
                        Id = Guid.NewGuid(),
                        MerchantId = input.MerchantId,
                        MerchantName = merchant.Name,
                        Type = MerchantPaySettingType.Direct,
                        Currency = input.Currency,
                        PaymentMethod = input.PaymentMethod,
                        ChannelCode = input.ChannelCode,
                        Status = input.Status,
                        Operator = operatorName
                    };
                    await _merchantPaySettingRepository.InsertAsync(entity);
                    return new MerchantPaySettingItemDto
                    {
                        Id = "MPS-" + entity.Id,
                        Type = MerchantPaySettingType.Direct,
                        Currency = entity.Currency,
                        PaymentMethod = entity.PaymentMethod,
                        ChannelCode = entity.ChannelCode,
                        Status = entity.Status,
                        CreationTime = entity.CreationTime,
                        LastModificationTime = entity.LastModificationTime,
                        Operator = entity.Operator
                    };
                }
            }
        }

        /// <summary>
        /// 仅更新支付设置状态（开关），Id 格式：MPS-{guid} 或 MCC-{int}
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnPaySettings)]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.HttpPut]
        public async Task UpdateMerchantPaySettingStatusAsync(UpdateMerchantPaySettingStatusDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Id))
                throw new Abp.UI.UserFriendlyException("参数错误");
            if (input.Id.StartsWith("MCC-") && int.TryParse(input.Id.Substring(4), out var mccId))
            {
                var entity = await _merchantChannelCurrencyReposiotory.FirstOrDefaultAsync(mccId);
                if (entity == null || entity.MerchantId != input.MerchantId)
                    throw new Abp.UI.UserFriendlyException("支付设置不存在或无权操作");
                entity.OpenClose = input.Status;
                await _merchantChannelCurrencyReposiotory.UpdateAsync(entity);
            }
            else if (input.Id.StartsWith("MPS-") && Guid.TryParse(input.Id.Substring(4), out var mpsId))
            {
                var entity = await _merchantPaySettingRepository.FirstOrDefaultAsync(mpsId);
                if (entity == null || entity.MerchantId != input.MerchantId)
                    throw new Abp.UI.UserFriendlyException("支付设置不存在或无权操作");
                entity.Status = input.Status;
                await _merchantPaySettingRepository.UpdateAsync(entity);
            }
            else
            {
                throw new Abp.UI.UserFriendlyException("参数格式错误");
            }
        }

        /// <summary>
        /// 删除商户（同时删除该商户下的支付设置、渠道币种等关联数据）
        /// </summary>
        /// <param name="id">商户主键</param>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnDelete)]
        public async Task DeleteMerchantAsync(int id)
        {
            var entity = await _merchantRepository.FirstOrDefaultAsync(id);
            if (entity == null)
                throw new Abp.UI.UserFriendlyException("商户不存在");

            var mccList = await _merchantChannelCurrencyReposiotory.GetAll().Where(x => x.MerchantId == id).ToListAsync();
            foreach (var mcc in mccList)
                await _merchantChannelCurrencyReposiotory.DeleteAsync(mcc);

            var mpsList = await _merchantPaySettingRepository.GetAll().Where(x => x.MerchantId == id).ToListAsync();
            foreach (var mps in mpsList)
                await _merchantPaySettingRepository.DeleteAsync(mps);

            await _merchantRepository.DeleteAsync(entity);
        }

        /// <summary>
        /// 删除商户支付设置
        /// Id 格式：MPS-{guid} 删除 Direct；MCC-{int} 删除 VA
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnPaySettings)]
        public async Task DeleteMerchantPaySettingAsync(string id, int merchantId)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Abp.UI.UserFriendlyException("参数错误");
            if (id.StartsWith("MCC-") && int.TryParse(id.Substring(4), out var mccId))
            {
                var entity = await _merchantChannelCurrencyReposiotory.FirstOrDefaultAsync(mccId);
                if (entity == null || entity.MerchantId != merchantId)
                    throw new Abp.UI.UserFriendlyException("支付设置不存在或无权操作");
                await _merchantChannelCurrencyReposiotory.DeleteAsync(entity);
            }
            else if (id.StartsWith("MPS-") && Guid.TryParse(id.Substring(4), out var mpsId))
            {
                var entity = await _merchantPaySettingRepository.FirstOrDefaultAsync(mpsId);
                if (entity == null || entity.MerchantId != merchantId)
                    throw new Abp.UI.UserFriendlyException("支付设置不存在或无权操作");
                await _merchantPaySettingRepository.DeleteAsync(entity);
            }
            else
            {
                throw new Abp.UI.UserFriendlyException("参数格式错误");
            }
        }


        public async Task<PagedResultDto<MerchantSubCodeDto>> GetMerchantSubCodeListAsync(SearchMerchantSubCodeDto input)
        {
            var qry = _merchantSubCodeRepository.GetAllIncluding(n => n.Merchant).
            Where(n => n.MerchantId == input.MerchantId)
            .WhereIf(!input.SubCode.IsNullOrWhiteSpace(), n => n.SubCode.Contains(input.SubCode))
            .WhereIf(!input.SubDescription.IsNullOrWhiteSpace(), n => n.SubDescription.Contains(input.SubDescription))
            .WhereIf(input.CreationTimeStart.HasValue, n => n.CreationTime >= input.CreationTimeStart.Value)
            .WhereIf(input.CreationTimeEnd.HasValue, n => n.CreationTime <= input.CreationTimeEnd.Value)
            .WhereIf(input.LastModificationTimeStart.HasValue, n => n.LastModificationTime >= input.LastModificationTimeStart.Value)
            .WhereIf(input.LastModificationTimeEnd.HasValue, n => n.LastModificationTime <= input.LastModificationTimeEnd.Value)
            .OrderBy(input.Sorting);
            var count = await qry.CountAsync();
            var pagedResult = await qry.PageBy(input).ToListAsync();
            var dtos = ObjectMapper.Map<MerchantSubCodeDto[]>(pagedResult);
            return new PagedResultDto<MerchantSubCodeDto>(count, dtos);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnSubCodeSettings)]
        public async Task CreateMerchantSubCodeAsync(CreateMerchantSubCodeDto input)
        {
            MerchantSubCode entity = new MerchantSubCode
            {
                MerchantId = input.MerchantId,
                SubDescription = input.SubDescription
            };

            var subCode = SerialNumberHelper.GenerateSerial(6);
            while (true)
            {
                var isExists = await _merchantSubCodeRepository.GetAll().AnyAsync(n => n.SubCode == subCode);
                var mainIsExists = await _merchantRepository.GetAll().AnyAsync(n => n.RelationCode == subCode);
                if (!isExists && !mainIsExists)
                {
                    break;
                }
                else
                {
                    subCode = SerialNumberHelper.GenerateSerial(6);
                }
            }
            entity.SubCode = subCode;
            await _merchantSubCodeRepository.InsertAsync(entity);
        }

        [AbpAuthorize(PermissionNames.Pages_AllianceManagement_MerchantList_BtnSubCodeSettings)]
        public async Task UpdateMerchantSubCodeAsync(UpdateMerchantSubCodeDto input)
        { 

            var entity = await _merchantSubCodeRepository.FirstOrDefaultAsync(input.Id);
            if (entity == null)
                throw new Abp.UI.UserFriendlyException("子码不存在");
            entity.SubDescription = input.SubDescription;
            await _merchantSubCodeRepository.UpdateAsync(entity);
        }
    }
}
