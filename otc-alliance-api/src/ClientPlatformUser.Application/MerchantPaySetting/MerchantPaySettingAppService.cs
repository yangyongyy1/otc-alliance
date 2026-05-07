using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Settings;
using ClientPlatform.UserManagement;
using MerchantPaySettingEntity = ClientPlatform.Settings.MerchantPaySetting;
using ClientPlatformUser.MerchantPaySetting.Dto;

namespace ClientPlatformUser.MerchantPaySetting
{
    /// <summary>
    /// 商户支付设置（C 端登录用户：按当前用户所属商户获取支付配置）
    /// </summary>
    [AbpAuthorize]
    public class MerchantPaySettingAppService : AppServiceBase
    {
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<Merchant, int> _merchantRepository;
        private readonly IRepository<MerchantPaySettingEntity, Guid> _merchantPaySettingRepository;
        private readonly IRepository<MerchantChannelCurrency, int> _merchantChannelCurrencyRepository;
        private readonly IRepository<DataDictionary, int> _dataDictionaryRepository;

        public MerchantPaySettingAppService(
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<Merchant, int> merchantRepository,
            IRepository<MerchantPaySettingEntity, Guid> merchantPaySettingRepository,
            IRepository<MerchantChannelCurrency, int> merchantChannelCurrencyRepository,
            IRepository<DataDictionary, int> dataDictionaryRepository)
        {
            _clientUserRepository = clientUserRepository;
            _merchantRepository = merchantRepository;
            _merchantPaySettingRepository = merchantPaySettingRepository;
            _merchantChannelCurrencyRepository = merchantChannelCurrencyRepository;
            _dataDictionaryRepository = dataDictionaryRepository;
        }

        /// <summary>
        /// 获取当前登录用户所属商户的支付设置。返回 { DirectPay: [], : [] }，无配置时对应空数组。
        /// </summary>
        public async Task<GetMyMerchantPaySettingsResultDto> GetMyMerchantPaySettingsAsync()
        {
            var result = new GetMyMerchantPaySettingsResultDto();
            var abpUserId = AbpSession.UserId;
            if (!abpUserId.HasValue)
                throw new Abp.UI.UserFriendlyException("请先登录");

            var clientUser = await _clientUserRepository.FirstOrDefaultAsync(x => x.AbpUserId == abpUserId.Value);
            if (clientUser == null || !clientUser.MerchantId.HasValue)
                return result;

            var merchant = await _merchantRepository.FirstOrDefaultAsync(clientUser.MerchantId.Value);
            if (merchant == null || string.IsNullOrWhiteSpace(merchant.PaySettings))
                return result;

            var allowedTypes = ParsePaySettingsTypes(merchant.PaySettings);
            if (allowedTypes.Count == 0)
                return result;
            if (allowedTypes.Contains(MerchantPaySettingType.Direct))
            {
                // Direct 来自 MerchantPaySetting
                var directList = await _merchantPaySettingRepository.GetAll()
                    .Where(x => x.MerchantId == merchant.Id && x.Type == MerchantPaySettingType.Direct && x.Status == OpenClose.Open)
                    .OrderBy(x => x.CreationTime)
                    .ToListAsync();

                var paymentMethodMap = await GetPaymentMethodMapAsync();

                foreach (var entity in directList)
                    result.DirectPay.Add(MapToDirectPayItemDto(entity, paymentMethodMap));
            }
            else
            { 
                result.DirectPay = new List<DirectPaySettingItemDto>();
            }
            
            if(allowedTypes.Contains(MerchantPaySettingType.VA))
            {   
                var vaList = await _merchantChannelCurrencyRepository.GetAll()
                    .Where(x => x.MerchantId == merchant.Id && x.OpenClose == OpenClose.Open)
                    .OrderBy(x => x.CreationTime)
                    .ToListAsync();

                foreach (var entity in vaList)
                    result.Pay.Add(MapToItemDto(entity));
            }
            else
            {
                result.Pay = new List<PaySettingItemDto>();
            }
            return result;
        }

        private static List<MerchantPaySettingType> ParsePaySettingsTypes(string paySettings)
        {
            if (string.IsNullOrWhiteSpace(paySettings)) return new List<MerchantPaySettingType>();
            var parts = paySettings.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            var result = new List<MerchantPaySettingType>();
            foreach (var p in parts)
            {
                if (int.TryParse(p, out var num) && Enum.IsDefined(typeof(MerchantPaySettingType), num))
                {
                    result.Add((MerchantPaySettingType)num);
                    continue;
                }
                if (Enum.TryParse<MerchantPaySettingType>(p, true, out var byName))
                    result.Add(byName);
            }
            return result.Distinct().ToList();
        }

        private async Task<Dictionary<string, PaymentMethodMeta>> GetPaymentMethodMapAsync()
        {
            var map = new Dictionary<string, PaymentMethodMeta>(StringComparer.OrdinalIgnoreCase);

            var entity = await _dataDictionaryRepository.FirstOrDefaultAsync(x => x.DicKey == "PaymentMethods");
            if (entity == null || string.IsNullOrWhiteSpace(entity.DicValue))
                return map;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 优先尝试按对象数组解析：[{ displayName, platformName }, ...]
                var asObjects = JsonSerializer.Deserialize<List<PaymentMethodMeta>>(entity.DicValue, options);
                if (asObjects != null && asObjects.Count > 0)
                {
                    foreach (var item in asObjects)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) && string.IsNullOrWhiteSpace(item.PlatformName))
                            continue;

                        var platform = string.IsNullOrWhiteSpace(item.PlatformName) ? item.DisplayName : item.PlatformName;
                        var display = string.IsNullOrWhiteSpace(item.DisplayName) ? platform : item.DisplayName;

                        map[platform] = new PaymentMethodMeta
                        {
                            DisplayName = display,
                            PlatformName = platform
                        };
                    }

                    return map;
                }

                // 兼容旧格式：["SEPA", "SWIFT", ...]
                var asStrings = JsonSerializer.Deserialize<List<string>>(entity.DicValue, options);
                if (asStrings != null)
                {
                    foreach (var name in asStrings.Where(n => !string.IsNullOrWhiteSpace(n)))
                    {
                        map[name] = new PaymentMethodMeta
                        {
                            DisplayName = name,
                            PlatformName = name
                        };
                    }
                }
            }
            catch
            {
                // 忽略解析异常，返回当前已构建的 map（可能为空）
            }

            return map;
        }

        private static DirectPaySettingItemDto MapToDirectPayItemDto(
            MerchantPaySettingEntity entity,
            Dictionary<string, PaymentMethodMeta> paymentMethodMap)
        {
            PaymentMethodMeta meta = null;
            if (!string.IsNullOrWhiteSpace(entity.PaymentMethod) &&
                paymentMethodMap != null &&
                paymentMethodMap.TryGetValue(entity.PaymentMethod, out var found))
            {
                meta = found;
            }

            var platformName = meta?.PlatformName ?? entity.PaymentMethod ?? string.Empty;
            var displayName = meta?.DisplayName ?? platformName;

            return new DirectPaySettingItemDto
            {
                Currency = entity.Currency,
                PaymentMethod = platformName,
                DisplayName = displayName,
                PlatformName = platformName
            };
        }

        private static PaySettingItemDto MapToItemDto(MerchantChannelCurrency entity)
        {
            return new PaySettingItemDto
            {
                Currency = entity.Currency,
                ChannelCode = entity.ChannelCode
            };
        }

        private class PaymentMethodMeta
        {
            public string DisplayName { get; set; }
            public string PlatformName { get; set; }
        }
    }
}
