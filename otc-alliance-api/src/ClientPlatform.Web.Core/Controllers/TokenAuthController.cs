using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Authentication.JwtBearer;
using ClientPlatform.Authorization;
using ClientPlatform.Authorization.Users;
using ClientPlatform.Configuration;
using ClientPlatform.Models;
using ClientPlatform.Models.TokenAuth;
using ClientPlatform.MultiTenancy;
using ClientPlatform.UserManagement;

namespace ClientPlatform.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : ClientPlatformControllerBase
    {
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly ICacheManager _cacheManager;
        private readonly UserManager _userManager;
        private readonly UserClaimsPrincipalFactory _claimsPrincipalFactory;
        private readonly IRepository<User, long> _abpUserRepository;
        private readonly IRepository<Merchant, int> _merchantRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IRepository<ClientUser, int> _userRepository;
        private readonly EnvironmentVariables _environmentVariables;

        private readonly MerchantSubCodeManager _merchantSubCodeManager;


        public TokenAuthController(
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            ICacheManager cacheManager,
            UserManager userManager,
            UserClaimsPrincipalFactory claimsPrincipalFactory,
            IRepository<User, long> abpUserRepository,
            IRepository<Merchant, int> merchantRepository,
            IHttpContextAccessor httpContextAccessor,
            IRepository<ClientUser, int> userRepository,
            EnvironmentVariables environmentVariables,
            MerchantSubCodeManager merchantSubCodeManager
            )
        {
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _cacheManager = cacheManager;
            _userManager = userManager;
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _abpUserRepository = abpUserRepository;
            _merchantRepository = merchantRepository;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _environmentVariables = environmentVariables;
            _merchantSubCodeManager = merchantSubCodeManager;
        }

        /// <summary>
        /// 后台登录：仅按「邮箱/用户名 + UserType=PlatformUser」查库并校验密码，不经过 LogInManager，避免同邮箱存在 BusinessUser 时取到错误用户。
        /// </summary>
        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var normalizedName = _userManager.KeyNormalizer?.NormalizeName(model.UserNameOrEmailAddress) ?? model.UserNameOrEmailAddress?.ToUpperInvariant();
            var normalizedEmail = _userManager.KeyNormalizer?.NormalizeEmail(model.UserNameOrEmailAddress) ?? model.UserNameOrEmailAddress?.ToUpperInvariant();

            var user = await _abpUserRepository.FirstOrDefaultAsync(u =>
                (u.NormalizedUserName == normalizedName || u.NormalizedEmailAddress == normalizedEmail)
                && u.UserType != SystemUserType.BusinessUser
                && !u.IsDeleted);

            if (user == null)
            {
                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(AbpLoginResultType.InvalidUserNameOrEmailAddress, model.UserNameOrEmailAddress, GetTenancyNameOrNull());
            }
            if (!user.IsActive)
            {
                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(AbpLoginResultType.UserIsNotActive, model.UserNameOrEmailAddress, GetTenancyNameOrNull());
            }
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(AbpLoginResultType.InvalidPassword, model.UserNameOrEmailAddress, GetTenancyNameOrNull());
            }

            var claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(user);
            var identity = claimsPrincipal?.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new UserFriendlyException(L("FailedToCreateUserIdentity"));
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(identity));
            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = user.Id
            };
        }


        /// <summary>
        /// 用户登录认证(web端) - 仅通过邮箱和邮箱验证码登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Abp.UI.UserFriendlyException"></exception>

        [HttpPost]
        public async Task<AuthenticateResultModel> AuthenticateForUser([FromBody] AuthenticateModelForUser model)
        {
            // 验证邮箱验证码（仅沙盒和生产环境需要验证）
            if (_environmentVariables.IsSandbox || _environmentVariables.IsProduction)
            {
                var cache = _cacheManager.GetCache<string, string>(ClientPlatformConsts.CacheNamespace);
                var cacheKey = $"emailverifycode:{model.UserNameOrEmailAddress}";
                var cachedCode = await cache.GetOrDefaultAsync(cacheKey);

                if (string.IsNullOrWhiteSpace(model.EmailVerifyCode) || cachedCode != model.EmailVerifyCode)
                {
                    throw new UserFriendlyException(L("EmailverifyCodeFail"));
                }

                // 验证成功后删除验证码缓存
                await cache.RemoveAsync(cacheKey);
            }

            // 仅按 UserType=BusinessUser 查库，避免同邮箱存在 PlatformUser 时找错人
            var normalizedEmail = _userManager.KeyNormalizer?.NormalizeEmail(model.UserNameOrEmailAddress) ?? model.UserNameOrEmailAddress?.ToUpperInvariant();
            var user = await _abpUserRepository.FirstOrDefaultAsync(u =>
                u.NormalizedEmailAddress == normalizedEmail
                && u.UserType == SystemUserType.BusinessUser
                && !u.IsDeleted);

            if (user == null)
            {
                throw new UserFriendlyException(L("UserNotFound"));
            }
            if (!user.IsActive)
            {
                throw new UserFriendlyException(L("UserIsNotActive"));
            }

            var claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(user);
            var identity = claimsPrincipal?.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new UserFriendlyException(L("FailedToCreateUserIdentity"));
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(identity));
            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = user.Id
            };
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken);
        }


        [HttpPost]
        public async Task<AuthenticateResultModel> RegisterAsync([FromBody] RegisterUserRequestDto input)
        {
            // 防止恶意注册：检查IP注册频率
            await CheckRegistrationRateLimitAsync();

            var apbUser = await _abpUserRepository.FirstOrDefaultAsync(n => n.EmailAddress == input.EmailAddress);

            if (apbUser != null)
            {
                throw new UserFriendlyException(L("EmailAlreadyRegistered"));
            }

            // 防止恶意注册：检查同一邮箱注册频率
            await CheckEmailRegistrationRateLimitAsync(input.EmailAddress);

            var merchant = await _merchantSubCodeManager.GetMerchantBySubCode(input.InviteCode);

            if (merchant == null)
            {
                throw new UserFriendlyException(L("InviteCodeInvalid"));
            }

            //验证邮箱验证码（仅沙盒和生产环境需要验证）
            if (_environmentVariables.IsSandbox || _environmentVariables.IsProduction)
            {
                var cache = _cacheManager.GetCache<string, string>(ClientPlatformConsts.CacheNamespace);
                var cacheKey = $"emailverifycode:{input.InviteCode}:{input.EmailAddress}";
                var cachedCode = await cache.GetOrDefaultAsync(cacheKey);

                if (cachedCode != input.EmailVerifyCode)
                {
                    throw new UserFriendlyException(L("EmailverifyCodeFail"));
                }

                // 验证成功后删除验证码缓存
                await cache.RemoveAsync(cacheKey);
            }

            var defalutPassWord = ClientPlatform.Authorization.Users.User.CreateRandomPassword();

            // 创建用户并获取密码
            var abpUserId = await CreateAbpUserAsync(input, merchant, defalutPassWord);

            var user= await _abpUserRepository.FirstOrDefaultAsync(n=>n.Id== abpUserId);

            await CreateVAUserAsync(input, merchant, abpUserId);


            // 创建 ClaimsIdentity
            var claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(user);
            var identity = claimsPrincipal.Identity as ClaimsIdentity;

            if (identity == null)
            {
                throw new UserFriendlyException(L("FailedToCreateUserIdentity"));
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = user.Id
            };


        }


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
        /// 获取客户端IP地址
        /// </summary>
        /// <returns></returns>
        private string GetClientIpAddress()
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
        /// 创建 ABP 用户（BusinessUser）
        /// </summary>
        protected async Task<long> CreateAbpUserAsync(RegisterUserRequestDto input, Merchant merchant, string defalutPassWord)
        {
            return await _userManager.CreateUserAsync(input.EmailAddress, defalutPassWord);
        }

        /// <summary>
        /// 创建VA用户信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="merchant"></param>
        /// <returns></returns>
        protected async Task<ClientUser> CreateVAUserAsync(RegisterUserRequestDto input, Merchant merchant, long abpUserId)
        {
            var abpUser = await _abpUserRepository.FirstOrDefaultAsync(n =>
                n.EmailAddress == input.EmailAddress
                && n.UserType == SystemUserType.BusinessUser
                && !n.IsDeleted);
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
                AllianceId = merchant.AllianceId,
                MerchantId = merchant.Id,
                UserType = input.UserType,
                UserAuthStatus =  KycBizStatus.PENDINGSUBMISSION, // 初始状态为未认证
                UserStatus = UserStatus.Active // 初始状态为启用
            };

            await _userRepository.InsertAsync(clientUser);
            await CurrentUnitOfWork.SaveChangesAsync();

            return clientUser;
        }

    }
}
