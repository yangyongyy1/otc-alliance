using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Authorization.Users;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Infrastructure;
using ClientPlatform.Orders;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Channels.SunPay.Builders;
using ClientPlatform.Pay.Entities;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.UserManagement;
using ClientPlatform.Web;

namespace ClientPlatform.Pay.Channels
{
    /// <summary>
    /// SunPay 本地支付 ChannelProvider（PAYIN 收银台）
    /// 继承自 SunPayChannelProvider，专门处理本地支付订单与回调，和 VA 业务在代码上分层
    /// </summary>
    public class SunPayLocalPayChannelProvider : SunPayChannelProvider
    {
        private readonly IPayClient _payClient;
        private readonly IRepository<CollectionOrder, Guid> _collectionOrderRepository;
        private readonly IRepository<CollectionOrderDetail, Guid> _collectionOrderDetailRepository;
        private readonly IDistributedLockService _distributedLockService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        /// <summary>
        /// 构造函数注入依赖
        /// 通过 base(...) 复用 SunPayChannelProvider 的依赖，同时注入本地支付订单相关仓储
        /// </summary>
        public SunPayLocalPayChannelProvider(
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
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedLockService distributedLockService,
            IRepository<CollectionOrder, Guid> collectionOrderRepository,
            IRepository<CollectionOrderDetail, Guid> collectionOrderDetailRepository)
            : base(
                  payClient,
                  serviceMinIo,
                  sunPayCustomerInputBuilder,
                  payChannelCustomerRepository,
                  payChannelAccountRepository,
                  paymentMethodRepository,
                  payChannelRequestLogRepository,
                  payChannelServiceRequestRepository,
                  clientUserRepository,
                  merchantChannelCurrencyRepository,
                  payChannelTransactionRepository,
                  bus,
                  unitOfWorkManager,
                  distributedLockService)
        {
            _payClient = payClient;
            _collectionOrderRepository = collectionOrderRepository;
            _collectionOrderDetailRepository = collectionOrderDetailRepository;
            _distributedLockService = distributedLockService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        #region 本地支付下单与回调处理

        /// <summary>
        /// 创建 SunPay 本地支付（PAYIN 收银台）订单
        /// 这里只负责调用 SunPay 接口本身，订单落库由上层业务（如 PayAppService）组合处理
        /// </summary>
        /// <param name="input">本地支付下单参数</param>
        /// <param name="merchantOption">商户配置（包含 MerchantKey / MerchantSecret）</param>
        /// <returns>SunPay 返回的收银台链接等信息</returns>
        public async Task<PayApiResponse<SunPayLocalPayCreateResponse>> CreateLocalPayOrderAsync(
            SunPayLocalPayCreateInput input,
            PayMerchantOption merchantOption)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (merchantOption == null) throw new ArgumentNullException(nameof(merchantOption));

            Logger.Info(
                $"SunPayLocalPayChannelProvider===开始创建本地支付订单===OutOrderNo={input.OutOrderNo}===OutUserId={input.OutUserId}===Amount={input.Amount} {input.Currency}===PaymentType={input.PaymentType}");

            var response = await _payClient.CreateLocalPayOrderAsync(input, merchantOption);

            Logger.Info(
                $"SunPayLocalPayChannelProvider===本地支付下单完成===OutOrderNo={input.OutOrderNo}===IsSuccess={response.IsSuccess}===Code={response.Code}===Msg={response.Msg}");

            return response;
        }

        /// <summary>
        /// 处理 SunPay 本地支付（PAYIN 收银台）回调
        /// 回调示例：
        /// {
        ///   "biz_status":"SUCCESS",
        ///   "biz_type":"PAYIN",
        ///   "msg":null,
        ///   "data":{
        ///     "order_no":"O202303141635547856778850304",
        ///     "out_order_no":"20230314154529612",
        ///     "out_user_id":"123",
        ///     "amount":100.0,
        ///     "fee":1,
        ///     "currency":"USD",
        ///     "name":"123"
        ///   }
        /// }
        /// </summary>
        /// <param name="payload">请求体原始 JSON 字符串</param>
        /// <param name="signature">签名（预留，后续可以接入校验）</param>
        public async Task HandlePayinCallbackAsync(string payload, string signature)
        {
            Logger.Info($"SunPayPayinCallback===收到本地支付回调===Payload={payload}");

            if (string.IsNullOrWhiteSpace(payload))
            {
                Logger.Error("SunPayPayinCallback===Payload 为空，忽略回调");
                return;
            }

            JObject root;
            try
            {
                root = JsonConvert.DeserializeObject<JObject>(payload);
            }
            catch (Exception ex)
            {
                Logger.Error($"SunPayPayinCallback===反序列化失败===Error={ex.Message}", ex);
                return;
            }

            var bizStatus = root?["biz_status"]?.ToString();
            var bizType = root?["biz_type"]?.ToString();
            var data = root?["data"] as JObject;

            if (data == null)
            {
                Logger.Error("SunPayPayinCallback===data 为空，忽略回调");
                return;
            }

            var channelOrderNo = data["order_no"]?.ToString();      // 渠道订单号
            var outOrderNo = data["out_order_no"]?.ToString();      // 商户侧订单号
            var outUserId = data["out_user_id"]?.ToString();
            var amount = data["amount"]?.ToObject<decimal?>() ?? 0m;
            var currency = data["currency"]?.ToString();
            var name = data["name"]?.ToString();

            if (string.IsNullOrEmpty(channelOrderNo) && string.IsNullOrEmpty(outOrderNo))
            {
                Logger.Error("SunPayPayinCallback===order_no 和 out_order_no 均为空，无法定位订单");
                return;
            }

            var lockKey = $"Lock:SunPayPayin:{channelOrderNo ?? outOrderNo}";
            using (await _distributedLockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10)))
            {
                using (var uow = _unitOfWorkManager.Begin())
                {
                    // 1. 优先按渠道订单号查找订单，其次按平台/商户订单号查找
                    CollectionOrder order = null;

                    if (!string.IsNullOrEmpty(channelOrderNo))
                    {
                        order = await _collectionOrderRepository.FirstOrDefaultAsync(
                            x => x.ChannelOrderNo == channelOrderNo);
                    }

                    if (order == null && !string.IsNullOrEmpty(outOrderNo))
                    {
                        // 当前仅按 PlatformOrderNo 进行匹配，后续如果在 CollectionOrder 中新增 MerchantOrderNo 字段，可改为按商户订单号查询
                        order = await _collectionOrderRepository.FirstOrDefaultAsync(
                            x => x.PlatformOrderNo == outOrderNo);
                    }

                    if (order == null)
                    {
                        // 暂时不自动建单，避免脏数据，记录告警日志方便排查
                        Logger.Warn($"SunPayPayinCallback===未找到对应的 CollectionOrder===ChannelOrderNo={channelOrderNo}===OutOrderNo={outOrderNo}===BizStatus={bizStatus}===BizType={bizType}");
                        return;
                    }

                    // 2. 根据 SunPay biz_status 映射平台订单状态
                    var newStatus = MapBizStatusToOrderStatus(bizStatus);
                    order.OrderStatus = newStatus;

                    // 3. 同步关键字段（防止创建阶段缺失）
                    if (!string.IsNullOrEmpty(channelOrderNo))
                    {
                        order.ChannelOrderNo = channelOrderNo;
                    }

                    if (!string.IsNullOrEmpty(currency))
                    {
                        order.Currency = currency;
                    }

                    if (amount > 0 && order.Amount <= 0)
                    {
                        order.Amount = amount;
                    }

                    if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(order.Remark))
                    {
                        order.Remark = name;
                    }

                    await _collectionOrderRepository.UpdateAsync(order);

                    // 4. 写入 / 更新订单明细，保存完整回调 JSON，方便审计和排查
                    var detail = await _collectionOrderDetailRepository.FirstOrDefaultAsync(d => d.CollectionOrderId == order.Id);
                    if (detail == null)
                    {
                        detail = new CollectionOrderDetail
                        {
                            Id = Guid.NewGuid(),
                            CollectionOrderId = order.Id,
                            Callback = payload
                        };
                        await _collectionOrderDetailRepository.InsertAsync(detail);
                    }
                    else
                    {
                        detail.Callback = payload;
                        await _collectionOrderDetailRepository.UpdateAsync(detail);
                    }

                    await uow.CompleteAsync();

                    Logger.Info($"SunPayPayinCallback===处理成功===OrderId={order.Id}===Status={order.OrderStatus}===BizStatus={bizStatus}===BizType={bizType}");
                }
            }
        }

        /// <summary>
        /// 将 SunPay biz_status 映射为平台订单状态
        /// 复用已有 CollectionOrderStatus 枚举，避免重复定义状态枚举
        /// </summary>
        /// <param name="bizStatus">SunPay 业务状态字符串</param>
        /// <returns>平台统一订单状态</returns>
        private CollectionOrderStatus MapBizStatusToOrderStatus(string bizStatus)
        {
            if (string.IsNullOrWhiteSpace(bizStatus))
            {
                return CollectionOrderStatus.Pending;
            }

            var statusUpper = bizStatus.Trim().ToUpperInvariant();

            switch (statusUpper)
            {
                case "SUCCESS":
                    return CollectionOrderStatus.Success;

                case "FAILED":
                case "FAIL":
                    return CollectionOrderStatus.Failed;

                case "CANCELLED":
                case "CANCELED":
                    return CollectionOrderStatus.Cancelled;

                case "PENDING":
                case "PROCESSING":
                default:
                    return CollectionOrderStatus.Pending;
            }
        }

        #endregion
    }
}

