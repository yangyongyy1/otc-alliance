using Abp.Auditing;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Web;

namespace ClientPlatformUser
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
        private readonly IRepository<Alliance, int> _allianceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="minio"></param>
        /// <param name="configuration"></param>
        /// <param name="cacheManager"></param>
        public  BasicAppService(ServiceMinIo minio,IConfiguration configuration, ICacheManager cacheManager,
            IRepository<Alliance, int> allianceRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger logger

            )
        {
            _minio = minio;
            _configuration = configuration;
            _cacheManager = cacheManager;
            _allianceRepository = allianceRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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


        [AllowAnonymous]
        public async Task<Alliance> GetWebSiteConfig()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            
            // 获取客户端域名，优先使用 X-Forwarded-Host（经过代理时），否则使用 Host
            var host = request.Headers["Origin"].FirstOrDefault() ?? request.Host.ToString();
            
            _logger.Info($"GetWebSiteConfig baseUrl:{host}");

            var alliance = await _allianceRepository.FirstOrDefaultAsync(n => n.WebSiteUrl.TrimEnd('/') == host);

            return alliance;
        }





    }
}
