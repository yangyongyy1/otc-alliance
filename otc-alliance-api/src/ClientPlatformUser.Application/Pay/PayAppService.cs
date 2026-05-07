using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rebus.Bus;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.Authorization.Users;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Extensions;
using ClientPlatform.Kyc;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Channels;
using ClientPlatform.Pay.Channels.SunPay.Builders;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.Pay.Entities;
using ClientPlatform.Pay.Managers;
using ClientPlatform.Pay.Mappers;
using ClientPlatform.Pay.Models;
using ClientPlatform.UserManagement;

namespace ClientPlatformUser
{
    /// <summary>
    /// Pay 测试接口服务
    /// </summary>
    public class PayAppService : AppServiceBase
    {

        private readonly IPayClient _payClient;
        private readonly IVARoutingManager _routingManager;
        private readonly SunPayVaBuilderFactory _sunPayVaBuilderFactory;
        private readonly IOptions<PayClientOptions> _payClientOptions;
        private readonly SunPayChannelProvider _sunPayChannelProvider;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<UserPayChannelCustomer, Guid> _payChannelCustomerRepository;
        private readonly IRepository<UserPayChannelAccount, Guid> _payChannelAccountRepository;
        private readonly IRepository<PayChannelRequestLog, Guid> _payChannelRequestLogRepository;
        private readonly IRepository<KycApplicant, System.Guid> _kycApplicantRepository;
        private readonly VaCreationRequestMapper _vaMapper;
        private readonly IRepository<ClientPlatform.AllianceManagement.MerchantChannelCurrency, int> _merchantChannelCurrencyRepository;
        private readonly IRepository<PayChannelAccountPaymentMethod, Guid> _payChannelAccountPaymentMethodRepository;
        private readonly IRepository<PayChannelServiceRequest, Guid> _payChannelServiceRequestRepository;
        private readonly Rebus.Bus.IBus _bus;

        public PayAppService(
            IPayClient payClient,
            IVARoutingManager routingManager,
            SunPayVaBuilderFactory sunPayVaBuilderFactory,
            IOptions<PayClientOptions> payClientOptions,
            SunPayChannelProvider sunPayChannelProvider,
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<UserPayChannelCustomer, Guid> payChannelCustomerRepository,
            IRepository<UserPayChannelAccount, Guid> payChannelAccountRepository,
            IRepository<PayChannelRequestLog, Guid> payChannelRequestLogRepository,
            IRepository<KycApplicant, System.Guid> kycApplicantRepository,
            VaCreationRequestMapper vaMapper,
            // IRepository<CountryInfo, int> countryInfoRepository, // Removed
            IRepository<ClientPlatform.AllianceManagement.MerchantChannelCurrency, int> merchantChannelCurrencyRepository,
            IRepository<PayChannelAccountPaymentMethod, Guid> payChannelAccountPaymentMethodRepository,
            IRepository<PayChannelServiceRequest, Guid> payChannelServiceRequestRepository,
            Rebus.Bus.IBus bus)
        {
            _payClient = payClient;
            _routingManager = routingManager;
            _sunPayVaBuilderFactory = sunPayVaBuilderFactory;
            _payClientOptions = payClientOptions;
            _sunPayChannelProvider = sunPayChannelProvider;
            _clientUserRepository = clientUserRepository;
            _payChannelCustomerRepository = payChannelCustomerRepository;
            _payChannelAccountRepository = payChannelAccountRepository;
            _payChannelRequestLogRepository = payChannelRequestLogRepository;
            _kycApplicantRepository = kycApplicantRepository;
            _vaMapper = vaMapper;
            // _countryInfoRepository = countryInfoRepository; // Removed
            _merchantChannelCurrencyRepository = merchantChannelCurrencyRepository;
            _payChannelAccountPaymentMethodRepository = payChannelAccountPaymentMethodRepository;
            _payChannelServiceRequestRepository = payChannelServiceRequestRepository;
            _bus = bus;
        }


        // ... (Existing methods omitted for brevity)

        // Find GetCurrentMerchantIdAsync and other methods...
        // Replacing CreateAccount and adding new methods below
        /// <summary>
        /// 获取虚拟账户开户所需字段 (合并客户与账户字段)
        /// </summary>
        [HttpGet]
        public async Task<PayApiResponse<List<ChannelRequiredFieldResponse>>> GetCreateAccountRequiredFields(string currency, Guid? requestId = null)
        {
            try
            {
                var userIdResult = await GetCurrentUserIdAsync();
                if (!userIdResult.HasValue) return new PayApiResponse<List<ChannelRequiredFieldResponse>> { IsSuccess = false, Code = 401, Msg = "No user logged in" };

                var clientUser = await _clientUserRepository.GetAllIncluding(x => x.Merchant).FirstOrDefaultAsync(x => x.Id == userIdResult.Value);
                if (clientUser?.Merchant == null) return new PayApiResponse<List<ChannelRequiredFieldResponse>> { IsSuccess = false, Code = 400, Msg = "Merchant not found" };

                // 1. Get Route
                var route = await _routingManager.GetRouteAsync(clientUser.MerchantId.Value, currency);
                if (string.IsNullOrEmpty(route?.ChannelCode))
                    return new PayApiResponse<List<ChannelRequiredFieldResponse>> { IsSuccess = false, Code = 400, Msg = $"No route found for {currency}" };

                var merchantOption = new PayMerchantOption { MerchantKey = clientUser.Merchant.BusinessID, MerchantSecret = clientUser.Merchant.Key };

                // 2. 检查是否已有活跃账户
                var internalUserId = clientUser.Id;
                // 4.1 Check Existing Active or Pending Account
                var existingAccount = await _payChannelAccountRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.UserId == internalUserId &&
                                   x.Currency == currency &&
                                  x.ChannelProvider == route.ChannelCode  &&
                                   (x.Status == VAStatus.Active || x.Status == VAStatus.Pending)); 

                if (existingAccount != null)
                {
                    if (existingAccount.Status == VAStatus.Active)
                    {
                        return new PayApiResponse<List<ChannelRequiredFieldResponse>>
                        {
                            IsSuccess = false,
                            Code = 409,
                            Msg = "Active account already exists for this currency.",
                            Data = null!
                        };
                    }

                    if (existingAccount.Status == VAStatus.Pending)
                    {
                        return new PayApiResponse<List<ChannelRequiredFieldResponse>>
                        {
                            IsSuccess = false,
                            Code = 423,
                            Msg = "Account creation is in progress (Pending). Please wait.",
                            Data = null!
                        };
                    }
                }

                // 4. 检查是否有正在进行的请求 (防止重复提交)
                var existingRequest = await _payChannelServiceRequestRepository.GetAll()
                    .FirstOrDefaultAsync(r => r.UserId == internalUserId &&
                                         r.Currency == currency &&
                                         r.ChannelProvider == route.ChannelCode &&
                                         r.Status != PayChannelServiceRequestStatus.Completed &&
                                         r.Status != PayChannelServiceRequestStatus.Failed);

                if (existingRequest != null)
                {
                    return new PayApiResponse<List<ChannelRequiredFieldResponse>>
                    {
                        IsSuccess = false,
                        Code = 435,
                        Msg = "Account creation is in progress (Pending). Please wait.",
                    };
                }

                // [New Logic]: 尝试从失败记录恢复数据
                // 如果是新建（requestId 为空），尝试查找最近的失败记录
                if (requestId == null)
                {
                    var lastFailedRequest = await _payChannelServiceRequestRepository.GetAll()
                        .Where(x => x.UserId == userIdResult.Value &&
                                    x.ChannelProvider == route.ChannelCode &&
                                    x.Currency == currency &&
                                    x.Status == PayChannelServiceRequestStatus.Failed)
                        .OrderByDescending(x => x.CreationTime)
                        .FirstOrDefaultAsync();

                    if (lastFailedRequest != null)
                    {
                        requestId = lastFailedRequest.Id;
                    }
                }

                // 4. Get Account Fields from Provider
                var accountFieldDefs = await _sunPayChannelProvider.GetRequiredFieldsAsync(currency, route.ChannelCode, merchantOption);

                // 5. Define Customer Fields (Standard set)
                var customerFields = new List<FieldDefinition>
                {
                    new FieldDefinition { Key = "email", Label = "Email", Required = true, Type = "text" }, // Email moved to top
                    // Currency：统一隐藏（所有渠道一致），值由后端填充
                    new FieldDefinition { Key = "currency", Label = "Currency", Required = true, Display = false }, // Value set below
                    new FieldDefinition { Key = "first_name", Label = "First Name", Required = true, Type = "text" },
                    new FieldDefinition { Key = "middle_name", Label = "Middle Name", Required = false, Type = "text" },
                    new FieldDefinition { Key = "last_name", Label = "Last Name", Required = true, Type = "text" },
                    new FieldDefinition { Key = "birth_date", Label = "Date of Birth", Type = "date", Required = true },
                    new FieldDefinition { Key = "nationality", Label = "Nationality", Type = "select", Required = true },
                    new FieldDefinition { Key = "country_code", Label = "Residence Country", Type = "select", Required = true },
                    new FieldDefinition { Key = "city", Label = "City", Required = true, Type = "text" },
                    new FieldDefinition { Key = "address_line", Label = "Address", Required = true, Type = "text" },
                    new FieldDefinition { Key = "post_code", Label = "Post Code", Required = true, Type = "text" }
                };

                // 4. Merge (Account Fields overwrite Customer Fields if same key, but usually they are distinct)
                var mergedMap = new Dictionary<string, ChannelRequiredFieldResponse>();

                // Helper to map definition to response
                ChannelRequiredFieldResponse MapToResponse(FieldDefinition def)
                {
                    return new ChannelRequiredFieldResponse
                    {
                        FieldName = def.Key,
                        Description = def.Description ?? def.Label, // Use Description if available, else Label
                        Label = def.Label, // Ensure Label is set (Name property relies on this)
                        Type = def.Type,
                        Required = def.Required,
                        Regex = def.RegexPattern,
                        ValidationMessage = def.ValidationMessage,
                        Options = def.Options?.Select(o => new ChannelRequiredFieldResponseValue { Key = o.Value, Value = o.Label }).ToList(),
                        Display = true
                    };
                }

                // Add Customer Fields first
                foreach (var f in customerFields) mergedMap[f.Key] = MapToResponse(f);

                // [Solution Fix]: Set Currency Value and Readonly
                if (mergedMap.TryGetValue("currency", out var currencyField))
                {
                    currencyField.Value = currency;
                    currencyField.Readonly = true;
                    currencyField.Display = false;
                }

                // Add/Override Account Fields
                foreach (var f in accountFieldDefs) mergedMap[f.Key] = MapToResponse(f);

                var resultList = mergedMap.Values.ToList();

                // 5. Populate Defaults from KYC
                // Need to fetch KYC data first
                var kycApplicant = await _kycApplicantRepository.GetAll()
                    .Where(x => x.UserId == clientUser.Id && x.KycBizStatus == KycBizStatus.APPROVED && !x.IsClosed)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (kycApplicant != null)
                {
                    // 从 KYC 记录填充默认值（包括币种、邮箱、姓名等）
                    PopulateDefaultValuesFromKyc(resultList, kycApplicant, currency);
                }

                // 6. Populate Dropdown Options (e.g. Countries)
                await PopulateCountryOptionsAsync(resultList);

                // 7. [Solution B] If requestId is provided (Retry flow OR Recovered flow), overwrite with values from Request Log
                if (requestId.HasValue)
                {
                    // Unified logic: populate from log using the ID (whether passed in or found above)
                    await PopulateValuesFromRequestLog(resultList, requestId.Value);

                    // Add RequestId as a hidden field so frontend knows it (for both recovered and explicit cases)
                    resultList.Add(new ChannelRequiredFieldResponse
                    {
                        FieldName = "requestId",
                        Value = requestId.ToString(),
                        Display = false,
                        Readonly = true,
                        Required = false,
                        Type = ChannelTypeName.Text
                    });
                }

                // 8. 最终兜底：确保币种和邮箱有默认值（防止被渠道字段或请求日志覆盖为空）
                var finalCurrencyField = resultList.FirstOrDefault(f => f.FieldName == "currency");
                if (finalCurrencyField != null)
                {
                    var currencyValue = finalCurrencyField.Value as string;
                    if (string.IsNullOrWhiteSpace(currencyValue))
                    {
                        finalCurrencyField.Value = currency;
                    }
                    finalCurrencyField.Readonly = true;
                    finalCurrencyField.Display = false;
                }

                var finalEmailField = resultList.FirstOrDefault(f => f.FieldName == "email");
                if (finalEmailField != null)
                {
                    var emailValue = finalEmailField.Value as string;
                    if (string.IsNullOrWhiteSpace(emailValue))
                    {
                        finalEmailField.Value = clientUser.Email;
                        finalEmailField.Readonly = true;
                    }
                }

                return new PayApiResponse<List<ChannelRequiredFieldResponse>>
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = resultList
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"GetRequiredFields Error: {ex.Message}", ex);
                return new PayApiResponse<List<ChannelRequiredFieldResponse>> { IsSuccess = false, Code = 500, Msg = ex.Message };
            }
        }

        [HttpPost]
        public async Task<PayApiResponse<CreateUserAccountDto>> CreateAccount([FromBody] CreateAccountInput input)
        {
            try
            {
                var userIdResult = await GetCurrentUserIdAsync();
                if (!userIdResult.HasValue)
                {
                    return new PayApiResponse<CreateUserAccountDto> { IsSuccess = false, Code = 401, Msg = "No user logged in" };
                }
                var userId = userIdResult.Value;

                var clientUser = await _clientUserRepository.GetAllIncluding(x => x.Merchant)
                        .FirstOrDefaultAsync(x => x.Id == userId);

                if (clientUser == null || clientUser.MerchantId == null)
                {
                    return new PayApiResponse<CreateUserAccountDto> { IsSuccess = false, Code = 400, Msg = "User or Merchant not found." };
                }
                var internalUserId = clientUser.Id;

                // 1. 检查 KYC 状态
                var kycApplicant = await _kycApplicantRepository.GetAll()
                    .Where(x => x.UserId == internalUserId && x.KycBizStatus == KycBizStatus.APPROVED && !x.IsClosed)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (kycApplicant == null || kycApplicant.KycBizStatus != KycBizStatus.APPROVED)
                {
                    return new PayApiResponse<CreateUserAccountDto> { IsSuccess = false, Code = 400, Msg = L(ErrorCodes.Kyc.NotApproved), Data = null! };
                }

                // 2. 路由校验
                var route = await _routingManager.GetRouteAsync(clientUser.MerchantId.Value, input.Currency);
                Logger.Info($"CreateAccount===GetRouteAsync===CustomerId={clientUser.Id}===MerchantId={clientUser.MerchantId.Value}==={route.ChannelCode}");
                if (string.IsNullOrEmpty(route?.ChannelCode))
                {
                    return new PayApiResponse<CreateUserAccountDto> { IsSuccess = false, Code = 400, Msg = $"No route found for currency {input.Currency}" };
                }

                // 3. 检查是否已有活跃账户
                var existingAccount = await _payChannelAccountRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.UserId == internalUserId &&
                                   x.ChannelProvider == route.ChannelCode &&
                                   x.Currency == input.Currency &&
                                   (x.Status == VAStatus.Active || x.Status == VAStatus.Pending));

                if (existingAccount != null)
                {
                    var msg = existingAccount.Status == VAStatus.Active
                        ? "Active account already exists for this currency."
                        : "Account creation is in progress (Pending). Please wait.";
                    var code = existingAccount.Status == VAStatus.Active ? 409 : 423;

                    return new PayApiResponse<CreateUserAccountDto>
                    {
                        IsSuccess = false,
                        Code = code,
                        Msg = msg,
                        Data = null!
                    };
                }

                // 4. 检查是否有正在进行的请求 (防止重复提交)
                var existingRequest = await _payChannelServiceRequestRepository.GetAll()
                    .FirstOrDefaultAsync(r => r.UserId == internalUserId &&
                                         r.Currency == input.Currency &&
                                         r.ChannelProvider == route.ChannelCode &&
                                         r.Status != PayChannelServiceRequestStatus.Completed &&
                                         r.Status != PayChannelServiceRequestStatus.Failed);

                if (existingRequest != null)
                {
                    return new PayApiResponse<CreateUserAccountDto>
                    {
                        IsSuccess = false,
                        Code = 435,
                        Msg = "Account creation is in progress (Pending). Please wait.",
                        Data = new CreateUserAccountDto
                        {
                            RequestId = existingRequest.Id,
                            Status = existingRequest.Status.ToString(),
                            Currency = existingRequest.Currency
                        }
                    };
                }

                // 5. 格式化数据 (Input format)
                if (!string.IsNullOrEmpty(input.BirthDate) && input.BirthDate.Length == 10) input.BirthDate += " 00:00:00";
                if (!string.IsNullOrEmpty(input.BusinessPersonBirthDate) && input.BusinessPersonBirthDate.Length == 10) input.BusinessPersonBirthDate += " 00:00:00";

                // 6. 创建跟踪记录
                // 6. Check for Existing Active Customer (Reuse Strategy)
                var activeCustomer = await _payChannelCustomerRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.UserId == internalUserId && x.Status == PayChannelCustomerStatus.Active);

                var initialStatus = activeCustomer != null && !string.IsNullOrEmpty(activeCustomer.ChannelCustomerId)
                    ? PayChannelServiceRequestStatus.PendingAccount
                    : PayChannelServiceRequestStatus.PendingCustomer;

                var customerId = activeCustomer?.ChannelCustomerId;

                // 7. Create Request Record
                var request = new PayChannelServiceRequest
                {
                    UserId = internalUserId,
                    Currency = input.Currency,
                    ChannelProvider = route.ChannelCode,
                    RequestPayload = JsonConvert.SerializeObject(input),
                    Status = initialStatus,
                    CustomerId = customerId,
                    FailStep = PayChannelServiceRequestFailStep.None,
                    RetryCount = 0
                };
                await _payChannelServiceRequestRepository.InsertAsync(request);
                await CurrentUnitOfWork.SaveChangesAsync(); // Get ID

                // 8. Publish Event based on Status (AFTER COMMIT)
                if (initialStatus == PayChannelServiceRequestStatus.PendingAccount)
                {
                    CurrentUnitOfWork.Completed += (sender, args) =>
                    {
                        AsyncHelper.RunSync(() => _bus.Publish(new ClientPlatform.Pay.Dto.CreatePayChannelAccountEto
                        {
                            RequestId = request.Id
                        }));
                    };
                    Logger.Info($"CreateAccount===Reusing Active Customer===CustomerId={customerId}===RequestId={request.Id}");
                }
                else
                {
                    CurrentUnitOfWork.Completed += (sender, args) =>
                    {
                        AsyncHelper.RunSync(() => _bus.Publish(new ClientPlatform.Pay.Dto.CreatePayChannelCustomerEto
                        {
                            UserId = internalUserId,
                            KycApplicantId = kycApplicant.Id,
                            RequestId = request.Id
                        }));
                    };
                    Logger.Info($"CreateAccount===Starting New Customer Flow===RequestId={request.Id}");
                }

                return new PayApiResponse<CreateUserAccountDto>
                {
                    IsSuccess = true,
                    Code = 202,
                    Msg = "Request accepted",
                    Data = new CreateUserAccountDto
                    {
                        RequestId = request.Id,
                        Status = request.Status.ToString(),
                        Currency = request.Currency
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"CreateAccount failed: {ex.Message}", ex);
                return new PayApiResponse<CreateUserAccountDto> { IsSuccess = false, Code = 500, Msg = ex.Message };
            }
        }

        /// <summary>
        /// 查询创建状态
        /// </summary>
        [HttpGet]
        public async Task<PayApiResponse<PayChannelServiceRequestDto>> GetCreationStatus(Guid requestId)
        {
            var request = await _payChannelServiceRequestRepository.FirstOrDefaultAsync(requestId);
            if (request == null)
            {
                return new PayApiResponse<PayChannelServiceRequestDto>
                {
                    IsSuccess = false,
                    Code = 404,
                    Msg = "Request not found"
                };
            }

            // Zombie Check (Passive): If processing > 30 mins, mark locally as Failed? 
            // Or just rely on display status. 
            // Better to trigger a background job or allow manual retry.
            // Here we just return the status.

            var dto = ObjectMapper.Map<PayChannelServiceRequestDto>(request);



            // 2. Map Fail Reason
            if (request.Status == PayChannelServiceRequestStatus.Failed)
            {
                dto.FailReason = MapErrorMessage(request.FailReason);
            }

            // 3. Populate Request Data for Retry
            dto.RequestData = request.RequestPayload;

            return new PayApiResponse<PayChannelServiceRequestDto>
            {
                IsSuccess = true,
                Code = 200,
                Data = dto
            };
        }

        private string MapToDisplayStatus(PayChannelServiceRequestStatus status)
        {
            switch (status)
            {
                case PayChannelServiceRequestStatus.PendingCustomer:
                case PayChannelServiceRequestStatus.CustomerProcessing:
                case PayChannelServiceRequestStatus.PendingAccount:
                case PayChannelServiceRequestStatus.AccountProcessing:
                    return "Processing";
                case PayChannelServiceRequestStatus.Completed:
                    return "Success";
                case PayChannelServiceRequestStatus.Failed:
                    return "Failed";
                default:
                    return "Processing";
            }
        }

        private string MapErrorMessage(string internalError)
        {
            if (string.IsNullOrEmpty(internalError)) return "Unknown Error";
            // Basic User Friendly Mapping
            if (internalError.Contains("duplicate", StringComparison.OrdinalIgnoreCase)) return "Duplicate Request";
            if (internalError.Contains("timeout", StringComparison.OrdinalIgnoreCase)) return "Request Timed Out";
            if (internalError.Contains("kyc", StringComparison.OrdinalIgnoreCase)) return "KYC Verification Issue";

            return internalError;
        }

        /// <summary>
        /// 重试创建 (支持修改参数)
        /// </summary>
        [HttpPost]
        public async Task<PayApiResponse<string>> RetryCreation([FromBody] RetryCreationInput input)
        {
            var request = await _payChannelServiceRequestRepository.FirstOrDefaultAsync(input.RequestId);
            if (request == null)
            {
                return new PayApiResponse<string> { IsSuccess = false, Code = 404, Msg = "Request not found" };
            }

            if (request.Status == PayChannelServiceRequestStatus.Completed)
            {
                return new PayApiResponse<string> { IsSuccess = false, Code = 400, Msg = "Request already completed" };
            }

            var userIdResult = await GetCurrentUserIdAsync();
            if (!userIdResult.HasValue)
            {
                return new PayApiResponse<string> { IsSuccess = false, Code = 401, Msg = "No user logged in" };
            }
            var userId = userIdResult.Value;

            var clientUser = await _clientUserRepository.GetAllIncluding(x => x.Merchant)
                    .FirstOrDefaultAsync(x => x.Id == userId);

            if (clientUser == null || clientUser.MerchantId == null)
            {
                return new PayApiResponse<string> { IsSuccess = false, Code = 400, Msg = "User or Merchant not found." };
            }

            var internalUserId = clientUser.Id;

            // 1. 检查 KYC 状态
            var kycApplicant = await _kycApplicantRepository.GetAll()
                .Where(x => x.UserId == internalUserId && x.KycBizStatus == KycBizStatus.APPROVED && !x.IsClosed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (kycApplicant == null || kycApplicant.KycBizStatus != KycBizStatus.APPROVED)
            {
                return new PayApiResponse<string> { IsSuccess = false, Code = 400, Msg = L(ErrorCodes.Kyc.NotApproved), Data = null! };
            }

            var existingAccount = await _payChannelAccountRepository.GetAll()
           .FirstOrDefaultAsync(x => x.UserId == internalUserId &&
                   x.Currency == input.Currency &&
                   x.ChannelProvider == request.ChannelProvider &&
                   (x.Status == VAStatus.Active || x.Status == VAStatus.Pending));

            if (existingAccount != null)
            {
                if (existingAccount.Status == VAStatus.Active)
                {
                    return new PayApiResponse<string>
                    {
                        IsSuccess = false,
                        Code = 409,
                        Msg = "Active account already exists for this currency.",
                        Data = null!
                    };
                }

                if (existingAccount.Status == VAStatus.Pending)
                {
                    return new PayApiResponse<string>
                    {
                        IsSuccess = false,
                        Code = 423,
                        Msg = "Account creation is in progress (Pending). Please wait.",
                        Data = null!
                    };
                }
            }


            // 4. 检查是否有正在进行的请求 (防止重复提交)
            var existingRequest = await _payChannelServiceRequestRepository.GetAll()
                .FirstOrDefaultAsync(r => r.UserId == internalUserId &&
                                     r.Currency == input.Currency &&
                                     r.ChannelProvider == request.ChannelProvider &&
                                     r.Status != PayChannelServiceRequestStatus.Completed &&
                                     r.Status != PayChannelServiceRequestStatus.Failed);

            if (existingRequest != null)
            {
                return new PayApiResponse<string>
                {
                    IsSuccess = false,
                    Code = 435,
                    Msg = "Account creation is in progress (Pending). Please wait.",
                };
            }

            // 5. 格式化数据 (Input format)
            if (!string.IsNullOrEmpty(input.BirthDate) && input.BirthDate.Length == 10) input.BirthDate += " 00:00:00";
            if (!string.IsNullOrEmpty(input.BusinessPersonBirthDate) && input.BusinessPersonBirthDate.Length == 10) input.BusinessPersonBirthDate += " 00:00:00";

            // 更新参数
            request.RequestPayload = JsonConvert.SerializeObject(input);
            request.RetryCount++;
            request.FailReason = null; // Clear error

            // 决定如何重启
            if (request.FailStep == PayChannelServiceRequestFailStep.Customer)
            {
                request.Status = PayChannelServiceRequestStatus.PendingCustomer;
                var kyc = await _kycApplicantRepository.GetAll()
                    .Where(x => x.UserId == request.UserId && x.KycBizStatus == KycBizStatus.APPROVED && !x.IsClosed)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
                if (kyc == null) return new PayApiResponse<string> { IsSuccess = false, Code = 500, Msg = "KYC not found" };

                await _payChannelServiceRequestRepository.UpdateAsync(request);
                CurrentUnitOfWork.Completed += (sender, args) =>
                {
                    AsyncHelper.RunSync(() => _bus.Publish(new ClientPlatform.Pay.Dto.CreatePayChannelCustomerEto
                    {
                        UserId = request.UserId,
                        KycApplicantId = kyc.Id,
                        RequestId = request.Id
                    }));
                };
            }
            else if (request.FailStep == PayChannelServiceRequestFailStep.Account)
            {
                request.Status = PayChannelServiceRequestStatus.PendingAccount; // Ready for Account
                await _payChannelServiceRequestRepository.UpdateAsync(request);
                CurrentUnitOfWork.Completed += (sender, args) =>
                {
                    AsyncHelper.RunSync(() => _bus.Publish(new ClientPlatform.Pay.Dto.CreatePayChannelAccountEto
                    {
                        RequestId = request.Id
                    }));
                };
            }
            else
            {
                // 如果是手动重试但没有明确失败步骤，假设从头开始？或者根据当前状态？
                // 如果状态是 Failed 但 FailStep None... 通常不该发生。
                // 默认从头开始
                request.Status = PayChannelServiceRequestStatus.PendingCustomer;
                var kyc = await _kycApplicantRepository.GetAll()
                    .Where(x => x.UserId == request.UserId && x.KycBizStatus == KycBizStatus.APPROVED && !x.IsClosed)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
                if (kyc == null) return new PayApiResponse<string> { IsSuccess = false, Code = 500, Msg = "KYC not found" };

                await _payChannelServiceRequestRepository.UpdateAsync(request);
                await _bus.Publish(new ClientPlatform.Pay.Dto.CreatePayChannelCustomerEto
                {
                    UserId = request.UserId,
                    KycApplicantId = kyc.Id,
                    RequestId = request.Id
                });
            }

            return new PayApiResponse<string> { IsSuccess = true, Code = 200, Msg = "Retry initiated" };
        }

        /// <summary>
        /// 获取当前可用币种列表
        /// </summary>
        [HttpGet]
        public async Task<PayApiResponse<List<string>>> GetSupportedCurrencies()
        {
            var merchantId = await GetCurrentMerchantIdAsync();
            if (merchantId == null)
            {
                return new PayApiResponse<List<string>> { IsSuccess = false, Code = 401, Msg = "Merchant not found" };
            }

            var currencies = await _merchantChannelCurrencyRepository.GetAll()
                .Where(x => x.MerchantId == merchantId.Value && x.OpenClose == OpenClose.Open)
                .Select(x => x.Currency)
                .Distinct()
                .ToListAsync();

            return new PayApiResponse<List<string>>
            {
                IsSuccess = true,
                Code = 200,
                Data = currencies
            };
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var clientUser = await _clientUserRepository.GetAllIncluding(x => x.Merchant)
                   .FirstOrDefaultAsync(x => x.AbpUserId == AbpSession.UserId);

            if (clientUser == null || clientUser.MerchantId == null)
            {
                throw new UserFriendlyException("No user logged in (AbpSession.UserId is null)");
            }
            return clientUser.Id;
        }

        [HttpGet]
        public async Task<int?> GetCurrentMerchantIdAsync()
        {
            var userId = AbpSession.UserId;
            if (!userId.HasValue) return null;
            var user = await _clientUserRepository.GetAllIncluding(x => x.Merchant).FirstOrDefaultAsync(x => x.AbpUserId == userId.Value);
            return user?.MerchantId;
        }

        /// <summary>
        /// 获取当前用户的进件（开户）申请记录
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<PayChannelServiceRequestDto>> GetCreationRequests(PagedResultRequestDto input)
        {
            var userId = await GetCurrentUserIdAsync();
            if (!userId.HasValue) return new PagedResultDto<PayChannelServiceRequestDto>(0, new List<PayChannelServiceRequestDto>());

            var query = _payChannelServiceRequestRepository.GetAll()
                .Where(x => x.UserId == userId.Value);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreationTime)
                .PageBy(input)
                .ToListAsync();

            var dtos = items.Select(x =>
            {
                var dto = new PayChannelServiceRequestDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Currency = x.Currency,
                    RequestPayload = NormalizeDateFieldsInPayload(x.RequestPayload), // 清理日期格式
                    Status = x.Status,
                    FailStep = x.FailStep,
                    FailReason = x.FailReason,
                    CustomerId = x.CustomerId,
                    AccountId = x.AccountId,
                    RetryCount = x.RetryCount,
                    CreationTime = x.CreationTime
                };


                if (x.Status == PayChannelServiceRequestStatus.Failed)
                {
                    dto.FailReason = MapErrorMessage(x.FailReason);
                }
                return dto;
            }).ToList();

            return new PagedResultDto<PayChannelServiceRequestDto>(totalCount, dtos);
        }


        /// <summary>
        /// 获取当前用户的所有支付渠道账户
        /// </summary>
        [HttpGet]
        public async Task<PayApiResponse<List<PayChannelAccountDto>>> GetPayChannelAccounts()
        {
            var abpUserId = AbpSession.UserId;
            if (!abpUserId.HasValue)
            {
                throw new UserFriendlyException("No user logged in (AbpSession.UserId is null)");
            }

            // 1. 获取用户及其商户信息
            var clientUser = await _clientUserRepository.GetAllIncluding(x => x.Merchant)
                    .FirstOrDefaultAsync(x => x.AbpUserId == abpUserId.Value);

            if (clientUser == null || clientUser.MerchantId == null || clientUser.Merchant == null)
            {
                throw new UserFriendlyException("User or Merchant not found.");
            }
            var accounts = await _payChannelAccountRepository.GetAllListAsync(a => a.UserId == clientUser.Id &&
                                                                              (a.Status == VAStatus.Active ||
                                                                               a.Status == VAStatus.Suspended ||
                                                                               a.Status == VAStatus.Closed));

            var dtos = accounts.Select(a => new PayChannelAccountDto
            {
                Id = a.Id,
                ChannelAccountId = a.ChannelAccountId,
                ReferenceId = a.ReferenceId,
                Currency = a.Currency,
                AccountName = a.AccountName,
                AccountNumber = a.AccountNumber,
                BankName = a.BankName,
                Status = a.Status,
                RejectionReason = a.RejectionReason,
                CreationTime = a.CreationTime
            }).OrderByDescending(a => a.CreationTime).ToList();

            return new PayApiResponse<List<PayChannelAccountDto>>
            {
                Data = dtos,
                IsSuccess = true,
                Code = 200
            };
        }

        private bool IsAllNull(params string[] values)
        {
            return values.All(string.IsNullOrWhiteSpace);
        }

        /// <summary>
        /// 获取指定账户的详情及支付方式 (存款指引)
        /// </summary>
        [HttpGet]
        public async Task<PayApiResponse<PayChannelAccountDetailDto>> GetPayChannelAccountPaymentMethods(Guid accountId)
        {
            var abpUserId = AbpSession.UserId;
            if (!abpUserId.HasValue)
            {
                throw new UserFriendlyException("No user logged in (AbpSession.UserId is null)");
            }
            var account = await _payChannelAccountRepository.GetAll().Where(x => x.Id == accountId).FirstOrDefaultAsync();

            if (account == null)
            {
                throw new UserFriendlyException("Account not found");
            }

            // 1. 获取用户及其商户信息
            var clientUser = await _clientUserRepository.GetAllIncluding(x => x.Merchant)
                    .FirstOrDefaultAsync(x => x.AbpUserId == abpUserId.Value);

            if (clientUser == null || clientUser.MerchantId == null || clientUser.Merchant == null)
            {
                throw new UserFriendlyException("User or Merchant not found.");
            }

            var methods = await _payChannelAccountPaymentMethodRepository.GetAllListAsync(m => m.AccountId == accountId);


            // Determine Account Holder Name Priority:
            // 1. Account.AccountName (from Channel Callback)
            // 2. Request Payload (User Input)
            // 3. ClientUser.UserName (Fallback)
            string accountHolderName = account.AccountName;

            if (string.IsNullOrEmpty(accountHolderName))
            {
                // Try to find from request
                PayChannelServiceRequest request = null;

                // A. Match by ChannelAccountId
                if (!string.IsNullOrEmpty(account.ChannelAccountId))
                {
                    request = await _payChannelServiceRequestRepository.GetAll()
                        .OrderByDescending(r => r.CreationTime)
                        .FirstOrDefaultAsync(r => r.AccountId == account.ChannelAccountId);
                }

                // B. Match by UserId/Currency (Soft Link)
                if (request == null)
                {
                    request = await _payChannelServiceRequestRepository.GetAll()
                        .OrderByDescending(r => r.CreationTime)
                        .FirstOrDefaultAsync(r => r.UserId == account.UserId && r.Currency == account.Currency);
                }

                if (request != null && !string.IsNullOrEmpty(request.RequestPayload))
                {
                    try
                    {
                        var payload = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(request.RequestPayload);
                        if (payload != null)
                        {
                            Logger.Info($"GetPayChannelAccountPaymentMethods===Found Payload for Request {request.Id}===Payload Length={request.RequestPayload.Length}");

                            // Priority 2.1: account_name
                            if (payload.TryGetValue("account_name", StringComparison.OrdinalIgnoreCase, out var anToken) && !string.IsNullOrEmpty(anToken?.ToString()))
                            {
                                accountHolderName = anToken.ToString();
                            }
                            // Priority 2.2: company_name
                            else if (payload.TryGetValue("company_name", StringComparison.OrdinalIgnoreCase, out var cnToken) && !string.IsNullOrEmpty(cnToken?.ToString()))
                            {
                                accountHolderName = cnToken.ToString();
                            }
                            // Priority 2.3: First + Last Name
                            else
                            {
                                string first = payload.TryGetValue("first_name", StringComparison.OrdinalIgnoreCase, out var fToken) ? fToken.ToString() : "";
                                string middle = payload.TryGetValue("middle_name", StringComparison.OrdinalIgnoreCase, out var mToken) ? mToken.ToString() : "";
                                string last = payload.TryGetValue("last_name", StringComparison.OrdinalIgnoreCase, out var lToken) ? lToken.ToString() : "";

                                Logger.Info($"GetPayChannelAccountPaymentMethods===Extracted Names===First={first}===Middle={middle}===Last={last}");

                                var nameParts = new[] { first, middle, last }.Where(s => !string.IsNullOrWhiteSpace(s));
                                if (nameParts.Any())
                                {
                                    accountHolderName = string.Join(" ", nameParts);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Failed to parse RequestPayload for AccountHolderName: {ex.Message}");
                    }
                }
                else
                {
                    Logger.Warn($"GetPayChannelAccountPaymentMethods===Request not found or payload empty===AccountId={account.ChannelAccountId}===UserId={account.UserId}");
                }
            }

            // Priority 3: Fallback
            if (string.IsNullOrEmpty(accountHolderName))
            {
                accountHolderName = clientUser.UserName;
            }

            var result = new PayChannelAccountDetailDto
            {
                Id = account.Id,
                Currency = account.Currency,
                Status = account.Status,
                ChannelAccountId = account.ChannelAccountId,
                AccountHolderName = accountHolderName,
                CreationTime = account.CreationTime
            };

            var methodDtos = methods.Select(m => new PayChannelAccountPaymentMethodDetailDto
            {
                Id = m.Id,
                PaymentType = m.PaymentType,
                AccountDetail = IsAllNull(m.AccountHolderName, m.AccountNumber, m.AccountRoutingNumber, m.Memo, m.SwiftBic, m.IntermediarySwiftBic)
                    ? null
                    : new PaymentMethodAccountDetailDto
                    {
                        AccountHolderName = m.AccountHolderName,
                        AccountNumber = m.AccountNumber,
                        AccountRoutingNumber = m.AccountRoutingNumber,
                        Memo = m.Memo,
                        // SwiftBic Logic: Clear if currency is EUR
                        SwiftBic = (account.Currency == "EUR") ? null : m.SwiftBic,
                        IntermediarySwiftBic = m.IntermediarySwiftBic
                    },
                InstitutionInformation = IsAllNull(m.InstitutionName, m.InstitutionAddressLine1, m.InstitutionCity, m.InstitutionState, m.InstitutionPostalCode, m.InstitutionCountryCode)
                    ? null
                    : new PaymentMethodInstitutionDto
                    {
                        Name = m.InstitutionName,
                        AddressLine1 = m.InstitutionAddressLine1,
                        City = m.InstitutionCity,
                        State = m.InstitutionState,
                        PostalCode = m.InstitutionPostalCode,
                        CountryCode = m.InstitutionCountryCode
                    },
                AccountHolderAddress = IsAllNull(m.HolderAddressLine1, m.HolderCity, m.HolderState, m.HolderPostalCode, m.HolderCountryCode)
                    ? null
                    : new PaymentMethodAddressDto
                    {
                        AddressLine1 = m.HolderAddressLine1,
                        City = m.HolderCity,
                        State = m.HolderState,
                        PostalCode = m.HolderPostalCode,
                        CountryCode = m.HolderCountryCode
                    }
            }).ToList();

            result.PaymentMethods = methodDtos;

            return new PayApiResponse<PayChannelAccountDetailDto>
            {
                Data = result,
                IsSuccess = true,
                Code = 200
            };
        }


        #region KYC Default Values Helper Methods

        /// <summary>
        /// 从 KYC 数据填充字段默认值
        /// </summary>
        /// <param name="fields">字段列表</param>
        /// <param name="kycApplicant">KYC 申请数据</param>
        /// <param name="currency">币种</param>
        private void PopulateDefaultValuesFromKyc(
            List<ChannelRequiredFieldResponse> fields,
            KycApplicant kycApplicant,
            string currency)
        {
            if (fields == null || fields.Count == 0) return;

            foreach (var field in fields)
            {
                // 填充默认值
                field.Value = GetDefaultValueForField(field.FieldName, kycApplicant, currency);

                // 设置只读字段（币种、邮箱、姓名）
                if (field.FieldName == "currency" ||
                    field.FieldName == "email" ||
                    field.FieldName == "first_name" ||
                    field.FieldName == "middle_name" ||
                    field.FieldName == "last_name")
                {
                    field.Readonly = true;
                }
                if (field.FieldName == "currency")
                {
                    // currency 字段的 Display 由上游合并逻辑统一控制（不同渠道显示策略不同）
                }
            }
        }

        /// <summary>
        /// 获取字段的默认值
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="kyc">KYC 数据</param>
        /// <param name="currency">币种</param>
        /// <returns>默认值</returns>
        private object GetDefaultValueForField(string fieldName, KycApplicant kyc, string currency)
        {
            if (kyc == null) return fieldName == "currency" ? currency : null;

            return fieldName switch
            {
                "currency" => currency,  // 币种使用输入参数
                "email" => kyc.Email,  // 邮箱（只读）
                "first_name" => kyc.FixedFirstName ?? kyc.FirstName,  // 优先使用修正后的值
                "middle_name" => kyc.FixedMiddleName ?? kyc.MiddleName,
                "last_name" => kyc.FixedLastName ?? kyc.LastName,
                "birth_date" => (kyc.FixedDob ?? kyc.DateOfBirth)?.ToString("yyyy-MM-dd"),
                "nationality" => ConvertToIso2CountryCode(kyc.Nationality),  // 国籍（ISO-2）
                "address_line" => kyc.Street ?? kyc.FormattedAddress,
                "city" => kyc.Town,
                "post_code" => kyc.PostCode,
                "country_code" => ConvertToIso2CountryCode(kyc.Country),  // 国家代码（ISO-2）
                _ => null
            };
        }

        /// <summary>
        /// 将国家代码转换为 ISO-2 格式 (2 字母)
        /// 支持输入：ISO-2 (DE), ISO-3 (DEU), 或完整名称
        /// </summary>

        /// <summary>
        /// 填充国家/国籍字段的选项列表
        /// </summary>
        private async Task PopulateCountryOptionsAsync(List<ChannelRequiredFieldResponse> fields)
        {
            if (fields == null || !fields.Any()) return;

            // 查找名为 "country" 或 "nationality" 的字段
            var countryFields = fields.Where(f =>
                f.Key.Equals("country", StringComparison.OrdinalIgnoreCase) ||
                f.Key.Equals("nationality", StringComparison.OrdinalIgnoreCase) ||
                f.Key.Equals("nationality", StringComparison.OrdinalIgnoreCase) ||
                (f.Name != null && f.Name.Contains("Country", StringComparison.OrdinalIgnoreCase)) ||
                (f.Name != null && f.Name.Contains("Nationality", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (!countryFields.Any()) return;

            // 使用静态字典
            var options = CountryConstants.Countries
                .Select(c => new ChannelRequiredFieldResponseValue
                {
                    Key = $"{c.Name} ({c.CCA2})", // Display Name as Key (frontend requirement)
                    Value = c.CCA2                // ID as Value
                })
                .OrderBy(o => o.Key) // Order by Display Name
                .ToList();

            foreach (var field in countryFields)
            {
                // 如果字段没有预设选项，或者是空的，或者是只有默认值，则填充
                if (field.Options == null || !field.Options.Any())
                {
                    field.Options = options;
                }
            }

            await Task.CompletedTask;
        }

        // ConvertToIso2CountryCode
        private string ConvertToIso2CountryCode(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            // 如果已经是 2 位，直接返回
            if (input.Length == 2) return input.ToUpper();

            // 尝试从静态字典查找（按 CCA3 查找）
            if (input.Length == 3)
            {
                var country = CountryConstants.Countries
                    .FirstOrDefault(c => c.CCA3 == input.ToUpper());
                if (country != null)
                {
                    return country.CCA2;
                }
            }

            // 尝试按名称查找
            var countryByName = CountryConstants.Countries
                .FirstOrDefault(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                                     c.CNName.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (countryByName != null)
            {
                return countryByName.CCA2;
            }

            // 兼容处理：截取前 2 位
            return input.Length >= 2 ? input.Substring(0, 2).ToUpper() : null;
        }

        private async Task PopulateValuesFromRequestLog(List<ChannelRequiredFieldResponse> fields, Guid requestId)
        {
            var request = await _payChannelServiceRequestRepository.FirstOrDefaultAsync(requestId);
            if (request == null || string.IsNullOrEmpty(request.RequestPayload))
            {
                return;
            }

            try
            {
                // Deserialize payload dynamically
                var payload = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(request.RequestPayload);
                if (payload == null) return;

                foreach (var field in fields)
                {
                    // Match by field name (snake_case)
                    if (payload.TryGetValue(field.FieldName, StringComparison.InvariantCultureIgnoreCase, out var token))
                    {
                        field.Value = token.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to populate values from request log {requestId}: {ex.Message}");
            }
        }

        /// <summary>
        /// 规范化 RequestPayload 中的日期字段格式（去掉 " 00:00:00" 时间部分）
        /// </summary>
        private string NormalizeDateFieldsInPayload(string requestPayload)
        {
            if (string.IsNullOrEmpty(requestPayload)) return requestPayload;

            try
            {
                var payload = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(requestPayload);
                if (payload == null) return requestPayload;

                // 需要清理的日期字段
                var dateFields = new[] { "birth_date", "business_person_birth_date" };

                foreach (var fieldName in dateFields)
                {
                    if (payload.TryGetValue(fieldName, StringComparison.InvariantCultureIgnoreCase, out var token))
                    {
                        var value = token.ToString();
                        // 如果包含 " 00:00:00"，去掉它
                        if (value.Contains(" 00:00:00"))
                        {
                            payload[fieldName] = value.Replace(" 00:00:00", "");
                        }
                    }
                }

                return JsonConvert.SerializeObject(payload);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to normalize date fields in payload: {ex.Message}");
                return requestPayload; // 出错时返回原值
            }
        }



        #endregion
    }
}
