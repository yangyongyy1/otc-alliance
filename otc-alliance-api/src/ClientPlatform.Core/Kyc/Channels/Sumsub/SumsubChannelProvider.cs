using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using Abp.UI;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedLockNet;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ClientPlatform;
using ClientPlatform.Authorization.Users;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Extensions;
using ClientPlatform.Kyc.Dto;
using ClientPlatform.Kyc.Jobs;
using ClientPlatform.Pay.Jobs;
using ClientPlatform.UserManagement;
using ClientPlatform.Web;


namespace ClientPlatform.Kyc.Channels.Sumsub
{
    public class SumsubChannelProvider : IKycChannelProvider, ITransientDependency
    {
        public string ChannelName => KycChannelCodes.Sumsub;

        private readonly IConfiguration _configuration;
        private readonly SumsubSetting _apiSetting;
        private readonly ServiceMinIo _minio;
        private readonly IDistributedLockFactory _distributedLockFactory;
        private readonly IRepository<KycApplicant, Guid> _kycApplicantRepository;
        private readonly IRepository<KycApplicantDocument, Guid> _kycApplicantDocumentRepository;
        private readonly IRepository<KycApplicantBeneficiary, Guid> _kycApplicantBeneficiaryRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ILogger Logger { get; set; }

        private readonly IBackgroundJobManager _backgroundJobManager;
        // Rebus IBus 在部分 Host（例如 Admin/联盟管理后端）可能未注册；
        // 这里改为可选依赖，避免仅生成链接时也因 MQ 缺失而失败。
        private readonly Rebus.Bus.IBus? _bus;
        private readonly IRepository<ClientUser, int> _clientUserRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration">配置</param>
        /// <param name="minio">MinIO 服务</param>
        /// <param name="distributedLockFactory">分布式锁工厂</param>
        /// <param name="kycApplicantRepository">KYC 申请人仓储</param>
        /// <param name="kycApplicantDocumentRepository">KYC 文档仓储</param>
        /// <param name="kycApplicantBeneficiaryRepository">KYC 受益人仓储</param>
        /// <param name="unitOfWorkManager">工作单元管理器</param>
        /// <param name="clientUserRepository">业务用户仓储</param>
        /// <param name="backgroundJobManager">后台任务管理器</param>
        /// <param name="mqMessagePublisher">MQ 消息发布者</param>
        public SumsubChannelProvider(
            IConfiguration configuration,
            ServiceMinIo minio,
            IDistributedLockFactory distributedLockFactory,
            IRepository<KycApplicant, Guid> kycApplicantRepository,
            IRepository<KycApplicantDocument, Guid> kycApplicantDocumentRepository,
            IRepository<KycApplicantBeneficiary, Guid> kycApplicantBeneficiaryRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ClientUser, int> clientUserRepository,
            IBackgroundJobManager backgroundJobManager,
            //IMqMessagePublisher mqMessagePublisher,
            Rebus.Bus.IBus? bus = null)
        {
            _configuration = configuration;
            _apiSetting = _configuration.GetSection("SumsubConf").Get<SumsubSetting>();

            if (_apiSetting == null)
            {
                throw new UserFriendlyException("Sumsub configuration 'SumsubConf' is missing or invalid.");
            }

            _minio = minio;
            _distributedLockFactory = distributedLockFactory;
            _kycApplicantRepository = kycApplicantRepository;
            _kycApplicantDocumentRepository = kycApplicantDocumentRepository;
            _kycApplicantBeneficiaryRepository = kycApplicantBeneficiaryRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _clientUserRepository = clientUserRepository;
            _backgroundJobManager = backgroundJobManager;
            //_mqMessagePublisher = mqMessagePublisher;
            _bus = bus;

            Logger = NullLogger.Instance;
        }

        public async Task<CreateApplicantResponse> CreateApplicantAsync(CreateApplicantRequest request)
        {
            try
            {
                var body = new Dictionary<string, object>
                {
                    { "externalUserId", request.ExternalUserId },
                    { "email", request.Email },
                    { "phone", request.Phone },
                    { "type", request.ApplicantType }
                };

                var resp = await RestAction(
                    resource: "/resources/applicants?levelName=" + request.LevelName,
                    parameters: body,
                    method: Method.Post
                );

                if (resp.IsSuccessful)
                {
                    var content = resp.Content;
                    var json = JObject.Parse(content);
                    return new CreateApplicantResponse
                    {
                        Success = true,
                        ApplicantId = json["id"]?.ToString(),
                        Message = "申请人创建成功"
                    };
                }
                else
                {
                    return new CreateApplicantResponse
                    {
                        Success = false,
                        Message = resp.Content
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Sumsub 创建申请人失败", ex);
                return new CreateApplicantResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<GetWebSdkLinkResponse> GetWebSdkLinkAsync(GetWebSdkLinkRequest request)
        {
            try
            {
                var body = new Dictionary<string, object>
                {
                    ["levelName"] = request.LevelName,
                    ["userId"] = request.UserId
                };

                if (request.TtlInSecs > 0)
                {
                    body["ttlInSecs"] = request.TtlInSecs;
                }

                Logger.Info($"Sumsub===GenerateWebSdkLink==request=={JsonConvert.SerializeObject(request)}");

                var resp = await RestAction(
                    resource: "/resources/sdkIntegrations/levels/-/websdkLink",
                    parameters: body,
                    method: Method.Post
                );

                Logger.Info($"Sumsub===GenerateWebSdkLink==resp=={resp.Content}");

                if (resp.IsSuccessful)
                {
                    var json = JsonConvert.DeserializeObject<JObject>(resp.Content);
                    return new GetWebSdkLinkResponse
                    {
                        Success = true,
                        Url = json.Value<string>("url"),
                        Message = "Generate external WebSDK link success"
                    };
                }

                return new GetWebSdkLinkResponse
                {
                    Success = false,
                    Message = resp.Content
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"请求 Sumsub WebSDK 链接发生异常: {ex.Message}", ex);
                return new GetWebSdkLinkResponse { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// 回调处理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<KycCallbackResult> HandleApplicantReviewed(HandleApplicantReviewedRequest request)
        {
            Logger.Info($"Sumsub===HandleApplicantReviewed==request=={JsonConvert.SerializeObject(request)}");
            try
            {
                using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
                {
                    var expiry = TimeSpan.FromSeconds(60);
                    var resource = $"{request.CorrelationId}{request.LevelName}-HandleApplicantReviewed-locking-on";
                    using (var redLock = await _distributedLockFactory.CreateLockAsync(resource, expiry))
                    {
                        if (redLock.IsAcquired)
                        {
                            var kycApplicant = await _kycApplicantRepository.GetAll().Where(x => x.ExternalUserId == request.ExternalUserId).FirstOrDefaultAsync();
                            Logger.Info($"Sumsub===HandleApplicantReviewed==kycApplicant=={JsonConvert.SerializeObject(kycApplicant)}");
                            if (kycApplicant == null)
                            {
                                return new KycCallbackResult { Success = false, Message = "Applicant not found" };
                            }

                            Logger.Info(
                                $"Sumsub===HandleApplicantReviewed===事件对比===Type={request.Type}===ReviewStatus={request.ReviewStatus}===CreatedAt={request.CreatedAt}===CreatedAtMs={request.CreatedAtMs}===CurrentBizStatus={kycApplicant.KycBizStatus}===CurrentReviewStatus={kycApplicant.ReviewStatus}===CurrentReviewDate={kycApplicant.ReviewDate}");

                            var incomingCallbackTime = ParseCallbackEventTime(request);
                            var currentCallbackTime = GetCurrentCallbackEventTime(kycApplicant);
                            Logger.Info(
                                $"Sumsub===HandleApplicantReviewed===时间戳判断===Incoming={incomingCallbackTime:O}===Current={currentCallbackTime:O}===StoredLastCallbackEventTime={kycApplicant.LastCallbackEventTime:O}===Type={request.Type}");

                            // 防乱序：只按时间戳判断，旧事件直接忽略，避免状态被回写
                            if (incomingCallbackTime.HasValue
                                && currentCallbackTime.HasValue
                                && incomingCallbackTime.Value <= currentCallbackTime.Value)
                            {
                                Logger.Warn(
                                    $"Sumsub===HandleApplicantReviewed===忽略旧回调===Type={request.Type}===Incoming={incomingCallbackTime:O}===Current={currentCallbackTime:O}===CurrentBizStatus={kycApplicant.KycBizStatus}");
                                return new KycCallbackResult
                                {
                                    Success = true,
                                    IsApproved = kycApplicant.KycBizStatus == KycBizStatus.APPROVED,
                                    UserId = kycApplicant.UserId,
                                    KycApplicantId = kycApplicant.Id,
                                    Message = "Ignored outdated callback event by timestamp"
                                };
                            }

                            if (request.Type == "applicantCreated")
                            {
                                Logger.Info($"Sumsub===HandleApplicantReviewed==创建==");
                                kycApplicant.ReviewStatus = request.ReviewStatus;
                            }

                            if (request.Type == "applicantPending")
                            {
                                Logger.Info($"Sumsub===HandleApplicantReviewed==待审核==");
                                kycApplicant.KycBizStatus = KycBizStatus.UNDERREVIEW;
                                kycApplicant.ReviewStatus = request.ReviewStatus;
                            }

                            if (request.Type == "applicantOnHold" || request.Type == "applicantPersonalInfoChanged")
                            {
                                Logger.Info($"Sumsub===HandleApplicantReviewed==审核中==");
                                if(request.ReviewStatus == "init")
                                {
                                    kycApplicant.KycBizStatus = KycBizStatus.PENDINGSUBMISSION;
                                    kycApplicant.ReviewStatus = request.ReviewStatus;
                                }
                                else if(request.ReviewStatus == "completed")
                                {
                                    //通过
                                    if (kycApplicant.ReviewAnswer == "GREEN")
                                    {
                                        kycApplicant.KycBizStatus = KycBizStatus.APPROVED;
                                        kycApplicant.ReviewStatus = request.ReviewStatus;
                                        Logger.Info($"BasicKYC===Setting KycBizStatus=APPROVED");
                                    }
                                    //不通过
                                    else if (kycApplicant.ReviewAnswer == "RED" && kycApplicant.ReviewRejectType == "FINAL")
                                    {
                                        kycApplicant.KycBizStatus = KycBizStatus.REJECTED;
                                        kycApplicant.ReviewStatus = request.ReviewStatus;
                                        Logger.Info($"BasicKYC===Setting KycBizStatus=REJECTED");
                                    }
                                    else if (kycApplicant.ReviewAnswer == "RED" && kycApplicant.ReviewRejectType == "RETRY")
                                    {
                                        kycApplicant.KycBizStatus = KycBizStatus.RESUBMISSIONREQUIRED;
                                        kycApplicant.ReviewStatus = request.ReviewStatus;
                                        Logger.Info($"BasicKYC===Setting KycBizStatus=RESUBMISSIONREQUIRED");
                                    }
                                }
                                else
                                {
                                    kycApplicant.KycBizStatus = KycBizStatus.UNDERREVIEW;
                                    kycApplicant.ReviewStatus = request.ReviewStatus;
                                }
    
                            }
                            var userBasicAuthLevel = AuthStandardLevel.Level1.GetAmbientValue();
                            if (request.Type == "applicantReviewed")
                            {
                                if (userBasicAuthLevel == request.LevelName)
                                {
                                    await BasicKYC(request, kycApplicant);
                                }
                                // Note: BasicKYB logic will be added if needed, checking existing types
                                // else if (kycApplicant.AuthStandardLevel == AuthStandardLevel.Level1)
                                // {
                                //    await BasicKYB(request, kycApplicant);
                                // }
                            }

                            //重置
                            if (request.Type == "applicantReset")
                            {
                                Logger.Info($"Sumsub===HandleApplicantReviewed==重置==");
                                kycApplicant.KycBizStatus = KycBizStatus.RESUBMISSIONREQUIRED;
                                kycApplicant.ReviewStatus = request.ReviewStatus;
                            }
                            if (request.Type == "applicantDeactivated")
                            {
                                Logger.Info($"Sumsub===HandleApplicantReviewed==禁用==");
                                kycApplicant.KycBizStatus = KycBizStatus.REJECTED;
                                kycApplicant.ReviewStatus = request.ReviewStatus;
                            }


                            kycApplicant.LastModificationTime = Clock.Now;
                            if (incomingCallbackTime.HasValue)
                            {
                                // 记录本次已处理回调时间，供后续回调做乱序判断（专用字段）
                                kycApplicant.LastCallbackEventTime = incomingCallbackTime.Value;
                            }
                            // 最后保存到数据库
                            await _kycApplicantRepository.UpdateAsync(kycApplicant);

                            var user = await _clientUserRepository.GetAll().Where(x => x.Id == kycApplicant.UserId).FirstOrDefaultAsync();
                            Logger.Info($"Sumsub===HandleApplicantReviewed==user=={JsonConvert.SerializeObject(user)}");
                            if (user == null)
                                return new KycCallbackResult { Success = false, Message = "User not found" };

                            var levelValueStr = ((int)kycApplicant.AuthStandardLevel).ToString();

                            // 仅在 KYC 结果发生明确变化时处理
                            if (kycApplicant.KycBizStatus == KycBizStatus.APPROVED
                                || kycApplicant.KycBizStatus == KycBizStatus.REJECTED
                                || kycApplicant.KycBizStatus == KycBizStatus.RESUBMISSIONREQUIRED)
                            {
                                if (string.IsNullOrWhiteSpace(levelValueStr))
                                    return new KycCallbackResult { Success = false, Message = "AuthStandardLevel is empty" };

                                // 解析当前已完成的等级
                                var levels = (user.KycLevelCompleted ?? string.Empty)
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim())
                                    .ToList();
                                Logger.Info($"Sumsub===HandleApplicantReviewed==已经存在的等级=={JsonConvert.SerializeObject(levels)}");
                                // ===== 通过：添加 =====
                                if (kycApplicant.KycBizStatus == KycBizStatus.APPROVED)
                                {
                                    if (!levels.Contains(levelValueStr, StringComparer.OrdinalIgnoreCase))
                                    {
                                        Logger.Info($"Sumsub===HandleApplicantReviewed=成功=已经存在的等级=={JsonConvert.SerializeObject(levels)}===当前等级{levelValueStr}==不存在添加");
                                        levels.Add(levelValueStr);
                                        user.KycLevelCompleted = string.Join(",", levels);
                                    }

                                }
                                // ===== 驳回：删除 =====
                                else if (kycApplicant.KycBizStatus == KycBizStatus.REJECTED || kycApplicant.KycBizStatus == KycBizStatus.RESUBMISSIONREQUIRED)
                                {
                                    Logger.Info($"Sumsub===HandleApplicantReviewed=拒绝=已经存在的等级=={JsonConvert.SerializeObject(levels)}===当前等级{levelValueStr}==");
                                    var removedCount = levels.RemoveAll(x =>
                                        x.Equals(levelValueStr, StringComparison.OrdinalIgnoreCase));
                                    Logger.Info($"Sumsub===HandleApplicantReviewed=拒绝=已经存在的等级=={JsonConvert.SerializeObject(levels)}===当前等级{levelValueStr}==={removedCount}=");
                                    if (removedCount > 0)
                                    {
                                        user.KycLevelCompleted = levels.Any()
                                            ? string.Join(",", levels)
                                            : null;
                                        Logger.Info($"Sumsub===HandleApplicantReviewed=拒绝=已经存在的等级=删除={JsonConvert.SerializeObject(levels)}===当前等级{levelValueStr}==={removedCount}=");
                                    }
                                }
                                user.FirstName = kycApplicant.FirstName;
                                user.LastName = kycApplicant.LastName;
                                user.MiddleName = kycApplicant.MiddleName;
                                user.CountryCode = kycApplicant.Country;
                                user.UserAuthStatus = kycApplicant.KycBizStatus;
                                await _clientUserRepository.UpdateAsync(user);

                            }
                            uow.Complete();
                            Logger.Info($"Sumsub===HandleApplicantReviewed==ok==");


                            if (kycApplicant.KycBizStatus == KycBizStatus.REJECTED || kycApplicant.KycBizStatus == KycBizStatus.RESUBMISSIONREQUIRED || kycApplicant.KycBizStatus == KycBizStatus.APPROVED)
                            {

                                if (_bus != null)
                                {
                                    await _bus.Send(new SumsubSaveSumsubDocumentImageEto
                                    {
                                        id = kycApplicant.Id.ToString(),
                                        applicantId = request.ApplicantId,
                                        inspectionId = request.InspectionId
                                    });
                                }
                                else
                                {
                                    Logger.Warn(
                                        "Sumsub===HandleApplicantReviewed==IBus not registered, skip sending SumsubSaveSumsubDocumentImageEto.");
                                }
                            }

                            Logger.Info($"Sumsub===HandleApplicantReviewed===KycBizStatus={kycApplicant.KycBizStatus}===IsApproved={(kycApplicant.KycBizStatus == KycBizStatus.APPROVED)}");

                            return new KycCallbackResult
                            {
                                Success = true,
                                IsApproved = kycApplicant.KycBizStatus == KycBizStatus.APPROVED,
                                UserId = kycApplicant.UserId,
                                KycApplicantId = kycApplicant.Id,
                                Message = "Processed successfully"
                            };

                        }
                        else
                        {
                            Logger.Error($"Sumsub===Exception===TheSystemIsBusy");
                            throw new UserFriendlyException("Error:TheSystemIsBusy");
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Sumsub===Exception==={ex.Message}==={ex.StackTrace}");
                return new KycCallbackResult { Success = false, Message = ex.Message };
            }
        }


        /// <summary>
        /// 处理回调
        /// </summary>
        /// <param name="payload">负载与签名</param>
        /// <param name="signature">签名</param>
        /// <returns></returns>
        public async Task<KycCallbackResult> HandleCallbackAsync(string payload, string signature)
        {
            var request = JsonConvert.DeserializeObject<HandleApplicantReviewedRequest>(payload);
            return await HandleApplicantReviewed(request);
        }

        /// <summary>
        /// 解析回调事件时间（优先使用 CreatedAtMs）
        /// </summary>
        private static DateTime? ParseCallbackEventTime(HandleApplicantReviewedRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.CreatedAtMs))
            {
                // 注意：不要混用 RoundtripKind 与 AssumeUniversal/AdjustToUniversal（会抛 ArgumentException）
                if (TryParseWebhookTimeToUtc(request.CreatedAtMs, out var createdAtMs))
                {
                    return createdAtMs;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.CreatedAt))
            {
                if (TryParseWebhookTimeToUtc(request.CreatedAt, out var createdAt))
                {
                    return createdAt;
                }
            }

            return null;
        }

        /// <summary>
        /// 安全解析 Sumsub 回调时间并统一转 UTC，避免 DateTimeStyles 组合异常
        /// </summary>
        private static bool TryParseWebhookTimeToUtc(string rawValue, out DateTime eventUtc)
        {
            eventUtc = default;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            // 先尝试 RoundtripKind（仅单独使用，不与 Assume/Adjust 组合）
            if (DateTimeOffset.TryParse(
                    rawValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var roundtrip))
            {
                eventUtc = roundtrip.UtcDateTime;
                return true;
            }

            // 再尝试常规 UTC 解析
            if (DateTimeOffset.TryParse(
                    rawValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                eventUtc = parsed.UtcDateTime;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前记录上次已处理回调时间
        /// </summary>
        private static DateTime? GetCurrentCallbackEventTime(KycApplicant kycApplicant)
        {
            if (kycApplicant.LastCallbackEventTime.HasValue)
            {
                return DateTime.SpecifyKind(kycApplicant.LastCallbackEventTime.Value, DateTimeKind.Utc);
            }

            if (kycApplicant.UpdatedTime.HasValue)
            {
                return DateTime.SpecifyKind(kycApplicant.UpdatedTime.Value, DateTimeKind.Utc);
            }

            if (kycApplicant.LastModificationTime.HasValue)
            {
                return DateTime.SpecifyKind(kycApplicant.LastModificationTime.Value, DateTimeKind.Utc);
            }

            if (kycApplicant.CreationTime != default)
            {
                return DateTime.SpecifyKind(kycApplicant.CreationTime, DateTimeKind.Utc);
            }

            return null;
        }

        private bool VerifySignature(string payload, string signature)
        {
            if (string.IsNullOrEmpty(_apiSetting.WebhookSecretKey)) return true; // Skip if no secret configured

            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_apiSetting.WebhookSecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var calculated = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return calculated == signature;
            }
        }


        /// <summary>
        /// 基础 KYC 处理
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="kycApplicant">KYC 申请人</param>
        private async Task BasicKYC(HandleApplicantReviewedRequest request, KycApplicant kycApplicant)
        {
            Logger.Info($"Sumsub===HandleApplicantReviewed==审核完成==");
            var applicant = await GetApplicantsInfo(request.ApplicantId);
            Logger.Info($"Sumsub===HandleApplicantReviewed==GetApplicantsInfo=={JsonConvert.SerializeObject(applicant)}");
            if (!applicant.Success)
            {
                throw new UserFriendlyException("Error:InternalServerError");
                //return HttpStatusCode.InternalServerError;
            }

            // 开始映射字段（除了 Id 主键外全部更新）
            kycApplicant.ApplicantId = request.ApplicantId;
            kycApplicant.InspectionId = applicant.InspectionId;
            kycApplicant.ExternalUserId = applicant.ExternalUserId;
            kycApplicant.ClientId = applicant.ClientId;
            kycApplicant.ApiKey = applicant.Key;

            // 申请人类型相关
            kycApplicant.ApplicantType = applicant.Type; // "individual"
            kycApplicant.ApplicantPlatform = applicant.ApplicantPlatform;
            kycApplicant.IpCountry = applicant.IpCountry;

            // 基础身份信息（info）
            if (applicant.Info != null)
            {
                kycApplicant.FirstName = applicant.Info.FirstName;
                kycApplicant.FirstNameEn = applicant.Info.FirstNameEn;
                kycApplicant.MiddleName = applicant.Info.MiddleName;
                kycApplicant.MiddleNameEn = applicant.Info.MiddleNameEn;
                kycApplicant.LastName = applicant.Info.LastName;
                kycApplicant.LastNameEn = applicant.Info.LastNameEn;
                kycApplicant.Country = applicant.Info.Country;

                if (DateTime.TryParse((string)applicant.Info.Dob, out DateTime dob))
                {
                    kycApplicant.DateOfBirth = dob;
                }
            }

            // 证件信息（从 IdDocs 取第一个）
            if (applicant.Info?.IdDocs != null && applicant.Info.IdDocs.Count > 0)
            {
                var doc = applicant.Info.IdDocs[0];
                kycApplicant.IdDocType = doc.IdDocType;
                kycApplicant.IdDocCountry = doc.Country;
                kycApplicant.IdDocNumber = doc.Number;

                if (DateTime.TryParse((string)doc.ValidUntil, out DateTime validUntil))
                {
                    kycApplicant.IdDocValidUntil = validUntil;
                }
            }

            // 修正信息（fixedInfo）
            if (applicant.FixedInfo != null)
            {
                kycApplicant.FixedFirstName = applicant.FixedInfo.FirstName;
                kycApplicant.FixedFirstNameEn = applicant.FixedInfo.FirstNameEn;
                kycApplicant.FixedMiddleName = applicant.FixedInfo.MiddleName;
                kycApplicant.FixedMiddleNameEn = applicant.FixedInfo.MiddleNameEn;
                kycApplicant.FixedLastName = applicant.FixedInfo.LastName;
                kycApplicant.FixedLastNameEn = applicant.FixedInfo.LastNameEn;

                // Gender/Nationality directly if dynamic supports it, cast as needed
                kycApplicant.Gender = (string)applicant.FixedInfo.Gender;
                kycApplicant.Nationality = (string)applicant.FixedInfo.Nationality;
                kycApplicant.TaxResidenceCountry = (string)applicant.FixedInfo.TaxResidenceCountry;

                if (DateTime.TryParse((string)applicant.FixedInfo.Dob, out DateTime fixedDob))
                {
                    kycApplicant.FixedDob = fixedDob;
                }
                if (applicant.FixedInfo?.Addresses != null
                    && applicant.FixedInfo.Addresses.Count > 0)
                {
                    var addr = applicant.FixedInfo.Addresses[0];

                    kycApplicant.Street = addr.StreetEn;
                    kycApplicant.State = addr.StateEn;
                    kycApplicant.BuildingNumber = addr.BuildingNumber;
                    kycApplicant.Town = addr.TownEn;
                    kycApplicant.PostCode = addr.PostCode;
                    kycApplicant.FormattedAddress = addr.FormattedAddress;
                }
            }

            // 联系方式
            kycApplicant.Email = applicant.Email;

            // 审核结果
            if (applicant.Review != null)
            {
                kycApplicant.ReviewStatus = applicant.Review.ReviewStatus;
                kycApplicant.ReviewAnswer = applicant.Review.ReviewResult?.ReviewAnswer;
                kycApplicant.ReviewLevel = applicant.Review.LevelName;
                kycApplicant.ReviewAttemptCount = applicant.Review.AttemptCnt;
                kycApplicant.Priority = applicant.Review.Priority != 0 ? Convert.ToInt32(applicant.Review.Priority) : (int?)null;
                kycApplicant.ReviewRejectType = applicant.Review.ReviewResult?.ReviewRejectType;
                kycApplicant.ModerationComment = applicant.Review.ReviewResult?.ModerationComment;
                kycApplicant.ClientComment = applicant.Review.ReviewResult?.ClientComment;

                if (applicant.Review.ReviewResult?.RejectLabels != null)
                {
                    kycApplicant.RejectLabels = string.Join(",", applicant.Review.ReviewResult.RejectLabels);
                }

                if (DateTime.TryParse(applicant.Review.CreateDate, out DateTime reviewDate))
                {
                    kycApplicant.ReviewDate = reviewDate;
                }
            }

            // Sumsub 创建时间
            if (DateTime.TryParse((string)applicant.CreatedAt, out DateTime createdAt))
            {
                kycApplicant.CreatedAt = createdAt;
            }

            // 更新时间
            kycApplicant.UpdatedTime = DateTime.UtcNow;

            // 原始 JSON 保存（用于审计）
            kycApplicant.RawJson = JsonConvert.SerializeObject(applicant);

            Logger.Info($"BasicKYC===ReviewStatus={kycApplicant.ReviewStatus}===ReviewAnswer={kycApplicant.ReviewAnswer}===ReviewRejectType={kycApplicant.ReviewRejectType}");

            if (kycApplicant.ReviewStatus == "completed")
            {
                //通过
                if (kycApplicant.ReviewAnswer == "GREEN")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.APPROVED;
                    Logger.Info($"BasicKYC===Setting KycBizStatus=APPROVED");
                }
                //不通过
                else if (kycApplicant.ReviewAnswer == "RED" && kycApplicant.ReviewRejectType == "FINAL")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.REJECTED;
                    Logger.Info($"BasicKYC===Setting KycBizStatus=REJECTED");
                }
                else if (kycApplicant.ReviewAnswer == "RED" && kycApplicant.ReviewRejectType == "RETRY")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.RESUBMISSIONREQUIRED;
                    Logger.Info($"BasicKYC===Setting KycBizStatus=RESUBMISSIONREQUIRED");
                }
            }
            else if (kycApplicant.ReviewStatus == "init")
            {
                //通过
                if (kycApplicant.ReviewAnswer == "GREEN")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.APPROVED;
                    Logger.Info($"BasicKYC===Setting KycBizStatus=APPROVED");
                }
                //不通过
                else if (kycApplicant.ReviewAnswer == "RED" && kycApplicant.ReviewRejectType == "FINAL")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.REJECTED;
                    Logger.Info($"BasicKYC===Setting KycBizStatus=REJECTED");
                }
                else if (kycApplicant.ReviewAnswer == "RED" && kycApplicant.ReviewRejectType == "RETRY")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.RESUBMISSIONREQUIRED;
                    Logger.Info($"BasicKYC===Setting KycBizStatus=RESUBMISSIONREQUIRED");
                }
            }
            else
            {
                Logger.Info($"BasicKYC===ReviewStatus is not completed, KycBizStatus will not be updated");
            }
        }

        /// <summary>
        /// 基础 KYB 处理
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="kycApplicant">KYC 申请人</param>
        private async Task BasicKYB(HandleApplicantReviewedRequest request, KycApplicant kycApplicant)
        {
            Logger.Info($"Sumsub===HandleApplicantReviewed==KYB审核完成==");

            var applicant = await GetCompanyApplicantsInfo(request.ApplicantId);
            Logger.Info($"Sumsub===HandleApplicantReviewed==GetCompanyApplicantsInfo=={JsonConvert.SerializeObject(applicant)}");

            if (!applicant.Success)
                throw new UserFriendlyException("Error:InternalServerError");

            /* ================= 基础标识 ================= */

            kycApplicant.ApplicantId = request.ApplicantId;
            kycApplicant.InspectionId = applicant.InspectionId;
            kycApplicant.ExternalUserId = applicant.ExternalUserId;
            kycApplicant.ClientId = applicant.ClientId;
            kycApplicant.ApiKey = applicant.Key;

            /* ================= 申请人类型 ================= */

            kycApplicant.ApplicantType = applicant.Type;           // company
            kycApplicant.ApplicantPlatform = applicant.ApplicantPlatform;
            kycApplicant.IpCountry = applicant.IpCountry;

            /* ================= 公司主体信息 ================= */

            var company = applicant.FixedInfo?.CompanyInfo;
            if (company != null)
            {
                kycApplicant.CompanyName = company.CompanyName;
                kycApplicant.CompanyRegistrationNumber = company.RegistrationNumber;
                kycApplicant.CompanyCountry = company.Country;
                kycApplicant.CompanyLegalAddress = company.LegalAddress;
                kycApplicant.CompanyEmail = company.Email;
                kycApplicant.OwnershipStructureDepth = company.OwnershipStructureDepth;

                if (company.SkippedTypes != null)
                {
                    kycApplicant.SkippedBeneficiaryTypes = string.Join(",", company.SkippedTypes);
                }
            }

            /* ================= Agreement ================= */

            if (applicant.Agreement != null)
            {
                if (DateTime.TryParse((string)applicant.Agreement.acceptedAt, out DateTime acceptedAt))
                {
                    kycApplicant.AgreementAcceptedAt = acceptedAt;
                }
                kycApplicant.AgreementSource = applicant.Agreement.source;
            }

            /* ================= Beneficiaries（UBO / Representative） ================= */

            if (company?.Beneficiaries != null)
            {
                // 幂等：先清空
                await _kycApplicantBeneficiaryRepository.DeleteAsync(
                    x => x.KycApplicantId == kycApplicant.Id);

                // JArray
                foreach (var ben in company.Beneficiaries)
                {
                    var beneficiary = new KycApplicantBeneficiary
                    {
                        KycApplicantId = kycApplicant.Id,

                        BeneficiaryId = ben.Id,
                        BeneficiaryApplicantId = ben.ApplicantId,
                        Submitted = ben.Submitted,

                        FirstName = ben.BeneficiaryInfo?.FirstName,
                        LastName = ben.BeneficiaryInfo?.LastName,
                        MiddleName = ben.BeneficiaryInfo?.MiddleName,

                        DateOfBirth = ben.BeneficiaryInfo?.Dob
                    };

                    if (ben.Types != null)
                    {
                        beneficiary.Role = string.Join(",", ben.Types);
                    }

                    await _kycApplicantBeneficiaryRepository.InsertAsync(beneficiary);
                }
            }

            /* ================= RequiredIdDocs（原样 JSON） ================= */

            if (applicant.RequiredIdDocs != null)
            {
                kycApplicant.RequiredIdDocsJson =
                    JsonConvert.SerializeObject(applicant.RequiredIdDocs);
            }

            /* ================= 审核结果 ================= */

            if (applicant.Review != null)
            {
                kycApplicant.ReviewStatus = applicant.Review.ReviewStatus;
                kycApplicant.ReviewAnswer = applicant.Review.ReviewResult?.ReviewAnswer;
                kycApplicant.ReviewLevel = applicant.Review.LevelName;
                kycApplicant.ReviewAttemptCount = applicant.Review.AttemptCnt;
                kycApplicant.Priority = applicant.Review.Priority != 0 ? Convert.ToInt32(applicant.Review.Priority) : (int?)null;

                kycApplicant.ReviewRejectType =
                    (string)applicant.Review.ReviewResult?.ReviewRejectType;

                kycApplicant.ModerationComment =
                    (string)applicant.Review.ReviewResult?.ModerationComment;

                kycApplicant.ClientComment =
                    (string)applicant.Review.ReviewResult?.ClientComment;

                if (applicant.Review.ReviewResult?.RejectLabels != null)
                {
                    kycApplicant.RejectLabels = string.Join(",", applicant.Review.ReviewResult.RejectLabels);
                }

                kycApplicant.ReviewDate = applicant.Review.ReviewDate;
            }

            /* ================= 时间 & 审计 ================= */
            kycApplicant.CreatedAt = applicant.CreatedAt;

            kycApplicant.UpdatedTime = DateTime.UtcNow;

            kycApplicant.RawJson = JsonConvert.SerializeObject(applicant);

            /* ================= 状态机 ================= */

            if (kycApplicant.ReviewStatus == "completed")
            {
                if (kycApplicant.ReviewAnswer == "GREEN")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.APPROVED;
                }
                else if (kycApplicant.ReviewAnswer == "RED"
                         && kycApplicant.ReviewRejectType == "FINAL")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.REJECTED;
                }
                else if (kycApplicant.ReviewAnswer == "RED"
                         && kycApplicant.ReviewRejectType == "RETRY")
                {
                    kycApplicant.KycBizStatus = KycBizStatus.RESUBMISSIONREQUIRED;
                }
            }
        }

        /// <summary>
        /// 保存 Sumsub 文档
        /// </summary>
        /// <param name="applicantId">申请人 ID</param>
        /// <param name="inspectionId">检查 ID</param>
        /// <param name="kycApplicantId">KYC 申请人实体 ID</param>
        /// <returns></returns>
        public async Task SaveSumsubDocument(string applicantId, string inspectionId, Guid kycApplicantId)
        {
            try
            {
                using var uow = _unitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.RequiresNew);

                // 1️⃣ 获取申请人主记录
                var applicantInfo = await _kycApplicantRepository
                    .GetAll()
                    .FirstOrDefaultAsync(x => x.Id == kycApplicantId);

                if (applicantInfo == null)
                    throw new UserFriendlyException("KYC Applicant 不存在");

                if (applicantInfo.AuthStandardLevel == AuthStandardLevel.Level1)
                {
                    await SaveBasicKYCSumsubDocument(applicantInfo, applicantId, inspectionId);
                }
                // else if(applicantInfo.AuthStandardLevel == AuthStandardLevel.Level1)
                // {
                //     await SaveBasicKYBSumsubDocument(applicantInfo, applicantId, inspectionId);
                // }

                uow.Complete();

                Logger.Info($"SaveSumsubDocument success: {applicantId}_{inspectionId}");
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"SaveSumsubDocument error: {applicantId}_{inspectionId}",
                    ex);
                throw;
            }
        }

        /// <summary>
        /// 保存基础 KYC Sumsub 文档
        /// </summary>
        /// <param name="applicantInfo">申请人信息</param>
        /// <param name="applicantId">申请人 ID</param>
        /// <param name="inspectionId">检查 ID</param>
        /// <returns></returns>
        public async Task SaveBasicKYCSumsubDocument(KycApplicant applicantInfo, string applicantId, string inspectionId)
        {
            // 2️⃣ 调 Sumsub 查询文档状态
            var applicant = await GetIdDocsStatus(applicantId);
            if (!applicant.Success || !applicant.ContainsKey("IDENTITY"))
                return;

            var identity = applicant["IDENTITY"];

            // 访问 ImageIds
            if (identity.ImageIds == null) return;
            var imageIds = identity.ImageIds;

            string targetIdDocType = identity.IdDocType;

            // 3️⃣ 读取本地已有文档（按 slot 建索引）
            var localDocs = await _kycApplicantDocumentRepository
                .GetAll()
                .Where(x =>
                    x.KycApplicantId == applicantInfo.Id &&
                    x.IdDocType == targetIdDocType)
                .ToListAsync();

            var localBySlot = localDocs.ToDictionary(x => x.ImageSlot);

            // 4️⃣ 遍历 Sumsub imageIds（slot = index）
            for (int slot = 0; slot < imageIds.Count; slot++)
            {
                var imageId = imageIds[slot].ToString();

                // 从 ImageReviewResults 字典中获取审核结果
                IdDocImageReviewResult reviewInfo = null;
                if (identity.ImageReviewResults != null && identity.ImageReviewResults.TryGetValue(imageId, out reviewInfo))
                {
                    // reviewInfo 已赋值
                }

                // 5️⃣ 下载并保存文件
                var imageUrl = await SaveSumsubDocumentImageAsync(
                    inspectionId,
                    imageId);

                if (localBySlot.TryGetValue(slot, out var exist))
                {
                    // 🔁 更新
                    exist.ImageId = imageId;
                    exist.Url = imageUrl;
                    exist.InspectionId = inspectionId;
                    exist.ReviewAnswer = reviewInfo?.ReviewAnswer;
                    exist.ClientComment = reviewInfo?.ClientComment;
                    exist.Country = identity.Country;

                    await _kycApplicantDocumentRepository.UpdateAsync(exist);
                }
                else
                {
                    // ➕ 新增
                    var entity = new KycApplicantDocument
                    {
                        KycApplicantId = applicantInfo.Id,
                        InspectionId = inspectionId,
                        IdDocType = identity.IdDocType,
                        ImageSlot = slot,

                        ImageId = imageId,
                        Url = imageUrl,
                        ReviewAnswer = reviewInfo?.ReviewAnswer,
                        ClientComment = reviewInfo?.ClientComment,
                        Country = identity.Country
                    };

                    await _kycApplicantDocumentRepository.InsertAsync(entity);
                }
            }
        }

        /// <summary>
        /// 保存基础 KYB Sumsub 文档
        /// </summary>
        /// <param name="applicantInfo">申请人信息</param>
        /// <param name="applicantId">申请人 ID</param>
        /// <param name="inspectionId">检查 ID</param>
        /// <returns></returns>
        public async Task SaveBasicKYBSumsubDocument(KycApplicant applicantInfo, string applicantId, string inspectionId)
        {
            // 2️⃣ 调 Sumsub 查询 KYB 文档状态
            var response = await GetKYBIdDocsStatus(applicantId);

            // 校验响应和 CompanyDocuments 是否存在
            if (response == null || response.CompanyDocuments == null)
                return;

            var companyDocs = response.CompanyDocuments;

            // 注意：新结构中 ImageIds 为空时处理
            if (companyDocs.ImageIds == null || companyDocs.ImageIds.Count == 0)
                return;

            string targetIdDocType = (string)companyDocs.IdDocType;

            // 3️⃣ 读取本地已有文档（按 slot 建索引）
            var localDocs = await _kycApplicantDocumentRepository
                .GetAll()
                .Where(x =>
                    x.KycApplicantId == applicantInfo.Id &&
                    x.IdDocType == targetIdDocType)
                .ToListAsync();

            var localBySlot = localDocs.ToDictionary(x => x.ImageSlot);

            // 4️⃣ 遍历 Sumsub imageIds（slot = index）
            for (int slot = 0; slot < companyDocs.ImageIds.Count; slot++)
            {
                // 注意：实体中定义是 List<long>，需转 string
                var imageIdLong = companyDocs.ImageIds[slot];
                var imageId = imageIdLong.ToString();

                string reviewAnswer = null;
                string clientComment = null;

                if (companyDocs.ImageReviewResults != null)
                {
                    var reviewResults = companyDocs.ImageReviewResults as Newtonsoft.Json.Linq.JObject;
                    if (reviewResults != null && reviewResults.ContainsKey(imageId))
                    {
                        dynamic rInfo = reviewResults[imageId];
                        reviewAnswer = rInfo?.reviewAnswer;
                        clientComment = rInfo?.clientComment;
                    }
                }

                // 5️⃣ 下载并保存文件
                var imageUrl = await SaveSumsubDocumentImageAsync(
                    inspectionId,
                    imageId);

                if (localBySlot.TryGetValue(slot, out var exist))
                {
                    // 🔁 更新
                    exist.ImageId = imageId;
                    exist.Url = imageUrl;
                    exist.InspectionId = inspectionId;
                    exist.ReviewAnswer = reviewAnswer;
                    exist.ClientComment = clientComment;
                    await _kycApplicantDocumentRepository.UpdateAsync(exist);
                }
                else
                {
                    // ➕ 新增
                    var entity = new KycApplicantDocument
                    {
                        KycApplicantId = applicantInfo.Id,
                        InspectionId = inspectionId,
                        IdDocType = companyDocs.IdDocType,
                        ImageSlot = slot,

                        ImageId = imageId,
                        Url = imageUrl,
                        ReviewAnswer = reviewAnswer,
                        ClientComment = clientComment,

                    };

                    await _kycApplicantDocumentRepository.InsertAsync(entity);
                }
            }
        }


        /// <summary>
        /// 执行 REST 请求
        /// </summary>
        /// <param name="resource">资源路径</param>
        /// <param name="parameters">参数</param>
        /// <param name="method">HTTP 方法</param>
        /// <returns></returns>
        public async Task<RestResponse> RestAction(
            string resource,
            Dictionary<string, object> parameters = null,
            Method method = Method.Get)
        {
            try
            {
                var client = new RestClient(_apiSetting.BaseUrl);
                var request = new RestRequest(resource, method);

                string path = resource;
                byte[] bodyBytes = Array.Empty<byte>();

                if (parameters != null)
                {
                    if (method == Method.Post || method == Method.Put || method == Method.Patch)
                    {
                        string jsonBody = JsonConvert.SerializeObject(parameters);

                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Accept", "application/json");
                        // Note: RequestFormat and AddBody behavior changed in newer RestSharp, 
                        // but AddParameter with RequestBody type usually works or AddStringBody.
                        request.AddStringBody(jsonBody, DataFormat.Json);

                        bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
                    }
                    else
                    {
                        foreach (var kv in parameters)
                            request.AddQueryParameter(kv.Key, kv.Value?.ToString() ?? "");

                        var qs = string.Join("&", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
                        if (!string.IsNullOrWhiteSpace(qs))
                            path += "?" + qs;
                    }
                }

                var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                HttpMethod httpMethod = method switch
                {
                    Method.Post => HttpMethod.Post,
                    Method.Put => HttpMethod.Put,
                    Method.Patch => HttpMethod.Patch,
                    Method.Delete => HttpMethod.Delete,
                    _ => HttpMethod.Get
                };

                string signature = CreateSignature(ts, httpMethod, path, bodyBytes);

                request.AddHeader("X-App-Token", _apiSetting.ApiToken);
                request.AddHeader("X-App-Access-Sig", signature);
                request.AddHeader("X-App-Access-Ts", ts.ToString());

                return await client.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Error("RestAction error: " + ex.Message, ex);
                throw;
            }
        }



        private async Task<ApplicantInfoResponse> GetApplicantsInfo(string applicantId)
        {
            var resp = await RestAction($"/resources/applicants/{applicantId}/one", method: Method.Get);
            Logger.Info($"Sumsub===HandleApplicantReviewed==GetApplicantsInfo=111={resp.Content}");
            if (resp.IsSuccessful)
            {
                try
                {
                    // 使用宽松的反序列化设置
                    var settings = new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    var applicantInfo = JsonConvert.DeserializeObject<ApplicantInfoResponse>(resp.Content, settings);
                    applicantInfo.Success = true;
                    return applicantInfo;
                }
                catch (JsonException ex)
                {
                    Logger.Error($"GetApplicantsInfo - Failed to deserialize ApplicantInfo: {ex.Message}");
                    Logger.Error($"GetApplicantsInfo - Response Content: {resp.Content}");
                    return new ApplicantInfoResponse { Success = false };
                }
            }
            return new ApplicantInfoResponse { Success = false };
        }

        private async Task<dynamic> GetCompanyApplicantsInfo(string applicantId)
        {
            // Assuming the endpoint for company is the same or similar, checking docs or user snippet context
            var resp = await RestAction($"/resources/applicants/{applicantId}/one", method: Method.Get);
            if (resp.IsSuccessful)
            {
                var json = JsonConvert.DeserializeObject<dynamic>(resp.Content);
                json.Success = true;
                return json;
            }
            return new { Success = false };
        }

        private async Task<IdDocsStatusResponse> GetIdDocsStatus(string applicantId)
        {
            var resp = await RestAction($"/resources/applicants/{applicantId}/requiredIdDocsStatus", method: Method.Get);
            if (resp.IsSuccessful)
            {
                try
                {
                    // 使用宽松的反序列化设置
                    var settings = new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    var idDocsStatus = JsonConvert.DeserializeObject<IdDocsStatusResponse>(resp.Content, settings);
                    idDocsStatus.Success = true;
                    return idDocsStatus;
                }
                catch (JsonException ex)
                {
                    Logger.Error($"GetIdDocsStatus - Failed to deserialize IdDocsStatus: {ex.Message}");
                    Logger.Error($"GetIdDocsStatus - Response Content: {resp.Content}");
                    return new IdDocsStatusResponse { Success = false };
                }
            }
            return new IdDocsStatusResponse { Success = false };
        }

        // Placeholder for KYB docs status if different
        private async Task<dynamic> GetKYBIdDocsStatus(string applicantId)
        {
            // For KYB it might return companyDocuments
            var resp = await RestAction($"/resources/applicants/{applicantId}/requiredIdDocsStatus", method: Method.Get);
            if (resp.IsSuccessful)
            {
                // The user snippet expects CompanyDocuments property in the response object
                var json = JsonConvert.DeserializeObject<dynamic>(resp.Content);
                // Manually constructing/checking the structure if API differs, but assuming std response

                // If structure is different for KYB, adapt here.
                // User said: "假设 GetKYBIdDocsStatus 已修改为返回 CompanyVerificationResponse"
                // I will return a dynamic that matches usage
                return json;
            }
            return null;
        }

        /// <summary>
        /// 保存 Sumsub 文档图片
        /// </summary>
        /// <param name="inspectionId">检查 ID</param>
        /// <param name="imageId">图片 ID</param>
        /// <returns>图片完整访问 URL（用于前端直连）</returns>
        public async Task<string> SaveSumsubDocumentImageAsync(string inspectionId, string imageId)
        {
            try
            {
                var client = new RestClient(_apiSetting.BaseUrl);
                var path = $"/resources/inspections/{inspectionId}/resources/{imageId}";

                var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var sig = CreateSignature(ts, HttpMethod.Get, path, null);

                var req = new RestRequest(path, Method.Get);
                req.AddHeader("X-App-Token", _apiSetting.ApiToken);
                req.AddHeader("X-App-Access-Sig", sig);
                req.AddHeader("X-App-Access-Ts", ts.ToString());

                var fileResp = await client.ExecuteAsync(req);
                if (fileResp.IsSuccessful && fileResp.RawBytes != null && fileResp.RawBytes.Length > 0)
                {
                    var fileName = $"{imageId}.jpg"; // Default
                    if (fileResp.ContentType == "image/png") fileName = $"{imageId}.png";
                    else if (fileResp.ContentType == "application/pdf") fileName = $"{imageId}.pdf";

                    // Upload to MinIO
                    var key = $"kyc-docs/{inspectionId}/{fileName}";
                    using (var ms = new MemoryStream(fileResp.RawBytes))
                    {
                        await _minio.UploadFile(ms, "kyc-docs", key, fileResp.ContentType);
                    }
                    // 落库时保存完整 URL，前端无需再拼接域名
                    return _minio.BuildPublicFileUrl("kyc-docs", key);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"SaveSumsubDocumentImageAsync error: {ex.Message}", ex);
            }
            return null;
        }


        private string CreateSignature(long ts, HttpMethod httpMethod, string path, byte[] body)
        {
            using (var hmac256 = new HMACSHA256(Encoding.ASCII.GetBytes(_apiSetting.ApiSecretKey)))
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(ts + httpMethod.Method.ToUpper() + path);
                if (body != null)
                {
                    using (var s = new MemoryStream())
                    {
                        s.Write(byteArray, 0, byteArray.Length);
                        s.Write(body, 0, body.Length);
                        byteArray = s.ToArray();
                    }
                }
                var result = hmac256.ComputeHash(new MemoryStream(byteArray))
                    .Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
                return result;
            }
        }
    }
}
