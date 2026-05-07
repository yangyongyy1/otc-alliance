using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Web;

namespace ClientPlatform
{
    /// <summary>
    /// 
    /// </summary>
    [DisableAuditing]
    public class BasicAppService:AppServiceBase
    {
        private readonly ServiceMinIo _minio;
        private readonly IConfiguration _configuration;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<DataDictionary, int> _dataDictionaryRepository;

        /// <summary>
        ///
        /// </summary>
        public BasicAppService(ServiceMinIo minio, IConfiguration configuration, ICacheManager cacheManager, IRepository<DataDictionary, int> dataDictionaryRepository)
        {
            _minio = minio;
            _configuration = configuration;
            _cacheManager = cacheManager;
            _dataDictionaryRepository = dataDictionaryRepository;
        }

        /// <summary>
        /// 文件上传，并返回http地址
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public async Task<string> UploadFile(IFormFile File)
        {
            Stream stream = File.OpenReadStream();
            KeyValuePair<string, string> objectName = this._minio.GetNewObjectName(Path.GetExtension(File.FileName), 3, true);
            await this._minio.UploadFile(stream, this._minio.defalut_bucketName, objectName.Key);
            var endpoint = _configuration.GetSection("MinIO:Endpoint").Value;
            var http = endpoint.StartsWith("192") ? "http://" : "https://";
            return $"{http}{endpoint}" + objectName.Value;
        }

        [AbpAllowAnonymous]
        public async Task<string> UploadFileForWeb()
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync("https://prefile.klicklpay.com/klicklpay/standardbusiness/8f2e79240a5b4402bca60a181b1f3804/e0fb69e024594d11a170e75287033699.png");

            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var imageStream = await response.Content.ReadAsStreamAsync();

            //KeyValuePair<string, string> objectName = this._minio.GetNewObjectName(Path.GetExtension(File.FileName), 3, true);
            await this._minio.UploadFile(imageStream, this._minio.defalut_bucketName, Guid.NewGuid().ToString());
            var endpoint = _configuration.GetSection("MinIO:Endpoint").Value;
            var http = endpoint.StartsWith("192") ? "http://" : "https://";
            return $"{http}{endpoint}" + ".png";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<List<EnumDto>> GetEnum(string enumName)
        {
            var cache = _cacheManager.GetCache<string, List<EnumDto>>(ClientPlatformConsts.CacheNamespace);
            var cacheKey = $"enum:{enumName}";

            // 尝试从缓存读取
            var cached = await cache.GetOrDefaultAsync(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // 缓存不存在 → 创建数据
            var enumType = Type.GetType($"ClientPlatform.{enumName}, ClientPlatform.Core");
            if (enumType == null || !enumType.IsEnum)
                throw new ArgumentException($"未找到枚举类型: {enumName}");

            var names = Enum.GetNames(enumType);
            var localization = Abp.Localization.LocalizationHelper.GetSource(ClientPlatformConsts.LocalizationSourceName);

            var list = names
                .Select(name =>
                {
                    var fullKey = $"{enumType.Name}:{name}";
                    var localized = localization.GetStringOrNull(fullKey) ?? name;
                    var intValue = (int)Enum.Parse(enumType, name);

                    return new EnumDto
                    {
                        Key = name,
                        Value = intValue,
                        DisplayName = localized
                    };
                })
                .ToList();

            // 写入缓存 + 设置 30 分钟过期
            await cache.SetAsync(
                cacheKey,
                list,
                absoluteExpireTime: DateTimeOffset.Now.AddMinutes(30)
            );

            return list;
        }

        /// <summary>
        /// 获取数据字典中的币种列表（DicKey=Currencies，DicValue 为 JSON 数组如 ["EUR","USD","CNY"]）
        /// </summary>
        [AllowAnonymous]
        public async Task<List<string>> GetCurrenciesAsync()
        {
            return await GetDataDictionaryStringListAsync("Currencies");
        }

        /// <summary>
        /// 获取数据字典中的渠道列表（DicKey=Channels）
        /// </summary>
        [AllowAnonymous]
        public async Task<List<string>> GetChannelsAsync()
        {
            return await GetDataDictionaryStringListAsync("Channels");
        }

        /// <summary>
        /// 获取数据字典中的支付方式列表（DicKey=PaymentMethods）。返回显示名与平台名，兼容旧格式纯字符串数组。
        /// </summary>
        [AllowAnonymous]
        public async Task<List<PaymentMethodItemDto>> GetPaymentMethodsAsync()
        {
            var entity = await _dataDictionaryRepository.FirstOrDefaultAsync(n => n.DicKey == "PaymentMethods");
            if (entity == null || string.IsNullOrWhiteSpace(entity.DicValue))
                return new List<PaymentMethodItemDto>();
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var asObjects = JsonSerializer.Deserialize<List<PaymentMethodItemDto>>(entity.DicValue, options);
                if (asObjects != null && asObjects.Count > 0)
                    return asObjects;
                var asStrings = JsonSerializer.Deserialize<List<string>>(entity.DicValue);
                if (asStrings == null) return new List<PaymentMethodItemDto>();
                return asStrings.Select(s => new PaymentMethodItemDto { DisplayName = s ?? "", PlatformName = s ?? "" }).ToList();
            }
            catch
            {
                return new List<PaymentMethodItemDto>();
            }
        }

        private async Task<List<string>> GetDataDictionaryStringListAsync(string dicKey)
        {
            var entity = await _dataDictionaryRepository.FirstOrDefaultAsync(n => n.DicKey == dicKey);
            if (entity == null || string.IsNullOrWhiteSpace(entity.DicValue))
                return new List<string>();
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(entity.DicValue);
                return list ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
