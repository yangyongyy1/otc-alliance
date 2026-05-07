using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Events.Bus;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.Configuration;
using ClientPlatform.Authorization.Users;
using ClientPlatform.Extensions;
using ClientPlatform.Kyc.Channels;
using ClientPlatform.Kyc.Dto;
using ClientPlatform.Kyc.Events;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.Pay.Managers;
using ClientPlatform.UserManagement;

namespace ClientPlatform.Kyc
{
    /// <summary>
    /// KYC 领域服务管理器
    /// 负责协调 KYC 流程，包括链接生成、回调处理和实体状态更新
    /// </summary>
    public class KycManager : DomainService
    {
        private readonly IKycChannelFactory _kycChannelFactory;
        private readonly IRepository<KycApplicant, Guid> _kycApplicantRepository;
        private readonly IEventBus _eventBus;
        private readonly PayClient _payClient;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly EnvironmentVariables _environmentVariables;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="kycChannelFactory">KYC 渠道工厂</param>
        /// <param name="kycApplicantRepository">KYC 申请人仓储</param>
        public KycManager(
            IKycChannelFactory kycChannelFactory,
            IRepository<KycApplicant, Guid> kycApplicantRepository,
            IEventBus eventBus,
            PayClient payClient,
            IRepository<ClientUser, int> clientUserRepository,
            EnvironmentVariables environmentVariables)
        {
            _kycChannelFactory = kycChannelFactory;
            _kycApplicantRepository = kycApplicantRepository;
            _eventBus = eventBus;
            _payClient = payClient;
            _clientUserRepository = clientUserRepository;
            _environmentVariables = environmentVariables;
        }

        /// <summary>
        /// 根据当前运行环境获取认证等级在渠道侧的 LevelName
        /// 测试环境使用枚举上的 AmbientValue，生产环境使用业务约定字符串
        /// </summary>
        /// <param name="authLevel">认证标准等级</param>
        /// <returns>渠道侧实际使用的 LevelName</returns>
        public string GetAuthLevelName(AuthStandardLevel authLevel)
        {
            // 生产环境：按业务要求使用固定名称
            if (_environmentVariables.IsProduction)
            {
                return "Klicklpay KYC-New";
            }

            // 其它环境：使用枚举上的 AmbientValue（例如 Test-kyc&liveness）
            return authLevel.GetAmbientValue();
        }

        /// <summary>
        /// 生成 WebSDK 链接并创建初始 KYC 记录
        /// </summary>
        /// <param name="userId">系统用户 ID</param>
        /// <param name="authLevel">认证标准等级</param>
        /// <param name="tenantId">租户 ID</param>
        /// <returns>WebSDK URL</returns>
        public async Task<string> GenerateWebSdkLinkAsync(int userId, AuthStandardLevel authLevel, int? tenantId)
        {
            // 获取 Sumsub 渠道提供商
            var provider = _kycChannelFactory.GetProvider(KycChannelCodes.Sumsub);

            // 生成唯一的外部用户 ID (ExternalUserId)
            // 格式: USER_{UserId}_{Timestamp}，确保每次请求唯一，或根据业务需求固定
            var externalUserId = $"USER_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            // Or user provided logic: _numberGeneratorHelper.NumberGenerator(KycNumberPrefixConsts.KP);
            // I'll stick to a simple one or use NumberGenerator if available (need to verify if it is injected)

            // 根据环境获取渠道实际使用的 LevelName
            string sumsubLevelName = GetAuthLevelName(authLevel);

            // 构建请求参数
            var req = new GetWebSdkLinkRequest
            {
                UserId = externalUserId,
                LevelName = sumsubLevelName,
                TtlInSecs = 9999999 // 链接有效期 1 小时
            };

            // 调用提供商接口获取链接
            var resp = await provider.GetWebSdkLinkAsync(req);

            if (!resp.Success)
            {
                throw new UserFriendlyException($"生成 KYC 链接失败: {resp.Message}");
            }

            // 创建并保存 KYC 申请记录到数据库
            var applicant = new KycApplicant
            {
                // TenantId = tenantId, // Removed by user
                UserId = userId,
                KycProvider = KycChannelCodes.Sumsub,
                KycChannelProductTypes = KycChannelProductTypes.WebSDK,
                KycType = BusinessUserType.Individual, // 默认为个人，后续可扩展
                AuthStandardLevel = authLevel,
                KycVerificationLink = resp.Url,
                KycBizStatus = KycBizStatus.PENDINGSUBMISSION, // 初始状态：待提交
                ExternalUserId = externalUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _kycApplicantRepository.InsertAsync(applicant);

            return resp.Url;
        }


        /// <summary>
        /// 检查用户是否在Pay系统存在KYC记录 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="middleName"></param>
        /// <param name="authLevel">认证标准等级</param>
        /// <returns></returns>
        public async Task<KycApplicant> CheckIfPayHasKycRecord(string firstName,string lastName,string middleName, ClientUser clientUser, AuthStandardLevel authLevel)
        {
             var  userName = string.Join(" ", new[] { firstName, middleName, lastName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
            var merchantOption = new PayMerchantOption { MerchantKey = clientUser.Merchant.BusinessID, MerchantSecret = clientUser.Merchant.Key };
            var res = await _payClient.QueryCustomerKycInfoAsync(new Pay.Dto.Request.QueryCustomerKycInfoInput() { userName = userName }, merchantOption);
            if (res.IsSuccess)
            {
                if (res.Data)
                {
                    // 创建并保存 KYC 申请记录到数据库
                    var applicant = new KycApplicant
                    {
                        // TenantId = tenantId, // Removed by user
                        UserId = clientUser.Id,
                        KycProvider = KycChannelCodes.Sumsub,
                        KycChannelProductTypes = KycChannelProductTypes.Pay,
                        KycType = BusinessUserType.Individual, // 默认为个人，后续可扩展
                        AuthStandardLevel = authLevel,
                        KycVerificationLink = "",
                        KycBizStatus = KycBizStatus.APPROVED, // 初始状态：待提交
                        ExternalUserId = "",
                        CreatedAt = DateTime.UtcNow,
                        FirstName = firstName,
                        LastName = lastName,
                        MiddleName = middleName,
                        FixedFirstName = firstName, 
                        FixedLastName = lastName,
                        FixedMiddleName = middleName,
                        Email = clientUser.Email
                      
                    };

                    await _kycApplicantRepository.InsertAsync(applicant);

                    clientUser.FirstName = applicant.FixedFirstName;
                    clientUser.LastName = applicant.FixedLastName;
                    clientUser.MiddleName = applicant.FixedMiddleName;
                    clientUser.UserAuthStatus = applicant.KycBizStatus;
                    clientUser.KycLevelCompleted = ((int)applicant.AuthStandardLevel).ToString();
                    await _clientUserRepository.UpdateAsync(clientUser);


                    return applicant;
                }
            }
            return null;
        }

        public async Task<KycApplicant> CheckIfHasKycRecord(string firstName, string lastName, string middleName, ClientUser clientUser, AuthStandardLevel authLevel)
        {
            var fullName = $"{firstName} {middleName} {lastName}".Trim().Replace("  ", " ");

            var exist =  await _kycApplicantRepository.GetAll()
                .AnyAsync(x =>
                    (x.FixedFirstName + " " + x.FixedMiddleName + " " + x.FixedLastName)
                        .Replace("  ", " ")
                        .Trim() == fullName
                    && x.KycBizStatus == KycBizStatus.APPROVED
                );
            if (exist) 
            {
                // 创建并保存 KYC 申请记录到数据库
                var applicant = new KycApplicant
                {
                    // TenantId = tenantId, // Removed by user
                    UserId = clientUser.Id,
                    KycProvider = KycChannelCodes.Sumsub,
                    KycChannelProductTypes = KycChannelProductTypes.Pay,
                    KycType = BusinessUserType.Individual, // 默认为个人，后续可扩展
                    AuthStandardLevel = authLevel,
                    KycVerificationLink = "",
                    KycBizStatus = KycBizStatus.APPROVED, // 初始状态：待提交
                    ExternalUserId = "",
                    CreatedAt = DateTime.UtcNow,
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    FixedFirstName = firstName,
                    FixedLastName = lastName,
                    FixedMiddleName = middleName,
                    Email = clientUser.Email

                };

                await _kycApplicantRepository.InsertAsync(applicant);
                clientUser.FirstName = applicant.FixedFirstName;
                clientUser.LastName = applicant.FixedLastName;
                clientUser.MiddleName = applicant.FixedMiddleName;
                clientUser.UserAuthStatus = applicant.KycBizStatus;
                clientUser.KycLevelCompleted = ((int)applicant.AuthStandardLevel).ToString();
                await _clientUserRepository.UpdateAsync(clientUser);


                return applicant;
            }
            return null;
        }



        /// <summary>
        /// 处理来自渠道的回调通知
        /// </summary>
        /// <param name="channelName">渠道名称</param>
        /// <param name="requestBody">回调内容</param>
        /// <param name="signature">签名</param>
        /// <returns></returns>
        public async Task HandleCallbackAsync(string channelName, string requestBody, string signature)
        {
            // 通过工厂获取对应的提供商来处理回调具体的验证和解析逻辑
            var provider = _kycChannelFactory.GetProvider(channelName);
            var result = await provider.HandleCallbackAsync(requestBody, signature);
            Logger.Info($"HandleCallbackAsync====result==={JsonConvert.SerializeObject(result)}");

            //if (result.Success && result.IsApproved)
            //{
            //    Logger.Info($"HandleCallbackAsync====开始创建客户===UserId={result.UserId}===KycApplicantId={result.KycApplicantId}");

            //    try
            //    {
            //        Logger.Info($"HandleCallbackAsync====触发事件 KycVerificationCompletedEvent");
            //        // LEGACY: Commented out to prevent automatic customer creation. 
            //        // Customer creation is now handled asynchronously via PayAppService.CreateAccount flow.
            //        // await _eventBus.TriggerAsync(new KycVerificationCompletedEvent(result.UserId, result.KycApplicantId, result.IsApproved));
            //        Logger.Info($"HandleCallbackAsync====事件触发已跳过 (Legacy Flow Disabled)");
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Error($"HandleCallbackAsync====事件触发失败===Error={ex.Message}===StackTrace={ex.StackTrace}", ex);
            //        throw;
            //    }
            //}
            //else
            //{
            //    Logger.Info($"HandleCallbackAsync====不满足创建客户条件===Success={result.Success}===IsApproved={result.IsApproved}");
            //}
        }
    }
}
