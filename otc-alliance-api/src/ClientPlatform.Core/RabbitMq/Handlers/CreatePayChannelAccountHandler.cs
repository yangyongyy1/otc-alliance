using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rebus.Bus;
using Rebus.Handlers;
using ClientPlatform.Authorization.Users;
using ClientPlatform.Infrastructure;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Dto;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Entities;
using ClientPlatform.UserManagement;

namespace ClientPlatform.RabbitMq.Handlers
{
    /// <summary>
    /// 创建支付渠道账户消息处理器
    /// </summary>
    public class CreatePayChannelAccountHandler : IHandleMessages<CreatePayChannelAccountEto>, ITransientDependency
    {
        private readonly IRepository<PayChannelServiceRequest, Guid> _payChannelServiceRequestRepository;
        private readonly IRepository<UserPayChannelAccount, Guid> _payChannelAccountRepository;
        private readonly IRepository<UserPayChannelCustomer, Guid> _payChannelCustomerRepository;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IPayClient _payClient;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IBus _bus;
        private readonly IDistributedLockService _distributedLockService;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public CreatePayChannelAccountHandler(
            IRepository<PayChannelServiceRequest, Guid> payChannelServiceRequestRepository,
            IRepository<UserPayChannelAccount, Guid> payChannelAccountRepository,
            IRepository<UserPayChannelCustomer, Guid> payChannelCustomerRepository,
            IRepository<ClientUser, int> clientUserRepository,
            IPayClient payClient,
            IUnitOfWorkManager unitOfWorkManager,
            IBus bus,
            IDistributedLockService distributedLockService)
        {
            _payChannelServiceRequestRepository = payChannelServiceRequestRepository;
            _payChannelAccountRepository = payChannelAccountRepository;
            _payChannelCustomerRepository = payChannelCustomerRepository;
            _clientUserRepository = clientUserRepository;
            _payClient = payClient;
            _unitOfWorkManager = unitOfWorkManager;
            _bus = bus;
            _distributedLockService = distributedLockService;
        }

        [UnitOfWork]
        public virtual async Task Handle(CreatePayChannelAccountEto message)
        {
            Logger.Info($"CreatePayChannelAccountHandler===开始处理===RequestId={message.RequestId}");

            // =============== 分布式锁：防止并发处理 ===============
            var lockKey = $"CreateAccount:RequestId:{message.RequestId}";
            using (var distributedLock = await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(60)))
            {
                if (distributedLock == null)
                {
                    Logger.Warn($"CreatePayChannelAccountHandler===获取分布式锁失败，跳过重复处理===RequestId={message.RequestId}===LockKey={lockKey}");
                    return; // 其他实例正在处理，跳过
                }

                Logger.Info($"CreatePayChannelAccountHandler===成功获取分布式锁===RequestId={message.RequestId}");

                try
                {
                    await ProcessRequest(message);
                }
                catch (Exception ex)
                {
                    Logger.Error($"CreatePayChannelAccountHandler===处理失败===RequestId={message.RequestId}===Error={ex.Message}", ex);
                    throw;
                }
            } // 释放分布式锁
        }

        private async Task ProcessRequest(CreatePayChannelAccountEto message)
        {
            var request = await _payChannelServiceRequestRepository.FirstOrDefaultAsync(message.RequestId);
            if (request == null)
            {
                Logger.Error($"CreatePayChannelAccountHandler===请求记录不存在===RequestId={message.RequestId}");
                return;
            }

            if (request.Status == PayChannelServiceRequestStatus.Completed)
            {
                Logger.Info($"CreatePayChannelAccountHandler===请求已完成，无需重复处理===RequestId={message.RequestId}");
                return;
            }

            try
            {
                request.Status = PayChannelServiceRequestStatus.AccountProcessing;
                await _payChannelServiceRequestRepository.UpdateAsync(request);

                // 1. 获取 ClientUser 以构建 MerchantOption
                var clientUser = await _clientUserRepository.GetAllIncluding(u => u.Merchant)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId);

                if (clientUser == null || clientUser.Merchant == null)
                {
                    throw new Exception($"ClientUser or Merchant not found for UserId={request.UserId}");
                }

                var merchantOption = new PayMerchantOption
                {
                    MerchantKey = clientUser.Merchant.BusinessID,
                    MerchantSecret = clientUser.Merchant.Key,
                    InvitationCode = clientUser.InviteCode ?? string.Empty
                };

                // 2. 准备 CreateAccountInput
                var input = JsonConvert.DeserializeObject<CreateAccountInput>(request.RequestPayload);
                if (input == null)
                {
                    throw new Exception("Failed to deserialize RequestPayload");
                }

                // 设置 CustomerId (从 CustomerHandler 处理结果中获取)
                input.CustomerId = request.CustomerId;

                Logger.Info($"CreatePayChannelAccountHandler===调用 API创建账户===UserId={request.UserId}===CustomerId={request.CustomerId}===Currency={input.Currency}=={JsonConvert.SerializeObject(input)}");

                // 3. 调用 API
                var response = await _payClient.CreateAccountAsync(input, merchantOption);

                Logger.Info($"CreatePayChannelAccountHandler===调用 API创建账户===UserId={request.UserId}===CustomerId={request.CustomerId}===Currency={input.Currency}==res={JsonConvert.SerializeObject(response)}");

                if (response.IsSuccess && response.Data != null)
                {
                    Logger.Info($"CreatePayChannelAccountHandler===账户创建成功===UserId={request.UserId}===AccountId={response.Data.AccountId}");

                    Logger.Info($"CreatePayChannelAccountHandler===账户申请提交成功 (等待回调)===UserId={request.UserId}===AccountId={response.Data.AccountId}");

                    // 4. 更新请求状态为处理中 (等待 VAAccountReceive 回调)
                    request.Status = PayChannelServiceRequestStatus.AccountProcessing;
                    request.AccountId = response.Data.AccountId;
                    request.AccountResponse = JsonConvert.SerializeObject(response);

                    // 注意：此处不创建 UserPayChannelAccount，也不标记为 Completed。
                    // 真正的账户创建和完成状态将在 HandleAccountCallbackAsync 中处理。

                    await _payChannelServiceRequestRepository.UpdateAsync(request);
                }
                else
                {
                    Logger.Error($"CreatePayChannelAccountHandler===账户创建失败===UserId={request.UserId}===Msg={response.Msg}");
                    request.Status = PayChannelServiceRequestStatus.Failed;
                    request.FailStep = PayChannelServiceRequestFailStep.Account;
                    request.FailReason = response.Msg;
                    request.AccountResponse = JsonConvert.SerializeObject(response);
                    await _payChannelServiceRequestRepository.UpdateAsync(request);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"CreatePayChannelAccountHandler===处理异常===RequestId={message.RequestId}===Error={ex.Message}", ex);
                request.Status = PayChannelServiceRequestStatus.Failed;
                request.FailStep = PayChannelServiceRequestFailStep.Account;
                request.FailReason = ex.Message;
                await _payChannelServiceRequestRepository.UpdateAsync(request);
            }
        }
    }
}
