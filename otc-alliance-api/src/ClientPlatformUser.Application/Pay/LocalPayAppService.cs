using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Infrastructure;
using ClientPlatform.Orders;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Channels;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.UserManagement;
using ClientPlatform.Kyc;
using MerchantPaySettingEntity = ClientPlatform.Settings.MerchantPaySetting;

namespace ClientPlatformUser
{
    /// <summary>
    /// SunPay 本地支付（PAYIN 收银台）应用服务
    /// 与 VA 相关的 PayAppService 分开，专门处理本地法币支付业务
    /// </summary>
    public class LocalPayAppService : AppServiceBase
    {
        private readonly SunPayLocalPayChannelProvider _sunPayLocalPayChannelProvider;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<CollectionOrder, Guid> _collectionOrderRepository;
        private readonly IRepository<CollectionOrderDetail, Guid> _collectionOrderDetailRepository;
        private readonly IRepository<MerchantPaySettingEntity, Guid> _merchantPaySettingRepository;
        private readonly IDistributedLockService _distributedLockService;

        public LocalPayAppService(
            SunPayLocalPayChannelProvider sunPayLocalPayChannelProvider,
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<CollectionOrder, Guid> collectionOrderRepository,
            IRepository<CollectionOrderDetail, Guid> collectionOrderDetailRepository,
            IRepository<MerchantPaySettingEntity, Guid> merchantPaySettingRepository,
            IDistributedLockService distributedLockService)
        {
            _sunPayLocalPayChannelProvider = sunPayLocalPayChannelProvider;
            _clientUserRepository = clientUserRepository;
            _collectionOrderRepository = collectionOrderRepository;
            _collectionOrderDetailRepository = collectionOrderDetailRepository;
            _merchantPaySettingRepository = merchantPaySettingRepository;
            _distributedLockService = distributedLockService;
        }

        /// <summary>
        /// 创建  本地支付订单（收银台）
        /// 前端调用此接口获取 payment_url，然后跳转到  收银台页面
        /// </summary>
        [HttpPost]
        public async Task<PayApiResponse<SunPayLocalPayCreateResponse>> CreateLocalPayOrder([FromBody] CreateLocalPayOrderInput input)
        {
            var abpUserId = AbpSession.UserId;
            if (!abpUserId.HasValue)
            {
                throw new UserFriendlyException("No user logged in (AbpSession.UserId is null)");
            }

            // 1. 获取当前业务用户及其商户 / 联盟信息
            var clientUser = await _clientUserRepository.GetAll()
                .Include(u => u.Merchant)
                .ThenInclude(m => m.Alliance)
                .FirstOrDefaultAsync(u => u.AbpUserId == abpUserId.Value);

            if (clientUser == null || clientUser.MerchantId == null || clientUser.Merchant == null)
            {
                return new PayApiResponse<SunPayLocalPayCreateResponse>
                {
                    IsSuccess = false,
                    Code = 400,
                    Msg = L(ErrorCodes.Common.UserNotFound),
                    Data = null!
                };
            }

            // 1.1 检查 KYC 是否通过（只有通过的用户才能发起本地支付）
            if (clientUser.UserAuthStatus != KycBizStatus.APPROVED)
            {
                return new PayApiResponse<SunPayLocalPayCreateResponse>
                {
                    IsSuccess = false,
                    Code = 400,
                    Msg = L(ErrorCodes.Kyc.NotApproved),
                    Data = null!
                };
            }

            // 1.2 使用 Redis 分布式锁控制下单频率（同一用户在窗口期内只能成功获取一次锁）
            var lockKey = $"LocalPay:RateLimit:UserId:{clientUser.Id}";
            using (var rateLock = await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(30)))
            {
                if (rateLock == null)
                {
                    return new PayApiResponse<SunPayLocalPayCreateResponse>
                    {
                        IsSuccess = false,
                        Code = 400,
                        Msg = L(ErrorCodes.Common.TooFrequent),
                        Data = null!
                    };
                }

                // 1.3 检查商户支付配置：该币种 + 支付方式 在 MerchantPaySetting 中是否可用
                var merchantId = clientUser.MerchantId.Value;
                var isPaySettingEnabled = await _merchantPaySettingRepository.GetAll()
                    .AnyAsync(x =>
                        x.MerchantId == merchantId
                        && x.Type == MerchantPaySettingType.Direct
                        && x.Currency == input.Currency
                        && x.PaymentMethod == input.PaymentType
                        && x.Status == OpenClose.Open);

                if (!isPaySettingEnabled)
                {
                    return new PayApiResponse<SunPayLocalPayCreateResponse>
                    {
                        IsSuccess = false,
                        Code = 400,
                        Msg = "This currency and payment method are not supported.",
                        Data = null!
                    };
                }

                // 2. 构造 SunPay 商户配置（复用 VA 逻辑：BusinessID / Key）
                var merchantOption = new PayMerchantOption
                {
                    MerchantKey = clientUser.Merchant.BusinessID,
                    MerchantSecret = clientUser.Merchant.Key
                };

                // 3. 生成平台订单号（本地唯一标识）、交易参考简码，并构造 SunPay 请求参数
                var platformOrderNo = GeneratePlatformOrderNo(clientUser.Id);
                var referenceCode = GenerateReferenceCode();

                // 3.1 组装发送给 SunPay 的请求体（不暴露给前端）
                var sunPayInput = new SunPayLocalPayCreateInput
                {
                    OutOrderNo = platformOrderNo,
                    OutUserId = clientUser.Id.ToString(),

                    Amount = input.Amount,
                    Currency = input.Currency,
                    PaymentType = input.PaymentType,
                    // 后端生成唯一简码作为交易参考，带给 SunPay
                    Reference = referenceCode
                };

                // 4. 调用 SunPay 创建本地支付订单
                var response = await _sunPayLocalPayChannelProvider.CreateLocalPayOrderAsync(sunPayInput, merchantOption);

                // 如果 SunPay 返回失败，则不落订单，仅透传错误信息
                if (!response.IsSuccess || response.Data == null)
                {
                    return response;
                }

                var data = response.Data;

                // 5. 创建本地订单记录（CollectionOrder）—— 初始状态为 Pending
                var order = new CollectionOrder
                {
                    Id = Guid.NewGuid(),
                    PlatformOrderNo = platformOrderNo,
                    ChannelOrderNo = data.OrderNo,
                    AllianceId = clientUser.AllianceId,
                    AllianceName = clientUser.Alliance?.Name,
                    MerchantId = clientUser.MerchantId.Value,
                    MerchantName = clientUser.Merchant?.Name,
                    UserId = clientUser.Id,
                    UserName = clientUser.UserName,
                    UserEmail = clientUser.Email,
                    Amount = data.Amount,
                    Currency = data.Currency,
                    ChannelCode = "SunPayLocalPay",
                    PaymentMethod = input.PaymentType,
                    OrderType = CollectionOrderType.Direct,
                    OrderStatus = CollectionOrderStatus.Pending,
                    PayerName = data.CustomerName,
                    RecipientName = null,
                    AccountId = Guid.Empty,
                    Reference = referenceCode,
                    UserType = clientUser.UserType
                };

                await _collectionOrderRepository.InsertAsync(order);

                // 6. 创建订单明细（保存原始请求 / 响应 JSON，便于审计）
                var detail = new CollectionOrderDetail
                {
                    Id = Guid.NewGuid(),
                    CollectionOrderId = order.Id,
                    PayerName = data.CustomerName,
                    PayerCurrency = data.Currency,
                    // 记录 SunPay 实际请求体，便于问题排查
                    RequestInfo = JsonConvert.SerializeObject(sunPayInput),
                    ResponseInfo = JsonConvert.SerializeObject(response)
                };

                await _collectionOrderDetailRepository.InsertAsync(detail);

                return response;
            }
        }

        /// <summary>
        /// 生成平台订单号（本地支付）
        /// 格式：LP + yyyyMMddHHmmssfff + 用户ID（保证在本系统内大概率唯一）
        /// </summary>
        private string GeneratePlatformOrderNo(int userId)
        {
            return $"LP{DateTime.UtcNow:yyyyMMddHHmmssfff}{userId}";
        }

        /// <summary>
        /// 生成唯一交易参考简码（带给 SunPay 的 reference）
        /// 12 位小写字母数字，便于展示与对账
        /// </summary>
        private static string GenerateReferenceCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12);
        }

        /// <summary>
        /// 组装付款人展示姓名（优先使用 FirstName/MiddleName/LastName，其次使用 UserName）
        /// </summary>
        private string BuildCustomerName(ClientUser clientUser)
        {
            if (clientUser == null)
            {
                return string.Empty;
            }

            var first = clientUser.FirstName;
            var middle = clientUser.MiddleName;
            var last = clientUser.LastName;

            var parts = new[] { first, middle, last };
            var name = string.Join(" ", parts).Trim();

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return clientUser.UserName;
        }
    }
}

