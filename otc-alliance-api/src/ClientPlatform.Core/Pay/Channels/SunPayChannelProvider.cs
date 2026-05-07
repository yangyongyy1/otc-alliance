using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Kyc;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.Pay.Entities;
using ClientPlatform.Web;
using ClientPlatform.UserManagement;
using ClientPlatform.Authorization.Users;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Pay.Models;
using ClientPlatform.Pay.Channels.SunPay.Builders;
using ClientPlatform.Pay.Channels.SunPay;
using ClientPlatform;
using System.Security.Cryptography;
using System.Text;
using ClientPlatform.Infrastructure;
using ClientPlatform.Pay.Channels.SunPay.Dto;
using ClientPlatform.Pay.Dto;



namespace ClientPlatform.Pay.Channels
{
    public class SunPayChannelProvider : ITransientDependency, IChannelMetadataProvider
    {
        public ILogger Logger { get; set; }

        private readonly IPayClient _payClient;
        private readonly ServiceMinIo _serviceMinIo;
        private readonly SunPayCustomerInputBuilder _sunPayCustomerInputBuilder;
        private readonly IRepository<UserPayChannelCustomer, Guid> _payChannelCustomerRepository;
        private readonly IRepository<UserPayChannelAccount, Guid> _payChannelAccountRepository;
        private readonly IRepository<PayChannelAccountPaymentMethod, Guid> _paymentMethodRepository;
        private readonly IRepository<PayChannelRequestLog, Guid> _payChannelRequestLogRepository;
        private readonly IRepository<PayChannelServiceRequest, Guid> _payChannelServiceRequestRepository;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<MerchantChannelCurrency, int> _merchantChannelCurrencyRepository;
        private readonly IRepository<PayChannelTransaction, Guid> _payChannelTransactionRepository;
        private readonly Rebus.Bus.IBus _bus;
        private readonly Abp.Domain.Uow.IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedLockService _distributedLockService;

        public SunPayChannelProvider(
            IPayClient payClient,
            ServiceMinIo serviceMinIo,
            SunPayCustomerInputBuilder sunPayCustomerInputBuilder,
            IRepository<UserPayChannelCustomer, Guid> payChannelCustomerRepository,
            IRepository<UserPayChannelAccount, Guid> payChannelAccountRepository,
            IRepository<PayChannelAccountPaymentMethod, Guid> paymentMethodRepository,
            IRepository<PayChannelRequestLog, Guid> payChannelRequestLogRepository,
            IRepository<PayChannelServiceRequest, Guid> payChannelServiceRequestRepository,
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<MerchantChannelCurrency, int> merchantChannelCurrencyRepository,
            IRepository<PayChannelTransaction, Guid> payChannelTransactionRepository,
            Rebus.Bus.IBus bus,
            Abp.Domain.Uow.IUnitOfWorkManager unitOfWorkManager,
            IDistributedLockService distributedLockService)
        {
            _payClient = payClient;
            _serviceMinIo = serviceMinIo;
            _sunPayCustomerInputBuilder = sunPayCustomerInputBuilder;
            _payChannelCustomerRepository = payChannelCustomerRepository;
            _payChannelAccountRepository = payChannelAccountRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _payChannelRequestLogRepository = payChannelRequestLogRepository;
            _payChannelServiceRequestRepository = payChannelServiceRequestRepository;
            _clientUserRepository = clientUserRepository;
            _merchantChannelCurrencyRepository = merchantChannelCurrencyRepository;
            _payChannelTransactionRepository = payChannelTransactionRepository;
            _bus = bus;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedLockService = distributedLockService;
            Logger = NullLogger.Instance;
        }

        public async System.Threading.Tasks.Task<List<FieldDefinition>> GetRequiredFieldsAsync(string currency, string channelCode, PayMerchantOption merchantOption = null, string customerId = null)
        {
            // 对于核心业务 (EUR)，强制走代码定义路径 (Code-First Metadata)
            if (currency == "EUR" && (channelCode == ClientPlatformConsts.VirtualAccountChannelCodes.BCB.ToUpper()
                                      || channelCode == ClientPlatformConsts.VirtualAccountChannelCodes.Stables.ToUpper()
                                      || channelCode == ClientPlatformConsts.VirtualAccountChannelCodes.Weavr.ToUpper()
                                      || channelCode == ClientPlatformConsts.VirtualAccountChannelCodes.Mangopay.ToUpper()))
            {
                return await System.Threading.Tasks.Task.FromResult(GetStaticRequiredFields(channelCode, currency));
            }

            // Fallback: 尝试调用 API 获取动态字段 (针对未知渠道)
            if (merchantOption != null)
            {
                try
                {
                    var input = new GetAccountRequiredFieldsInput
                    {
                        Currency = currency,
                        CustomerId = customerId ?? ""
                    };

                    var response = await _payClient.GetCreateAccountRequiredFieldsAsync(input, merchantOption);
                    if (response.IsSuccess && response.Data != null)
                    {
                        return response.Data.Select(f => new FieldDefinition
                        {
                            Key = f.FieldName,
                            Label = f.Description ?? f.FieldName,
                            Type = f.Type ?? "text",
                            Required = f.Required
                        }).ToList();
                    }
                    else
                    {
                        Logger.Warn($"Failed to get dynamic required fields from SunPay: {response.Msg}.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error calling GetCreateAccountRequiredFieldsAsync", ex);
                }
            }

            // 如果既不是核心已知渠道，API又调用失败，则返回空列表 (或者抛出异常)
            Logger.Warn($"No required fields definition found for Currency: {currency}, Channel: {channelCode}");
            return new List<FieldDefinition>();
        }

        private List<FieldDefinition> GetStaticRequiredFields(string channelCode, string currency)
        {
            var fields = new List<FieldDefinition>();

            // 1. 查找此渠道配置的字段 ID 列表
            if (SunPayFieldConstants.ChannelProfiles.TryGetValue(channelCode, out var requiredIds))
            {
                foreach (var id in requiredIds)
                {
                    // 2. 从主表获取字段定义
                    if (SunPayFieldConstants.MasterFields.TryGetValue(id, out var fieldDef))
                    {
                        // 深拷贝一份，防止副作用 (虽然目前只读，但好习惯)
                        fields.Add(new FieldDefinition
                        {
                            Key = fieldDef.Key,
                            Label = fieldDef.Label,
                            Type = fieldDef.Type,
                            Required = fieldDef.Required,
                            RegexPattern = fieldDef.RegexPattern,
                            ValidationMessage = fieldDef.ValidationMessage,
                            Options = fieldDef.Options,
                            Description = fieldDef.Description,
                            DefaultValue = fieldDef.Key == "currency" ? currency : fieldDef.DefaultValue // Auto-fill currency
                        });
                    }
                    else
                    {
                        Logger.Warn($"Required field ID {id} not found in SunPayFieldConstants.MasterFields.");
                    }
                }
            }
            else
            {
                Logger.Warn($"No static profile found for channel: {channelCode}");
            }

            return fields;
        }

        /// <summary>
        /// 执行映射并创建 SunPay 客户
        /// </summary>
        public async System.Threading.Tasks.Task<string> CreateCustomerFromKycAsync(
            KycApplicant kyc,
            List<KycApplicantDocument> docs,
            PayMerchantOption merchantOption)
        {
            if (kyc == null) throw new ArgumentNullException(nameof(kyc));
            if (merchantOption == null) throw new ArgumentNullException(nameof(merchantOption));

            // 1. 识别并上传所需文档 (获取 File ID)
            var fileIdMap = await UploadRequiredDocumentsAsync(kyc, docs, merchantOption);

            // 2. 构建创建客户参数
            var input = _sunPayCustomerInputBuilder.Build(kyc, fileIdMap);

            // 3. 调用 API 创建客户
            var response = await _payClient.CreateCustomerAsync(input, merchantOption);

            if (response.Code != 200 || response.Data == null)
            {
                throw new Exception($"SunPay 创建客户失败: {response.Msg}");
            }

            return response.Data.CustomerId.ToString();
        }

        private async System.Threading.Tasks.Task<Dictionary<string, string>> UploadRequiredDocumentsAsync(
            KycApplicant kyc,
            List<KycApplicantDocument> docs,
            PayMerchantOption merchantOption)
        {
            var fileIdMap = new Dictionary<string, string>();

            // 识别逻辑文档
            var docMap = _sunPayCustomerInputBuilder.IdentifyDocuments(kyc, docs);

            foreach (var kvp in docMap)
            {
                var docTypeKey = kvp.Key;
                var docEntity = kvp.Value;

                if (string.IsNullOrEmpty(docEntity.Url)) continue;

                try
                {
                    // 获取文件流
                    using var stream = await _serviceMinIo.GetFileObject("kyc-docs", docEntity.Url);
                    if (stream == null)
                    {
                        Logger.Warn($"MinIO 文件未找到: {docEntity.Url}");
                        continue;
                    }

                    // 构建上传参数
                    var uploadInput = new UploadFileInput
                    {
                        FileName = Path.GetFileName(docEntity.Url) ?? "document.jpg",
                        FileStream = stream,
                        // BizType = docTypeKey // 可选，视 API 是否需要
                    };

                    // 调用通用上传接口 (返回 File ID)
                    var response = await _payClient.UploadFileAsync(uploadInput, merchantOption);
                    if (response.IsSuccess && response.Data != null && !string.IsNullOrEmpty(response.Data.FileId))
                    {
                        fileIdMap[docTypeKey] = response.Data.FileId;
                        Logger.Info($"文档上传成功 [{docTypeKey}]: {response.Data.FileId}");
                    }
                    else
                    {
                        Logger.Error($"文档上传失败 [{docTypeKey}]: {response.Msg}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"文档上传异常 [{docTypeKey}]: {ex.Message}", ex);
                }
            }

            return fileIdMap;
        }

        /// <summary>
        /// 处理 SunPay 回调
        /// </summary>
        /// <summary>
        /// 处理创建客户回调（Public）
        /// </summary>
        public async System.Threading.Tasks.Task HandleCustomerCallbackAsync(string payload, string signature)
        {
            Logger.Info($"SunPayChannelProvider===开始处理客户创建回调===Signature={signature}");
            // 验证签名逻辑可在此处复用

            var callback = System.Text.Json.JsonSerializer.Deserialize<Dto.Callback.SunPayCreateCustomerCallbackRequest>(payload);
            if (callback?.Data == null)
            {
                Logger.Error($"SunPayChannelProvider===回调数据为空");
                return;
            }

            var data = callback.Data;
            Logger.Info($"SunPayChannelProvider===回调数据===CustomerId={data.CustomerId}===OutUserId={data.OutUserId}===Status={data.Status}");

            // 根据 out_user_id 查找客户记录（out_user_id 对应我们的 UserId）
            long userId;
            if (!long.TryParse(data.OutUserId, out userId))
            {
                Logger.Error($"SunPayChannelProvider===OutUserId 格式错误===OutUserId={data.OutUserId}");
                return;
            }

            string lockKey = $"Lock:SunPayCustomerCallback:{userId}";
            using (await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10)))
            {
                var customer = await _payChannelCustomerRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.ChannelCustomerId == data.CustomerId);

                if (customer != null)
                {
                    Logger.Info($"SunPayChannelProvider===找到客户记录===CustomerId={customer.Id}===UserId={userId}");

                    // 更新客户信息（保存回调数据）
                    customer.Status = MapChannelStatus(data.Status);
                    customer.CallbackPayload = payload;
                    customer.RawData = payload;

                    await _payChannelCustomerRepository.UpdateAsync(customer);

                    Logger.Info($"SunPayChannelProvider===客户回调处理成功===UserId={userId}===ChannelCustomerId={data.CustomerId}===Status={data.Status}");

                    // Workflow Trigger: If Active, trigger Account Creation
                    if (customer.Status == PayChannelCustomerStatus.Active)
                    {
                        // Find pending request associated with this user and channel
                        // We look for PendingCustomer or CustomerProcessing
                        var request = await _payChannelServiceRequestRepository.GetAll()
                            .Where(r => r.UserId == userId
                                && (r.Status == PayChannelServiceRequestStatus.PendingCustomer || r.Status == PayChannelServiceRequestStatus.CustomerProcessing))
                            .OrderByDescending(r => r.CreationTime)
                            .FirstOrDefaultAsync();

                        if (request != null)
                        {
                            Logger.Info($"SunPayChannelProvider===找到挂起的请求，触发账户创建===RequestId={request.Id}");

                            request.Status = PayChannelServiceRequestStatus.PendingAccount;
                            request.CustomerId = customer.ChannelCustomerId;
                            // request.CustomerResponse could be updated here if we have full response, but we only have callback data. 
                            // Callback payload is already saved in customer entity.

                            await _payChannelServiceRequestRepository.UpdateAsync(request);

                            // Publish Account Creation Event (AFTER COMMIT)
                            _unitOfWorkManager.Current.Completed += (sender, args) =>
                            {
                                Abp.Threading.AsyncHelper.RunSync(() => _bus.Publish(new ClientPlatform.Pay.Dto.CreatePayChannelAccountEto { RequestId = request.Id }));
                            };
                            Logger.Info($"SunPayChannelProvider===已发布 CreatePayChannelAccountEto (On UOW Completed)===RequestId={request.Id}");
                        }
                        else
                        {
                            Logger.Warn($"SunPayChannelProvider===客户激活但未找到挂起的请求===UserId={userId}");
                        }
                    }
                }
                else
                {
                    Logger.Warn($"SunPayChannelProvider===未找到客户记录===OutUserId={data.OutUserId}===UserId={userId}");
                }
            }
        }

        /// <summary>
        /// 处理创建账户回调 (Public)
        /// </summary>
        public async System.Threading.Tasks.Task HandleAccountCallbackAsync(string payload, string signature)
        {
            Logger.Info($"SunPayChannelProvider===开始处理账户创建回调===payload={payload}");

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var callback = System.Text.Json.JsonSerializer.Deserialize<Dto.Callback.SunPayCreateAccountCallbackRequest>(payload, options);

                if (callback == null)
                {
                    Logger.Error("SunPayChannelProvider===反序列化失败: callback 为 null");
                    return;
                }

                if (callback.Data == null)
                {
                    Logger.Error("SunPayChannelProvider===反序列化失败: callback.Data 为 null");
                    return;
                }

                var data = callback.Data;
                Logger.Info($"SunPayChannelProvider===反序列化成功===OutUserId={data.OutUserId}===Currency={data.Currency}");

                // 根据 out_user_id 和 currency 查找账户记录
                // [Fix] Validate OutUserId before lock
                if (!long.TryParse(data.OutUserId, out long userId))
                {
                    Logger.Error($"SunPayChannelProvider===OutUserId 格式错误===OutUserId={data.OutUserId}");
                    return;
                }

                string lockKey = $"Lock:SunPayAccountCallback:{data.OutUserId}:{data.Currency}";
                Logger.Info($"SunPayChannelProvider===准备获取分布式锁===LockKey={lockKey}");

                using (var distLock = await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10)))
                {
                    if (distLock == null)
                    {
                        Logger.Warn($"SunPayChannelProvider===获取分布式锁失败 (超时)===LockKey={lockKey}");
                        return;
                    }
                    Logger.Info($"SunPayChannelProvider===成功获取分布式锁===LockKey={lockKey}");

                    // 根据 out_user_id 和 currency 查找账户记录
                    // 优先查找 Pending 或 NotSubmitted 的记录，从新到旧
                    var accounts = await _payChannelAccountRepository.GetAll()
                        .Where(x => x.UserId == userId && x.Currency == data.Currency && x.ChannelAccountId == data.AccountId)
                        .OrderByDescending(x => x.CreationTime)
                        .ToListAsync();

                    Logger.Info($"SunPayChannelProvider===查找现有账户===UserId={userId}===Currency={data.Currency}===FoundCount={accounts.Count}");

                    var account = accounts.FirstOrDefault(x => x.Status == VAStatus.Pending || x.Status == VAStatus.NotSubmitted)
                                 ?? accounts.FirstOrDefault();

                    if (account != null)
                    {
                        Logger.Info($"SunPayChannelProvider===选中账户进行更新===AccountId={account.Id}===CurrentStatus={account.Status}");
                    }
                    else
                    {
                        Logger.Info($"SunPayChannelProvider===未找到匹配账户，尝试查找 Pending 请求");
                    }

                    if (account == null)
                    {
                        // V2: Asynchronous Flow - Account might not exist yet. Check for Pending Request.
                        var request = await _payChannelServiceRequestRepository.GetAll()
                            .Where(r => r.UserId == userId
                                    && r.Currency == data.Currency
                                    && (r.Status == PayChannelServiceRequestStatus.AccountProcessing || r.Status == PayChannelServiceRequestStatus.PendingAccount))
                            .OrderByDescending(r => r.CreationTime)
                            .FirstOrDefaultAsync();

                        if (request != null)
                        {
                            Logger.Info($"SunPayChannelProvider===找到挂起的账户请求，开始创建账户===RequestId={request.Id}");

                            // 1. Find the Local Customer ID (FK)
                            var customer = await _payChannelCustomerRepository.FirstOrDefaultAsync(x => x.UserId == userId);
                            if (customer == null)
                            {
                                Logger.Error($"SunPayChannelProvider===严重: 收到账户回调但找不到客户记录===UserId={userId}");
                                return;
                            }

                            // Determine ChannelProvider dynamically
                            string channelProvider = request.ChannelProvider;
                            // Dynamic lookup removed as per user request (using request value directly)

                            // Fallback to "SunPay" if ChannelProvider is still null
                            if (string.IsNullOrEmpty(channelProvider))
                            {
                                channelProvider = "SunPay";
                            }

                            // 2. Create UserPayChannelAccount
                            account = new UserPayChannelAccount
                            {
                                UserId = (int)userId, // Explicit cast if UserId is int in new schema
                                CustomerId = customer.Id,
                                ChannelProvider = channelProvider,
                                ChannelAccountId = data.AccountId,
                                ReferenceId = userId.ToString(),
                                Currency = data.Currency,
                                AccountName = data.BankAccountHolderName, // Use Holder Name from callback
                                AccountNumber = !string.IsNullOrEmpty(data.Iban) ? data.Iban : data.AccountNumber,
                                BankName = data.BankName,
                                Status = MapAccountStatus(data.Status),
                                RawData = payload,
                                CreationTime = DateTime.UtcNow
                            };

                            await _payChannelAccountRepository.InsertAsync(account);
                            await _unitOfWorkManager.Current.SaveChangesAsync(); // Ensure ID is generated

                            Logger.Info($"SunPayChannelProvider===账户创建成功 (Callback Driven)===AccountId={account.Id}");

                            // 3. Update Request to Completed
                            if(account.Status == VAStatus.Active)
                            {
                                request.Status = PayChannelServiceRequestStatus.Completed;
                            }
                            else if(account.Status == VAStatus.Failed)
                            {
                                request.Status = PayChannelServiceRequestStatus.Failed;
                            }
                           
                            request.AccountId = data.AccountId;
                            request.AccountResponse = payload;
                            await _payChannelServiceRequestRepository.UpdateAsync(request);
                        }
                        else
                        {
                            Logger.Warn($"Account not found and No Pending Request for out_user_id: {data.OutUserId}, currency: {data.Currency}");
                            return;
                        }
                    }
                    else
                    {
                        // Existing Account Update Logic
                        Logger.Info($"SunPayChannelProvider===开始更新现有账户===AccountId={account.Id}");
                        account.ChannelAccountId = data.AccountId;
                        account.Status = MapAccountStatus(data.Status);
                        account.AccountName = data.BankAccountHolderName;
                        account.AccountNumber = !string.IsNullOrEmpty(data.Iban) ? data.Iban : data.AccountNumber;
                        account.BankName = data.BankName;
                        account.RawData = payload;

                        await _payChannelAccountRepository.UpdateAsync(account);
                        Logger.Info($"SunPayChannelProvider===现有账户更新成功===AccountId={account.Id}===Status={account.Status}");

                        // 同步更新对应的服务请求状态（避免请求表卡在 AccountProcessing）
                        var relatedRequest = await _payChannelServiceRequestRepository.GetAll()
                            .Where(r => r.UserId == userId
                                        && r.Currency == data.Currency
                                        && r.ChannelProvider == account.ChannelProvider
                                        && (r.Status == PayChannelServiceRequestStatus.AccountProcessing
                                            || r.Status == PayChannelServiceRequestStatus.PendingAccount))
                            .OrderByDescending(r => r.CreationTime)
                            .FirstOrDefaultAsync();

                        if (relatedRequest != null)
                        {
                            if (account.Status == VAStatus.Active)
                            {
                                relatedRequest.Status = PayChannelServiceRequestStatus.Completed;
                            }
                            else if (account.Status == VAStatus.Failed)
                            {
                                relatedRequest.Status = PayChannelServiceRequestStatus.Failed;
                            }

                            relatedRequest.AccountId = data.AccountId;
                            relatedRequest.AccountResponse = payload;
                            await _payChannelServiceRequestRepository.UpdateAsync(relatedRequest);

                            Logger.Info($"SunPayChannelProvider===同步更新请求状态===RequestId={relatedRequest.Id}===NewStatus={relatedRequest.Status}");
                        }
                        else
                        {
                            Logger.Info("SunPayChannelProvider===未找到需要更新状态的服务请求记录（可能为历史账户或手工创建账户）");
                        }
                    }

                    // 根据币种解析并保存支付方式详情
                    if (data.Currency == "EUR" && !string.IsNullOrEmpty(data.Iban))
                    {
                        // EUR 账户：保存为 SEPA 支付方式
                        await SaveSepaPaymentMethod(account.Id, data);
                    }
                    else if (data.Currency == "USD" && data.DepositInstructions != null)
                    {
                        // USD 账户：保存 ACH、Fedwire、SWIFT 三种支付方式
                        if (data.DepositInstructions.Ach != null) await SaveUsdPaymentMethod(account.Id, "ACH", data.DepositInstructions.Ach);
                        if (data.DepositInstructions.Fedwire != null) await SaveUsdPaymentMethod(account.Id, "Fedwire", data.DepositInstructions.Fedwire);
                        if (data.DepositInstructions.Swift != null) await SaveUsdPaymentMethod(account.Id, "SWIFT", data.DepositInstructions.Swift);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"SunPayChannelProvider===处理账户回调时发生异常: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 处理创建账户回调 (Private, Deprecated - use public HandleAccountCallbackAsync instead)
        /// </summary>
        [Obsolete("This method is deprecated. Use the public HandleAccountCallbackAsync(string payload, string signature) method instead.")]
        private async System.Threading.Tasks.Task HandleCreateAccountCallbackAsync(string payload)
        {
            // Delegate to the public method, passing null for signature as it's not available here.
            // This method is kept for backward compatibility if any internal calls still use it.
            await HandleAccountCallbackAsync(payload, null);
        }

        /// <summary>
        /// 保存 EUR 的 SEPA 支付方式
        /// </summary>
        private async System.Threading.Tasks.Task SaveSepaPaymentMethod(Guid accountId, Dto.Callback.SunPayAccountCallbackData data)
        {
            // 查询该账户的 SEPA 支付方式是否已存在
            var existing = await _paymentMethodRepository.FirstOrDefaultAsync(
                x => x.AccountId == accountId && x.PaymentType == "SEPA"
            );

            if (existing != null)
            {
                // 更新现有记录
                existing.AccountHolderName = data.BankAccountHolderName;
                existing.AccountNumber = data.Iban;
                existing.SwiftBic = data.SwiftBic;
                existing.InstitutionName = data.BankName;

                await _paymentMethodRepository.UpdateAsync(existing);
                Logger.Info($"Updated SEPA payment method for account {accountId}, IBAN: {data.Iban}");
            }
            else
            {
                // 插入新记录
                var sepa = new PayChannelAccountPaymentMethod
                {
                    AccountId = accountId,
                    PaymentType = "SEPA",
                    AccountHolderName = data.BankAccountHolderName,
                    AccountNumber = data.Iban,
                    SwiftBic = data.SwiftBic,
                    InstitutionName = data.BankName
                };

                await _paymentMethodRepository.InsertAsync(sepa);
                Logger.Info($"Inserted SEPA payment method for account {accountId}, IBAN: {data.Iban}");
            }
        }

        /// <summary>
        /// 保存 USD 的支付方式（ACH/Fedwire/SWIFT）
        /// </summary>
        private async System.Threading.Tasks.Task SaveUsdPaymentMethod(Guid accountId, string paymentType, Dto.Callback.PaymentMethodDetail detail)
        {
            if (detail?.AccountDetail == null) return;

            // 查询该账户的该类型支付方式是否已存在
            var existing = await _paymentMethodRepository.FirstOrDefaultAsync(
                x => x.AccountId == accountId && x.PaymentType == paymentType
            );

            if (existing != null)
            {
                // 更新现有记录
                existing.AccountHolderName = detail.AccountDetail.AccountHolderName;
                existing.AccountNumber = detail.AccountDetail.AccountNumber;
                existing.AccountRoutingNumber = detail.AccountDetail.AccountRoutingNumber;
                existing.Memo = detail.AccountDetail.Memo;
                existing.SwiftBic = detail.AccountDetail.SwiftBic;
                existing.IntermediarySwiftBic = detail.AccountDetail.IntermediaryInstitutionSwiftBic;

                existing.InstitutionName = detail.InstitutionInformation?.InstitutionName;
                existing.InstitutionAddressLine1 = detail.InstitutionInformation?.AddressLine1;
                existing.InstitutionCity = detail.InstitutionInformation?.City;
                existing.InstitutionState = detail.InstitutionInformation?.State;
                existing.InstitutionPostalCode = detail.InstitutionInformation?.PostalCode;
                existing.InstitutionCountryCode = detail.InstitutionInformation?.CountryCode;

                existing.HolderAddressLine1 = detail.AccountHolderAddress?.AddressLine1;
                existing.HolderCity = detail.AccountHolderAddress?.City;
                existing.HolderState = detail.AccountHolderAddress?.State;
                existing.HolderPostalCode = detail.AccountHolderAddress?.PostalCode;
                existing.HolderCountryCode = detail.AccountHolderAddress?.CountryCode;

                await _paymentMethodRepository.UpdateAsync(existing);
                Logger.Info($"Updated {paymentType} payment method for account {accountId}, account_number: {detail.AccountDetail.AccountNumber}");
            }
            else
            {
                // 插入新记录
                var method = new PayChannelAccountPaymentMethod
                {
                    AccountId = accountId,
                    PaymentType = paymentType,

                    // 账户详情
                    AccountHolderName = detail.AccountDetail.AccountHolderName,
                    AccountNumber = detail.AccountDetail.AccountNumber,
                    AccountRoutingNumber = detail.AccountDetail.AccountRoutingNumber,
                    Memo = detail.AccountDetail.Memo,
                    SwiftBic = detail.AccountDetail.SwiftBic,
                    IntermediarySwiftBic = detail.AccountDetail.IntermediaryInstitutionSwiftBic,

                    // 机构信息
                    InstitutionName = detail.InstitutionInformation?.InstitutionName,
                    InstitutionAddressLine1 = detail.InstitutionInformation?.AddressLine1,
                    InstitutionCity = detail.InstitutionInformation?.City,
                    InstitutionState = detail.InstitutionInformation?.State,
                    InstitutionPostalCode = detail.InstitutionInformation?.PostalCode,
                    InstitutionCountryCode = detail.InstitutionInformation?.CountryCode,

                    // 持有人地址（Fedwire/SWIFT）
                    HolderAddressLine1 = detail.AccountHolderAddress?.AddressLine1,
                    HolderCity = detail.AccountHolderAddress?.City,
                    HolderState = detail.AccountHolderAddress?.State,
                    HolderPostalCode = detail.AccountHolderAddress?.PostalCode,
                    HolderCountryCode = detail.AccountHolderAddress?.CountryCode
                };

                await _paymentMethodRepository.InsertAsync(method);
                Logger.Info($"Inserted {paymentType} payment method for account {accountId}, account_number: {detail.AccountDetail.AccountNumber}");
            }
        }

        /// <summary>
        /// 处理更新客户回调（待实现）
        /// </summary>
        private async System.Threading.Tasks.Task HandleUpdateCustomerCallbackAsync(string payload)
        {
            // TODO: 实现客户更新回调处理
            Logger.Info("UpdateCustomer callback received, processing not implemented yet");
            await System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// 处理更新账户回调（待实现）
        /// </summary>
        private async System.Threading.Tasks.Task HandleUpdateAccountCallbackAsync(string payload)
        {
            // TODO: 实现账户更新回调处理
            Logger.Info("UpdateAccount callback received, processing not implemented yet");
            await System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// 处理交易回调 (Collection, CollectionSettlement)
        /// </summary>
        /// <summary>
        /// 处理 Collection (入金) 回调
        /// </summary>
        public async System.Threading.Tasks.Task HandleCollectionCallbackAsync(string payload, string signature)
        {
            Logger.Info($"HandleCollectionCallbackAsync===开始处理 Collection 回调===Signature={signature}");

            // 使用 Newtonsoft.Json 以支持 [JsonProperty] 属性 (处理 snake_case)
            var callback = Newtonsoft.Json.JsonConvert.DeserializeObject<SunPayCollectionCallbackDto>(payload);
            if (callback?.Data == null)
            {
                Logger.Error($"HandleCollectionCallbackAsync===Collection 回调数据为空");
                return;
            }

            var data = callback.Data;
            Logger.Info($"HandleCollectionCallbackAsync===Collection 回调数据===OrderNo={data.OrderNo}===Amount={data.Amount} {data.Currency}===Status={data.Status}===Sender={data.Sender?.Name}===Recipient={data.Recipient?.AccountId}");

            if (string.IsNullOrEmpty(data.OrderNo))
            {
                Logger.Error("HandleCollectionCallbackAsync===OrderNo 为空，无法处理 Collection 回调");
                return;
            }

            string lockKey = $"Lock:SunPayTransaction:{data.OrderNo}";
            using (await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10)))
            {
                await ProcessTransactionUpsertAsync(callback);
            }
        }

        /// <summary>
        /// 处理 CollectionSettlement (结算) 回调
        /// </summary>
        public async System.Threading.Tasks.Task HandleCollectionSettlementCallbackAsync(string payload, string signature)
        {
            Logger.Info($"SunPayChannelProvider===开始处理 CollectionSettlement 回调===Signature={signature}");

            // 使用 Newtonsoft.Json 以支持 [JsonProperty] 属性
            var callback = Newtonsoft.Json.JsonConvert.DeserializeObject<SunPayCollectionCallbackDto>(payload);
            if (callback?.Data == null)
            {
                Logger.Error($"SunPayChannelProvider===CollectionSettlement 回调数据为空");
                return;
            }

            var data = callback.Data;
            Logger.Info($"SunPayChannelProvider===CollectionSettlement 回调数据===OrderNo={data.OrderNo}===SettlementStatus={data.SettlementStatus}===SettlementAmount={data.SettlementAmount} {data.SettlementCurrency}===SettlementFee={data.SettlementFee}");

            if (string.IsNullOrEmpty(data.OrderNo))
            {
                Logger.Error("SunPayChannelProvider===OrderNo 为空，无法处理 CollectionSettlement 回调");
                return;
            }

            string lockKey = $"Lock:SunPayTransaction:{data.OrderNo}";
            using (await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10)))
            {
                await ProcessTransactionUpsertAsync(callback);
            }
        }

        private async System.Threading.Tasks.Task ProcessTransactionUpsertAsync(SunPayCollectionCallbackDto callback)
        {
            var data = callback.Data;

            // 1. 查找现有交易记录
            var transaction = await _payChannelTransactionRepository.GetAll()
                .FirstOrDefaultAsync(x => x.OrderNo == data.OrderNo);

            Logger.Info($"ProcessTransactionUpsertAsync===查找现有交易===OrderNo={data.OrderNo}===Found={(transaction != null)}");

            if (transaction == null)
            {
                // New Transaction
                // 2. 查找关联账户
                // SunPay recipient.account_id 对应我们的 ChannelAccountId
                if (string.IsNullOrEmpty(data.Recipient?.AccountId))
                {
                    Logger.Error($"HandleCollectionCallbackAsync===Recipient.AccountId 为空，无法关联账户===OrderNo={data.OrderNo}");
                    return;
                }

                var account = await _payChannelAccountRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.ChannelAccountId == data.Recipient.AccountId);

                Logger.Info($"ProcessTransactionUpsertAsync===查找关联账户===RecipientAccountId={data.Recipient.AccountId}===FoundAccountId={account?.Id}");

                if (account == null)
                {
                    Logger.Warn($"HandleCollectionCallbackAsync===未找到关联账户===ChannelAccountId={data.Recipient.AccountId}===OrderNo={data.OrderNo}");
                    // 即使找不到账户，也许应该也可以存下来？但 FK 约束会失败。
                    // 暂时跳过或记录错误。
                    return;
                }

                transaction = new PayChannelTransaction
                {
                    ChannelAccountId = account.Id,
                    ChannelProvider = account.ChannelProvider,
                    OrderNo = data.OrderNo,
                    OutOrderNo = data.OutOrderNo,
                    OutUserId = data.OutUserId,
                    Amount = data.Amount,
                    Currency = data.Currency,
                    Reference = data.Reference,
                    PaymentType = data.PaymentType,
                    Status = data.Status,
                    BizType = callback.BizType,
                    BizStatus = callback.BizStatus, // Top level status

                    // Sender
                    SenderName = data.Sender?.Name,
                    SenderAccountNumber = data.Sender?.AccountNumber,
                    SenderIban = data.Sender?.Iban,
                    SenderSwiftBic = data.Sender?.SwiftBic,

                    // Recipient
                    RecipientAccountId = data.Recipient?.AccountId,
                    RecipientCurrency = data.Recipient?.Currency,
                    RecipientIban = data.Recipient?.Iban,
                    RecipientSwiftBic = data.Recipient?.SwiftBic,
                    RecipientBankCountry = data.Recipient?.BankCountry,
                    RecipientBankAddress = data.Recipient?.BankAddress,
                    RecipientAccountNumber = data.Recipient?.AccountNumber,
                    RecipientBankName = data.Recipient?.BankName,
                    RecipientBankAccountHolderName = data.Recipient?.BankAccountHolderName,

                    // Time
                    TransactionCreationTime = data.CreationTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.CreationTime.Value).DateTime : null,
                    TransactionCompletionTime = data.CompletionTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.CompletionTime.Value).DateTime : null
                };

                // Settlement info if available
                if (callback.BizType == "CollectionSettlement" || !string.IsNullOrEmpty(data.SettlementStatus))
                {
                    transaction.SettlementStatus = data.SettlementStatus;
                    transaction.SettlementAmount = data.SettlementAmount;
                    transaction.SettlementCurrency = data.SettlementCurrency;
                    transaction.SettlementFee = data.SettlementFee;
                    transaction.SettlementFeeCurrency = data.SettlementFeeCurrency;
                    transaction.SettlementTime = data.SettlementTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.SettlementTime.Value).DateTime : null;
                }

                // Insert
                await _payChannelTransactionRepository.InsertAsync(transaction);
                Logger.Info($"SunPayChannelProvider===新建交易记录成功===Id={transaction.Id}===OrderNo={data.OrderNo}");
            }
            else
            {
                // Update Existing Transaction
                transaction.Status = data.Status;
                transaction.BizType = callback.BizType; // Update BizType (e.g. Collection -> CollectionSettlement)
                transaction.BizStatus = callback.BizStatus;
                if (data.CompletionTime.HasValue)
                {
                    transaction.TransactionCompletionTime = DateTimeOffset.FromUnixTimeMilliseconds(data.CompletionTime.Value).DateTime;
                }

                // Update Settlement Info if present
                if (callback.BizType == "CollectionSettlement" || !string.IsNullOrEmpty(data.SettlementStatus))
                {
                    transaction.SettlementStatus = data.SettlementStatus;
                    transaction.SettlementAmount = data.SettlementAmount;
                    transaction.SettlementCurrency = data.SettlementCurrency;
                    transaction.SettlementFee = data.SettlementFee;
                    transaction.SettlementFeeCurrency = data.SettlementFeeCurrency;
                    transaction.SettlementTime = data.SettlementTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.SettlementTime.Value).DateTime : null;
                }

                await _payChannelTransactionRepository.UpdateAsync(transaction);
                Logger.Info($"HandleCollectionCallbackAsync===更新交易记录成功===Id={transaction.Id}===OrderNo={data.OrderNo}===NewStatus={transaction.Status}===SettlementStatus={transaction.SettlementStatus}");
            }

            // Publish Event (Optional - good for decoupling)
            // await _bus.Publish(new PayChannelTransactionUpdatedEto { TransactionId = transaction.Id });
        }

        private PayChannelCustomerStatus MapChannelStatus(string channelStatus)
        {
            if (string.IsNullOrEmpty(channelStatus)) return PayChannelCustomerStatus.Pending;
            var statusUpper = channelStatus.ToUpper().Trim();

            if (statusUpper == SunPayFieldConstants.CustomerStatus.PENDING ||
                statusUpper == SunPayFieldConstants.CustomerStatus.REVIEWING)
                return PayChannelCustomerStatus.Pending;

            if (statusUpper == SunPayFieldConstants.CustomerStatus.ACTIVE) return PayChannelCustomerStatus.Active;
            if (statusUpper == SunPayFieldConstants.CustomerStatus.FROZEN) return PayChannelCustomerStatus.Frozen;
            if (statusUpper == SunPayFieldConstants.CustomerStatus.DISABLE) return PayChannelCustomerStatus.Disabled;
            if (statusUpper == SunPayFieldConstants.CustomerStatus.FAILED) return PayChannelCustomerStatus.Failed;

            return PayChannelCustomerStatus.Pending;
        }

        private VAStatus MapAccountStatus(string channelStatus)
        {
            if (string.IsNullOrEmpty(channelStatus)) return VAStatus.Pending;
            var statusUpper = channelStatus.ToUpper().Trim();

            // Based on user provided AmbientValues
            switch (statusUpper)
            {
                case "PENDDING":
                case "REVIEWING": // Fallback
                    return VAStatus.Pending;
                case "ACTIVE":
                    return VAStatus.Active;
                case "FAILED":
                    return VAStatus.Failed;
                case "SUSPENDED":
                    return VAStatus.Suspended;
                case "CLOSED":
                    return VAStatus.Closed;
                default:
                    return VAStatus.Pending;
            }
        }

        private bool VerifySignature(string payload, string signature)
        {
            // TODO: Get Secret Key from Config properly
            // var secretKey = _configuration["Payment:SunPay:SecretKey"];
            var secretKey = "YOUR_TEST_SECRET_KEY";

            if (string.IsNullOrEmpty(signature)) return false;

            try
            {
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
                {
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    return computedSignature == signature.ToLower();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error verifying signature", ex);
                return false;
            }
        }
    }
}
