using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ClientPlatform.Authorization.Users;
using ClientPlatform.Kyc;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Channels.SunPay;
using ClientPlatform.Pay.Channels.SunPay.Builders;
using ClientPlatform.Pay.Dto;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Entities;
using ClientPlatform.UserManagement;
using ClientPlatform.Web;
using ClientPlatform.Infrastructure;
using Rebus.Handlers;
using Rebus.Bus;

namespace ClientPlatform.RabbitMq.Handlers
{
    /// <summary>
    /// 创建支付渠道客户消息处理器
    /// </summary>
    public class CreatePayChannelCustomerHandler : IHandleMessages<CreatePayChannelCustomerEto>, ITransientDependency
    {
        private readonly UserManager _userManager;
        private readonly IRepository<KycApplicant, Guid> _kycApplicantRepository;
        private readonly IRepository<KycApplicantDocument, Guid> _kycDocumentRepository;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<UserPayChannelCustomer, Guid> _payChannelCustomerRepository;
        private readonly IPayClient _payClient;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly SunPayCustomerInputBuilder _sunPayCustomerInputBuilder;
        private readonly ServiceMinIo _minIoService;
        private readonly IRepository<PayChannelServiceRequest, Guid> _payChannelServiceRequestRepository;
        private readonly IBus _bus;
        private readonly IDistributedLockService _distributedLockService;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public CreatePayChannelCustomerHandler(
            UserManager userManager,
            IRepository<KycApplicant, Guid> kycApplicantRepository,
            IRepository<KycApplicantDocument, Guid> kycDocumentRepository,
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<UserPayChannelCustomer, Guid> payChannelCustomerRepository,
            IPayClient payClient,
            IUnitOfWorkManager unitOfWorkManager,
            SunPayCustomerInputBuilder sunPayCustomerInputBuilder,
            ServiceMinIo minIoService,
            IRepository<PayChannelServiceRequest, Guid> payChannelServiceRequestRepository,
            IBus bus,
            IDistributedLockService distributedLockService)
        {
            _userManager = userManager;
            _kycApplicantRepository = kycApplicantRepository;
            _kycDocumentRepository = kycDocumentRepository;
            _clientUserRepository = clientUserRepository;
            _payChannelCustomerRepository = payChannelCustomerRepository;
            _payClient = payClient;
            _unitOfWorkManager = unitOfWorkManager;
            _sunPayCustomerInputBuilder = sunPayCustomerInputBuilder;
            _minIoService = minIoService;
            _payChannelServiceRequestRepository = payChannelServiceRequestRepository;
            _bus = bus;
            _distributedLockService = distributedLockService;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        [UnitOfWork]
        public virtual async Task Handle(CreatePayChannelCustomerEto message)
        {
            Logger.Info($"CreatePayChannelCustomerHandler===开始处理===UserId={message.UserId}===KycApplicantId={message.KycApplicantId}");

            // =============== 分布式锁：防止并发处理 ===============
            var lockKey = $"CreateCustomer:UserId:{message.UserId}";
            using (var distributedLock = await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(30)))
            {
                if (distributedLock == null)
                {
                    Logger.Warn($"CreatePayChannelCustomerHandler===获取分布式锁失败，跳过重复处理===UserId={message.UserId}===LockKey={lockKey}");
                    return; // 其他实例正在处理，跳过
                }

                Logger.Info($"CreatePayChannelCustomerHandler===成功获取分布式锁===UserId={message.UserId}");

                PayChannelServiceRequest request = null;
                if (message.RequestId.HasValue)
                {
                    request = await _payChannelServiceRequestRepository.FirstOrDefaultAsync(message.RequestId.Value);
                }

                try
                {
                    // 步骤1：获取用户信息
                    Logger.Info($"CreatePayChannelCustomerHandler===步骤1：获取用户信息===UserId={message.UserId}");

                    // 步骤2：获取KYC信息
                    var kycApplicant = await _kycApplicantRepository.GetAsync(message.KycApplicantId);
                    if (kycApplicant == null)
                    {
                        Logger.Error($"CreatePayChannelCustomerHandler===KYC记录不存在===KycApplicantId={message.KycApplicantId}");
                        return;
                    }

                    // 步骤3：获取ClientUser和Merchant
                    var clientUser = await _clientUserRepository.GetAllIncluding(u => u.Merchant)
                        .FirstOrDefaultAsync(u => u.Id == message.UserId);

                    if (clientUser == null || clientUser.Merchant == null)
                    {
                        Logger.Error($"CreatePayChannelCustomerHandler===ClientUser或Merchant不存在===UserId={message.UserId}");
                        return;
                    }

                    // 步骤4：检查是否已存在客户记录（提前检查，避免重复创建）
                    Logger.Info($"CreatePayChannelCustomerHandler===步骤4：检查客户是否已创建===UserId={message.UserId}");
                    var existingCustomer = await _payChannelCustomerRepository.GetAll()
                        .FirstOrDefaultAsync(x => x.UserId == message.UserId && x.ChannelProvider == "SunPay");

                    if (existingCustomer != null && !string.IsNullOrEmpty(existingCustomer.ChannelCustomerId))
                    {
                        Logger.Info($"CreatePayChannelCustomerHandler===客户已存在，无需重复创建===UserId={message.UserId}===CustomerId={existingCustomer.ChannelCustomerId}===Status={existingCustomer.Status}");
                        return;
                    }

                    // 步骤5：构建商户配置（需要先获取，用于后续文件上传）
                    Logger.Info($"CreatePayChannelCustomerHandler===步骤5：获取商户配置===UserId={message.UserId}");
                    var merchantOption = new PayMerchantOption
                    {
                        MerchantKey = clientUser.Merchant.BusinessID,
                        MerchantSecret = clientUser.Merchant.Key,
                        InvitationCode = clientUser.InviteCode ?? string.Empty
                    };

                    // 步骤6：查询并处理真实文档
                    Logger.Info($"CreatePayChannelCustomerHandler===步骤6：查询文档记录===KycApplicantId={kycApplicant.Id}");
                    var documents = await _kycDocumentRepository.GetAll()
                        .Where(d => d.KycApplicantId == kycApplicant.Id)
                        .ToListAsync();

                    Logger.Info($"CreatePayChannelCustomerHandler===查询到 {documents.Count} 个文档记录");

                    // 使用 Builder 筛选需要的文档类型
                    var docMapping = _sunPayCustomerInputBuilder.IdentifyDocuments(kycApplicant, documents);
                    Logger.Info($"CreatePayChannelCustomerHandler===筛选出 {docMapping.Count} 个有效文档");

                    // 下载并上传文档到 SunPay
                    var fileIds = new System.Collections.Generic.Dictionary<string, string>();
                    foreach (var kvp in docMapping)
                    {
                        var docType = kvp.Key;
                        var doc = kvp.Value;

                        try
                        {
                            Logger.Info($"CreatePayChannelCustomerHandler===开始处理文档===DocType={docType}===ImageId={doc.ImageId}===IdDocType={doc.IdDocType}===Url={doc.Url}");

                            // 从 MinIO 下载文档（使用默认 bucket: ）
                            var bucketName = _minIoService.defalut_bucketName;
                            Logger.Info($"CreatePayChannelCustomerHandler===调用 MinIO 下载===Bucket={bucketName}===ObjectName={doc.Url}");
                            var stream = await _minIoService.GetFileObject(bucketName, doc.Url);
                            if (stream == null)
                            {
                                Logger.Warn($"CreatePayChannelCustomerHandler===MinIO 下载失败，跳过===DocType={docType}===Url={doc.Url}");
                                continue;
                            }

                            Logger.Info($"CreatePayChannelCustomerHandler===MinIO 下载成功，准备上传到 SunPay===DocType={docType}");

                            // 上传到 SunPay
                            var uploadInput = new UploadFileInput
                            {
                                FileName = $"{docType}_{doc.ImageId}.jpg",
                                FileStream = stream
                            };
                            var uploadRes = await _payClient.UploadFileAsync(uploadInput, merchantOption);

                            if (uploadRes.IsSuccess && uploadRes.Data != null)
                            {
                                fileIds[docType] = uploadRes.Data.FileId;
                                Logger.Info($"CreatePayChannelCustomerHandler===文档上传成功===DocType={docType}===FileId={uploadRes.Data.FileId}");
                            }
                            else
                            {
                                Logger.Warn($"CreatePayChannelCustomerHandler===SunPay 上传失败===DocType={docType}===Error={uploadRes.Msg}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"CreatePayChannelCustomerHandler===文档处理失败===DocType={docType}===Error={ex.Message}", ex);
                        }
                    }

                    Logger.Info($"CreatePayChannelCustomerHandler===文档上传完成===SuccessCount={fileIds.Count}");

                    // 步骤7：使用真实文件 ID 构建请求参数
                    CreateCustomerInput input;
                    try
                    {
                        // [Fix] Extract Overrides from RequestPayload if available
                        Dictionary<string, string> overrides = null;
                        if (request != null && !string.IsNullOrEmpty(request.RequestPayload))
                        {
                            try
                            {
                                overrides = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.RequestPayload);
                                Logger.Info($"CreatePayChannelCustomerHandler===提取覆盖参数===Count={overrides?.Count ?? 0}");
                            }
                            catch (Exception ex)
                            {
                                Logger.Warn($"CreatePayChannelCustomerHandler===解析覆盖参数失败===Error={ex.Message}");
                            }
                        }

                        input = _sunPayCustomerInputBuilder.Build(kycApplicant, fileIds, overrides);
                        Logger.Info($"CreatePayChannelCustomerHandler===请求参数构建成功===OutUserId={input.OutUserId}===CustomerType={input.CustomerType}===CountryCode={input.CountryCode}===FileCount={fileIds.Count}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"CreatePayChannelCustomerHandler===请求参数构建失败===UserId={message.UserId}===Error={ex.Message}", ex);
                        throw;
                    }

                    // 步骤8：准备或更新客户记录
                    UserPayChannelCustomer customer = null;
                    if (existingCustomer != null)
                    {
                        // 已存在记录但没有 CustomerId，更新并继续创建
                        Logger.Info($"CreatePayChannelCustomerHandler===发现未完成的客户记录，更新后继续创建===UserId={message.UserId}");
                        customer = existingCustomer;
                        customer.RequestPayload = JsonConvert.SerializeObject(input);
                        await _payChannelCustomerRepository.UpdateAsync(customer);
                    }
                    else
                    {
                        customer = new UserPayChannelCustomer
                        {
                            UserId = message.UserId,
                            ChannelProvider = "SunPay",
                            ReferenceId = message.UserId.ToString(),
                            Status = PayChannelCustomerStatus.Pending,
                            EntityType = PayChannelCustomerEntityType.Individual,
                            RequestPayload = JsonConvert.SerializeObject(input)
                        };
                        await _payChannelCustomerRepository.InsertAsync(customer);
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                    }

                    Logger.Info($"CreatePayChannelCustomerHandler===步骤9：构建merchantOption===MerchantKey={merchantOption.MerchantKey}");

                    // 步骤10：调用SunPay创建客户API
                    Logger.Info($"CreatePayChannelCustomerHandler===步骤7：调用 SunPay API===UserId={message.UserId}");
                    var response = await _payClient.CreateCustomerAsync(input, merchantOption);

                    customer.ResponsePayload = JsonConvert.SerializeObject(response);

                    if (response.IsSuccess && response.Data != null)
                    {
                        if (response.Data.CustomerId != Guid.Empty)
                        {
                            customer.ChannelCustomerId = response.Data.CustomerId.ToString();
                            customer.Status = MapChannelStatus(response.Data.Status);
                            customer.RawData = JsonConvert.SerializeObject(response.Data);

                            await _payChannelCustomerRepository.UpdateAsync(customer);
                            Logger.Info($"CreatePayChannelCustomerHandler===客户创建成功===UserId={message.UserId}===CustomerId={response.Data.CustomerId}");

                            if (request != null)
                            {
                                request.CustomerId = response.Data.CustomerId.ToString();
                                request.CustomerResponse = JsonConvert.SerializeObject(response);

                                // Active-Only Check
                                if (customer.Status == PayChannelCustomerStatus.Active)
                                {
                                    request.Status = PayChannelServiceRequestStatus.PendingAccount;
                                    await _payChannelServiceRequestRepository.UpdateAsync(request);
                                    await _bus.Publish(new CreatePayChannelAccountEto { RequestId = request.Id });
                                    Logger.Info($"CreatePayChannelCustomerHandler===客户状态Active，立即触发账户创建===RequestId={request.Id}");
                                }
                                else
                                {
                                    // If Pending/Reviewing, wait for callback
                                    request.Status = PayChannelServiceRequestStatus.CustomerProcessing;
                                    await _payChannelServiceRequestRepository.UpdateAsync(request);
                                    Logger.Info($"CreatePayChannelCustomerHandler===客户状态非Active({customer.Status})，等待回调===RequestId={request.Id}");
                                }
                            }
                            else
                            {
                                Logger.Warn($"CreatePayChannelCustomerHandler===客户创建成功，但request为null===UserId={message.UserId}");
                            }
                        }
                        else
                        {
                            customer.Status = PayChannelCustomerStatus.Failed;
                            await _payChannelCustomerRepository.UpdateAsync(customer);
                            Logger.Warn($"CreatePayChannelCustomerHandler===CustomerId无效===UserId={message.UserId}");

                            if (request != null)
                            {
                                request.Status = PayChannelServiceRequestStatus.Failed;
                                request.FailStep = PayChannelServiceRequestFailStep.Customer;
                                request.FailReason = "CustomerId invalid";
                                await _payChannelServiceRequestRepository.UpdateAsync(request);
                            }
                        }
                    }
                    else
                    {
                        customer.Status = PayChannelCustomerStatus.Failed;
                        await _payChannelCustomerRepository.UpdateAsync(customer);
                        Logger.Error($"CreatePayChannelCustomerHandler===API调用失败===UserId={message.UserId}===Message={response.Msg}");

                        if (request != null)
                        {
                            request.Status = PayChannelServiceRequestStatus.Failed;
                            request.FailStep = PayChannelServiceRequestFailStep.Customer;
                            request.FailReason = response.Msg;
                            await _payChannelServiceRequestRepository.UpdateAsync(request);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"CreatePayChannelCustomerHandler===处理失败===UserId={message.UserId}===Error={ex.Message}", ex);

                    if (request != null)
                    {
                        request.Status = PayChannelServiceRequestStatus.Failed;
                        request.FailStep = PayChannelServiceRequestFailStep.Customer;
                        request.FailReason = ex.Message;
                        await _payChannelServiceRequestRepository.UpdateAsync(request);
                    }
                    throw;
                }
            } // 释放分布式锁
        }

        private PayChannelCustomerStatus MapChannelStatus(string channelStatus)
        {
            if (string.IsNullOrEmpty(channelStatus))
            {
                return PayChannelCustomerStatus.Pending;
            }

            return channelStatus.ToLower() switch
            {
                "active" => PayChannelCustomerStatus.Active,
                "inactive" => PayChannelCustomerStatus.Disabled,
                "suspended" => PayChannelCustomerStatus.Frozen,
                "closed" => PayChannelCustomerStatus.Disabled,
                "frozen" => PayChannelCustomerStatus.Frozen,
                "disabled" => PayChannelCustomerStatus.Disabled,
                "failed" => PayChannelCustomerStatus.Failed,
                _ => PayChannelCustomerStatus.Pending
            };
        }
    }
}
