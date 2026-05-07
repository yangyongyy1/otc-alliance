using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.Runtime.Caching;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Result;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Authorization.Users;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Email;
using ClientPlatform.UserManagement;
using ClientPlatformUser.Users.Dto;

namespace ClientPlatformUser.Users
{
    /// <summary>
    /// 用户服务
    /// </summary>
    [AbpAllowAnonymous]
    public class ClientUserAppService : AppServiceBase
    {
        private IRepository<ClientUser, int> _userRepository;

        private IRepository<CountryInfo, int> _countryRepository;

        private readonly ILanguageManager _languageManager;

        private readonly IRepository<Merchant, int> _merchantRepository;

        private readonly IAllianceMailService _allianceMailService;
        private readonly IVerificationCodeMailEnqueuer _verificationCodeMailEnqueuer;

        private readonly ICacheManager _cacheManager;

        private readonly IRepository<User, long> _abpUserRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly UserManager _userManager;
        private readonly ILogger<ClientUserAppService> _logger;

        private readonly MerchantSubCodeManager _merchantSubCodeManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="countryRepository"></param>
        /// <param name="languageManager"></param>
        /// <param name="merchantRepository"></param>
        /// <param name="allianceMailService"></param>
        /// <param name="cacheManager"></param>
        /// <param name="abpUserRepository"></param>
        /// <param name="httpContextAccessor"></param>
        public ClientUserAppService(
            IRepository<ClientUser, int> userRepository,
            IRepository<CountryInfo, int> countryRepository,
            ILanguageManager languageManager,
            IRepository<Merchant, int> merchantRepository,
            IAllianceMailService allianceMailService,
            IVerificationCodeMailEnqueuer verificationCodeMailEnqueuer,
            ICacheManager cacheManager,
            IRepository<User, long> abpUserRepository,
            IHttpContextAccessor httpContextAccessor,
            UserManager userManager,
            ILogger<ClientUserAppService> logger,
            MerchantSubCodeManager merchantSubCodeManager
            )
        {
            _userRepository = userRepository;
            _logger = logger;
            _countryRepository = countryRepository;
            _languageManager = languageManager;
            _merchantRepository = merchantRepository;
            _allianceMailService = allianceMailService;
            _verificationCodeMailEnqueuer = verificationCodeMailEnqueuer;
            _cacheManager = cacheManager;
            _abpUserRepository = abpUserRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _merchantSubCodeManager = merchantSubCodeManager;
        }


        /// <summary>
        /// 获取国家地区下拉列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<CountryInfoSelectItemDto>> GetAllCountriesAsync()
        {
            var currentLangname = _languageManager.CurrentLanguage.Name;

            var countries = await _countryRepository.GetAll().OrderBy(n => n.Name).ToListAsync();

            List<CountryInfoSelectItemDto> result = new List<CountryInfoSelectItemDto>();

            foreach (var country in countries)
            {
                CountryInfoSelectItemDto item = new CountryInfoSelectItemDto();
                if (currentLangname == "zh-Hans")
                {
                    item.Name = country.CNName;
                    item.Code = country.CCA2;
                }
                else
                {
                    item.Name = country.Name;
                    item.Code = country.CCA2;
                }
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 获取登录的用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<ClientUserInfoDto> GetUserInfo()
        {
            var abpUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.Id == AbpSession.UserId.Value);
            if (abpUser == null)
            {
                throw new UserFriendlyException(L("AbpUserNotFound"));
            }
            var clientUser = await _userRepository.FirstOrDefaultAsync(n => n.AbpUserId == abpUser.Id);
            if (clientUser == null)
            {
                throw new UserFriendlyException(L("ClientUserNotFound"));
            }
            ClientUserInfoDto result = new ClientUserInfoDto
            {
                UserName = clientUser.UserName,
                Email = clientUser.Email,
                UserType = clientUser.UserType,
                CountryCode = clientUser.CountryCode,
                InviteCode = clientUser.InviteCode,
                UserAuthStatus = clientUser.UserAuthStatus,
                UserStatus = clientUser.UserStatus
            };
            return result;
        }



        /// <summary>
        /// 预检邮箱及验证码
        /// </summary>
        /// <param name="InviteCode"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task CheckEmailInviteCodeAsync(string InviteCode, string emailAddress)
        {
            var apbUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.EmailAddress == emailAddress);
            if (apbUser != null)
            {
                throw new UserFriendlyException(L("EmailAlreadyRegistered"));
            }
            var merchant = await _merchantSubCodeManager.GetMerchantBySubCode(InviteCode);
            if (merchant == null)
            {
                throw new UserFriendlyException(L("InviteCodeInvalid"));
            }
        }

        /// <summary>
        /// 预检登录邮箱
        /// </summary>
        /// <param name="userNameOrEmailAddress"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task CheckEmailForLoginAsync(string userNameOrEmailAddress)
        {
            var apbUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.EmailAddress == userNameOrEmailAddress);
            if (apbUser == null)
            {
                throw new UserFriendlyException(L("EmailNotExists"));
            }
            var clientUser = await _userRepository.FirstOrDefaultAsync(n => n.Email == userNameOrEmailAddress);
            if (clientUser == null)
            {
                throw new UserFriendlyException(L("EmailNotExists"));
            }
        }

        /// <summary>
        /// 发送注册验证码（带推荐人）
        /// </summary>
        /// <param name="InviteCode"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public async Task SendEmailVerifyCodeAsync(string InviteCode, string emailAddress)
        {
            _logger.LogInformation("SendEmailVerifyCodeAsync 被调用 - InviteCode: {InviteCode}, Email: {Email}", InviteCode, emailAddress);
            var apbUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.EmailAddress == emailAddress);

            if (apbUser != null)
            {
                throw new UserFriendlyException(L("EmailAlreadyRegistered"));
            }

            var clientUser = await _userRepository.FirstOrDefaultAsync(n => n.Email == emailAddress);
            if (clientUser!= null)
            {
                throw new UserFriendlyException(L("EmailNotExists"));
            }

            var merchant = await _merchantSubCodeManager.GetMerchantBySubCode(InviteCode);

            if (merchant == null)
            {
                throw new UserFriendlyException(L("InviteCodeInvalid"));
            }

            var verificationCode = new Random().Next(100000, 999999).ToString();

            var cache = _cacheManager.GetCache<string, string>(ClientPlatformConsts.CacheNamespace);
            var cacheKey = $"emailverifycode:{InviteCode}:{emailAddress}";
            await cache.SetAsync(cacheKey, verificationCode, absoluteExpireTime: DateTimeOffset.Now.AddMinutes(5));

            await _verificationCodeMailEnqueuer.EnqueueSendVerificationCodeAsync(merchant.Alliance.Name, emailAddress, verificationCode);
        }


        /// <summary>
        /// 发送登录邮箱验证码
        /// </summary>
        /// <param name="userNameOrEmailAddress">登录名称</param>
        /// <returns></returns>
        public async Task SendEmailVerifyCodeForLogin(string userNameOrEmailAddress)
        {
            _logger.LogInformation("SendEmailVerifyCodeForLogin 被调用 - Email: {Email}", userNameOrEmailAddress);
            var apbUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.EmailAddress == userNameOrEmailAddress);

            if (apbUser == null)
            {
                throw new UserFriendlyException(L("EmailNotExists"));
            }

            var clientUser = await _userRepository.GetAllIncluding(n => n.Merchant, n => n.Merchant.Alliance).Where(n => n.AbpUserId == apbUser.Id).FirstOrDefaultAsync();
            if (clientUser?.Merchant?.Alliance == null)
            {
                throw new UserFriendlyException(L("ClientUserNotFound"));
            }

            var verificationCode = new Random().Next(100000, 999999).ToString();

            var cache = _cacheManager.GetCache<string, string>(ClientPlatformConsts.CacheNamespace);
            var cacheKey = $"emailverifycode:{userNameOrEmailAddress}";
            await cache.SetAsync(cacheKey, verificationCode, absoluteExpireTime: DateTimeOffset.Now.AddMinutes(5));

            await _verificationCodeMailEnqueuer.EnqueueSendVerificationCodeAsync(clientUser.Merchant.Alliance.Name, userNameOrEmailAddress, verificationCode);
        }

        ///// <summary>
        ///// 用户注册
        ///// </summary>
        ///// <param name="input"></param>
        ///// <returns></returns>
        //public async Task<string> RegisterAsync(RegisterUserRequestDto input)
        //{
        //    // 防止恶意注册：检查IP注册频率
        //    await CheckRegistrationRateLimitAsync();

        //    var apbUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.EmailAddress == input.EmailAddress);

        //    if (apbUser != null)
        //    {
        //        throw new UserFriendlyException(L("EmailAlreadyRegistered"));
        //    }

        //    // 防止恶意注册：检查同一邮箱注册频率
        //    await CheckEmailRegistrationRateLimitAsync(input.EmailAddress);

        //    var merchant = await _merchantRepository.GetAllIncluding(n => n.Alliance).FirstOrDefaultAsync(n => n.RelationCode == input.InviteCode);

        //    if (merchant == null)
        //    {
        //        throw new UserFriendlyException("InviteCodeInvalid");
        //    }

        //    //验证邮箱验证码

        //    // var cache = _cacheManager.GetCache<string, string>(ClientPlatformConsts.CacheNamespace);

        //    // var cacheKey = $"emailverifycode:{input.InviteCode}:{input.EmailAddress}";

        //    // var cachedCode = await cache.GetOrDefaultAsync(cacheKey);

        //    //if (cachedCode!=input.EmailVerifyCode)
        //    //{
        //    //    throw new UserFriendlyException(L("EmailverifyCodeFail"));
        //    //}

        //    // 验证成功后删除验证码缓存
        //    // await cache.RemoveAsync(cacheKey);

        //    var defalutPassWord = User.CreateRandomPassword();

        //    // 创建用户并获取密码
        //    var abpUserId = await CreateAbpUserAsync(input, merchant, defalutPassWord);

        //    await CreateVAUserAsync(input, merchant, abpUserId);

        //    // 发送注册成功邮件
        //    try
        //    {
        //        // LoginUrl从MailConfig中获取，如果配置中没有则传空字符串，AllianceMailService会从配置中获取
        //        await _allianceMailService.SendRegistrationSuccessAsync(
        //            merchant.Alliance.Name,
        //            input.EmailAddress,
        //            input.EmailAddress,
        //            defalutPassWord,
        //            string.Empty // 传空字符串，让AllianceMailService从配置中获取
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        // 记录日志但不影响注册流程
        //        Logger?.Warn($"发送注册成功邮件失败 - 邮箱: {input.EmailAddress}, 错误: {ex.Message}", ex);
        //    }
        //    return defalutPassWord;
        //}

        /// <summary>
        /// 检查IP注册频率限制
        /// </summary>
        /// <returns></returns>
        private async Task CheckRegistrationRateLimitAsync()
        {
            var clientIp = GetClientIpAddress();
            if (string.IsNullOrEmpty(clientIp))
            {
                return;
            }

            var cache = _cacheManager.GetCache<string, int>(ClientPlatformConsts.CacheNamespace);
            var ipCacheKey = $"register:ip:{clientIp}";

            var registerCount = await cache.GetOrDefaultAsync(ipCacheKey);

            // 限制：同一IP在1小时内最多注册5次
            const int maxRegisterCount = 5;
            const int timeWindowMinutes = 60;

            if (registerCount >= maxRegisterCount)
            {
                //throw new UserFriendlyException(L("RegistrationTooFrequent"));
            }

            await cache.SetAsync(ipCacheKey, registerCount + 1, absoluteExpireTime: DateTimeOffset.Now.AddMinutes(timeWindowMinutes));
        }

        /// <summary>
        /// 检查邮箱注册频率限制
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        private async Task CheckEmailRegistrationRateLimitAsync(string emailAddress)
        {
            var cache = _cacheManager.GetCache<string, int>(ClientPlatformConsts.CacheNamespace);
            var emailCacheKey = $"register:email:{emailAddress}";

            var registerCount = await cache.GetOrDefaultAsync(emailCacheKey);

            // 限制：同一邮箱在24小时内最多注册3次
            const int maxRegisterCount = 3;
            const int timeWindowMinutes = 1440; // 24小时

            if (registerCount >= maxRegisterCount)
            {
                throw new UserFriendlyException(L("EmailRegistrationTooFrequent"));
            }

            await cache.SetAsync(emailCacheKey, registerCount + 1, absoluteExpireTime: DateTimeOffset.Now.AddMinutes(timeWindowMinutes));
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns></returns>
        private string? GetClientIpAddress()
        {
            if (_httpContextAccessor?.HttpContext == null)
            {
                return null;
            }

            var httpContext = _httpContextAccessor.HttpContext;

            // 优先从X-Forwarded-For头获取（适用于反向代理）
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            // 从X-Real-IP头获取
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // 最后从Connection.RemoteIpAddress获取
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// 创建登录用户信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="merchant"></param>
        /// <returns>返回生成的密码</returns>
        protected async Task<long> CreateAbpUserAsync(RegisterUserRequestDto input, Merchant merchant, string defalutPassWord)
        {
            var result = await _userManager.CreateUserAsync(input.EmailAddress, defalutPassWord);

            return result;
        }

        /// <summary>
        /// 创建VA用户信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="merchant"></param>
        /// <returns></returns>
        protected async Task CreateVAUserAsync(RegisterUserRequestDto input, Merchant merchant, long abpUserId)
        {
            // 获取刚创建的ABP用户
            var abpUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.EmailAddress == input.EmailAddress);
            if (abpUser == null)
            {
                throw new UserFriendlyException(L("AbpUserNotFound"));
            }

            // 创建VA用户
            var clientUser = new ClientUser
            {
                AbpUserId = abpUserId,
                UserName = input.EmailAddress.Split('@')[0], // 使用邮箱前缀作为用户名
                Email = input.EmailAddress,
                InviteCode = input.InviteCode,
                CountryCode = input.CountryCode,
                AllianceId = merchant.AllianceId,
                MerchantId = merchant.Id,
                UserType = input.UserType,
                UserAuthStatus =  KycBizStatus.PENDINGSUBMISSION, // 初始状态为未认证
                UserStatus = UserStatus.Active // 初始状态为启用
            };

            await _userRepository.InsertAsync(clientUser);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 检查Identity结果错误
        /// </summary>
        /// <param name="identityResult"></param>
        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }


        /// <summary>
        /// 获取用户所在地区支持的币种
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetUserCountryCurrencies()
        {
            var currenUser = await _userRepository.FirstOrDefaultAsync(n => n.AbpUserId == AbpSession.UserId.Value);
            if (currenUser == null)
            {
                throw new UserFriendlyException(L("ClientUserNotFound"));
            }
            var country = await _countryRepository.FirstOrDefaultAsync(n => n.CCA2 == currenUser.CountryCode);
            if (country == null)
            {
                throw new UserFriendlyException(L("CountryNotFound"));
            }
            var currencies = country.Currency.Split(',').ToList();
            return currencies;
        }





    }
}
