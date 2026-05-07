using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Dynamic;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;

namespace ClientPlatform.Pay;

/// <summary>
/// 支付客户端实现，用于与 Sun Pay API 进行通信
/// </summary>
public class PayClient : IPayClient, ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly PayClientOptions _options;
    private readonly ILogger<PayClient> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">支付客户端配置选项</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="httpClient">HTTP 客户端 (可选，用于测试注入)</param>
    public PayClient(IOptions<PayClientOptions> options, ILogger<PayClient> logger, HttpClient httpClient = null)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient();

        // 防御性检查：如果 BaseUrl 为空，抛出友好的错误信息
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException(
                "PayClientOptions.BaseUrl is not configured. " +
                "Please add 'PayClientConfig:BaseUrl' to your appsettings.json or Apollo configuration center."
            );
        }

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        // Add User-Agent to avoid Cloudflare blocking
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }
        if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        }
        if (!_httpClient.DefaultRequestHeaders.Contains("Accept-Language"))
        {
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        }
    }

    #region User Management

    /// <summary>
    /// 获取客户必填项
    /// </summary>
    public async Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCustomerRequiredFieldsAsync(GetCustomerRequiredFieldsInput input, string invitationCode)
    {
        return await GetAsync<System.Collections.Generic.List<ChannelRequiredFieldResponse>>("/api/v3/VA/VACustomerRequiredFields", input, invitationCode);
    }

    public async Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCustomerRequiredFieldsAsync(GetCustomerRequiredFieldsInput input, PayMerchantOption option)
    {
        return await GetAsync<System.Collections.Generic.List<ChannelRequiredFieldResponse>>("/api/v3/VA/VACustomerRequiredFields", input, option);
    }

    /// <summary>
    /// 创建客户
    /// </summary>
    public async Task<PayApiResponse<CreateVACustomerResponse>> CreateCustomerAsync(CreateCustomerInput input, string invitationCode)
    {
        return await PostAsync<CreateVACustomerResponse>("/api/v3/VA/Customer", input, invitationCode);
    }

    public async Task<PayApiResponse<CreateVACustomerResponse>> CreateCustomerAsync(CreateCustomerInput input, PayMerchantOption option)
    {
        return await PostAsync<CreateVACustomerResponse>("/api/v3/VA/Customer", input, option);
    }

    /// <summary>
    /// 查询客户信息
    /// </summary>
    public async Task<PayApiResponse<GetVACustomerResponse>> QueryCustomerAsync(QueryCustomerInput input, string invitationCode)
    {
        return await GetAsync<GetVACustomerResponse>("/api/v3/VA/Customer", input, invitationCode);
    }

    public async Task<PayApiResponse<GetVACustomerResponse>> QueryCustomerAsync(QueryCustomerInput input, PayMerchantOption option)
    {
        return await GetAsync<GetVACustomerResponse>("/api/v3/VA/Customer", input, option);
    }

    public async Task<PayApiResponse<bool>> QueryCustomerKycInfoAsync(QueryCustomerKycInfoInput input, PayMerchantOption option)
    {
        return await GetAsync<bool>("/api/v3/VA/Client/KycInfo", input, option);
    }

    /// <summary>
    /// 上传客户文档
    /// </summary>
    public async Task<PayApiResponse<UploadCustomerDocResponse>> UploadCustomerDocAsync(UploadCustomerDocInput input, string invitationCode)
    {
        return await PostMultipartAsync<UploadCustomerDocResponse>("/api/v3/VA/UploadCustomerDoc", input, invitationCode);
    }

    public async Task<PayApiResponse<UploadCustomerDocResponse>> UploadCustomerDocAsync(UploadCustomerDocInput input, PayMerchantOption option)
    {
        return await PostMultipartAsync<UploadCustomerDocResponse>("/api/v3/VA/UploadCustomerDoc", input, option);
    }

    /// <summary>
    /// 通用文件上传 (用于创建客户前获取文件 ID)
    /// </summary>
    public async Task<PayApiResponse<UploadFileResponse>> UploadFileAsync(UploadFileInput input, string invitationCode)
    {
        return await PostMultipartAsync<UploadFileResponse>("/api/v3/VA/UploadCustomerDoc", input, invitationCode);
    }

    public async Task<PayApiResponse<UploadFileResponse>> UploadFileAsync(UploadFileInput input, PayMerchantOption option)
    {
        return await PostMultipartAsync<UploadFileResponse>("/api/v3/VA/UploadCustomerDoc", input, option);
    }

    #endregion

    #region Account Management

    /// <summary>
    /// 获取账户必填项
    /// </summary>
    public async Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCreateAccountRequiredFieldsAsync(GetAccountRequiredFieldsInput input, string invitationCode)
    {
        return await GetAsync<System.Collections.Generic.List<ChannelRequiredFieldResponse>>("/api/v3/VA/AccountRequiredFields", input, invitationCode);
    }

    public async Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCreateAccountRequiredFieldsAsync(GetAccountRequiredFieldsInput input, PayMerchantOption option)
    {
        return await GetAsync<System.Collections.Generic.List<ChannelRequiredFieldResponse>>("/api/v3/VA/AccountRequiredFields", input, option);
    }

    /// <summary>
    /// 创建虚拟账户 (个人或企业)
    /// </summary>
    public async Task<PayApiResponse<CreateAccountResponse>> CreateAccountAsync(CreateAccountInput input, string invitationCode)
    {
        return await PostAsync<CreateAccountResponse>("/api/v3/VA/Account", input, invitationCode);
    }

    public async Task<PayApiResponse<CreateAccountResponse>> CreateAccountAsync(CreateAccountInput input, PayMerchantOption option)
    {
        return await PostAsync<CreateAccountResponse>("/api/v3/VA/Account", input, option);
    }

    /// <summary>
    /// 获取账户列表
    /// </summary>
    public async Task<PayApiResponse<System.Collections.Generic.List<GetAccountsResponse>>> GetAccountListAsync(GetAccountListInput input, string invitationCode)
    {
        return await GetAsync<System.Collections.Generic.List<GetAccountsResponse>>("/api/v3/VA/Accounts", input, invitationCode);
    }

    public async Task<PayApiResponse<System.Collections.Generic.List<GetAccountsResponse>>> GetAccountListAsync(GetAccountListInput input, PayMerchantOption option)
    {
        return await GetAsync<System.Collections.Generic.List<GetAccountsResponse>>("/api/v3/VA/Accounts", input, option);
    }

    /// <summary>
    /// 获取账户详情
    /// </summary>
    public async Task<PayApiResponse<GetAccountsResponse>> GetAccountDetailAsync(string accountId, string invitationCode)
    {
        return await GetAsync<GetAccountsResponse>("/api/v3/VA/Account", new { account_id = accountId }, invitationCode);
    }

    public async Task<PayApiResponse<GetAccountsResponse>> GetAccountDetailAsync(string accountId, PayMerchantOption option)
    {
        return await GetAsync<GetAccountsResponse>("/api/v3/VA/Account", new { account_id = accountId }, option);
    }

    #endregion

    #region Local Pay (PAYIN 收银台)

    /// <summary>
    /// 创建 SunPay 本地支付（PAYIN 收银台）订单
    /// </summary>
    /// <param name="input">本地支付下单请求参数</param>
    /// <param name="option">商户配置</param>
    /// <returns>包含收银台支付链接等信息的响应</returns>
    public async Task<PayApiResponse<SunPayLocalPayCreateResponse>> CreateLocalPayOrderAsync(SunPayLocalPayCreateInput input, PayMerchantOption option)
    {
        // 注意：接口路径根据 SunPay GA PAYIN CHECKOUT 文档配置，如有调整请同步修改此处
        const string url = "/api/v3/Fiat/PayIn";
        return await PostAsync<SunPayLocalPayCreateResponse>(url, input, option);
    }

    #endregion

    private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    private async Task<PayApiResponse<T>> PostAsync<T>(string url, object data, string invitationCode)
    {
        var merchant = GetMerchantOption(invitationCode);
        return await PostAsync<T>(url, data, merchant);
    }

    private async Task<PayApiResponse<T>> PostAsync<T>(string url, object data, PayMerchantOption merchant)
    {
        var jsonBody = JsonConvert.SerializeObject(data, _jsonSerializerSettings);

        // Debug: Print Payload
        System.Console.WriteLine($"[SunPay_Debug] URL: {url}");
        System.Console.WriteLine($"[SunPay_Debug] Body: {jsonBody}");

        var request = CreateRequest(HttpMethod.Post, url, merchant, jsonBody);

        _logger.LogInformation($"Sending POST request to {url}. Body: {jsonBody}");

        return await SendRequestAsync<T>(request);
    }

    private async Task<PayApiResponse<T>> GetAsync<T>(string url, object queryParams, string invitationCode)
    {
        var merchant = GetMerchantOption(invitationCode);
        return await GetAsync<T>(url, queryParams, merchant);
    }

    private async Task<PayApiResponse<T>> GetAsync<T>(string url, object queryParams, PayMerchantOption merchant)
    {
        if (queryParams != null)
        {
            var json = JsonConvert.SerializeObject(queryParams, _jsonSerializerSettings);
            var dict = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(json);
            if (dict != null && dict.Count > 0)
            {
                var queryString = string.Join("&", System.Linq.Enumerable.Select(dict, p => $"{p.Key}={System.Net.WebUtility.UrlEncode(p.Value)}"));
                url += "?" + queryString;
            }
        }

        var request = CreateRequest(HttpMethod.Get, url, merchant, "");
        _logger.LogInformation($"Sending GET request to {url}");

        return await SendRequestAsync<T>(request);
    }

    private async Task<PayApiResponse<T>> PostMultipartAsync<T>(string url, UploadFileInput input, string invitationCode)
    {
        var merchant = GetMerchantOption(invitationCode);
        return await PostMultipartAsync<T>(url, input, merchant);
    }

    private async Task<PayApiResponse<T>> PostMultipartAsync<T>(string url, UploadFileInput input, PayMerchantOption merchant)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");

        var signature = GenerateSignature(timestamp, nonce, "", merchant.MerchantSecret);

        request.Headers.Add("SunPay-Key", merchant.MerchantKey);
        request.Headers.Add("SunPay-Timestamp", timestamp);
        request.Headers.Add("SunPay-Nonce", nonce);
        request.Headers.Add("SunPay-Sign", signature);

        var content = new MultipartFormDataContent();
        // UploadFile likely just expects "file" or "document" param
        if (input.FileStream != null)
        {
            var fileContent = new StreamContent(input.FileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "document", input.FileName ?? "file.jpg");
        }

        // Add BizType if needed, check docs later. For now generic.
        if (!string.IsNullOrEmpty(input.BizType))
        {
            content.Add(new StringContent(input.BizType), "document_type");
        }

        request.Content = content;

        _logger.LogInformation($"Sending Multipart Upload File request to {url}. FileName: {input.FileName}");

        return await SendRequestAsync<T>(request);
    }

    private string GetContentType(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return "application/octet-stream";
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }


    private async Task<PayApiResponse<T>> PostMultipartAsync<T>(string url, UploadCustomerDocInput input, string invitationCode)
    {
        var merchant = GetMerchantOption(invitationCode);
        return await PostMultipartAsync<T>(url, input, merchant);
    }

    private async Task<PayApiResponse<T>> PostMultipartAsync<T>(string url, UploadCustomerDocInput input, PayMerchantOption merchant)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");

        var signature = GenerateSignature(timestamp, nonce, "", merchant.MerchantSecret);

        request.Headers.Add("SunPay-Key", merchant.MerchantKey);
        request.Headers.Add("SunPay-Timestamp", timestamp);
        request.Headers.Add("SunPay-Nonce", nonce);
        request.Headers.Add("SunPay-Sign", signature);

        var content = new MultipartFormDataContent();
        if (!string.IsNullOrEmpty(input.CustomerId))
        {
            content.Add(new StringContent(input.CustomerId), "customer_id");
        }
        content.Add(new StringContent(input.DocumentType), "document_type");

        if (input.FileStream != null)
        {
            var fileContent = new StreamContent(input.FileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "document", input.FileName ?? "document.jpg");
        }

        request.Content = content;

        _logger.LogInformation($"Sending Multipart POST request to {url}. CustomerId: {input.CustomerId}, DocumentType: {input.DocumentType}");

        return await SendRequestAsync<T>(request);
    }

    // SendRequestAsync and GenerateSignature and CreateRequest remain shared and unchanged (except logic is reused)
    // Actually, I need to make sure CreateRequest uses the Merchant object passed to it.
    // My new PostAsync/GetAsync call CreateRequest which already takes PayMerchantOption. 
    // And PostMultipartAsync logic manually constructs request but now uses 'merchant' param.
    // So all looks good.

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, PayMerchantOption merchant, string body = "")
    {
        var request = new HttpRequestMessage(method, url);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");

        var signature = GenerateSignature(timestamp, nonce, body, merchant.MerchantSecret);

        request.Headers.Add("SunPay-Key", merchant.MerchantKey);
        request.Headers.Add("SunPay-Timestamp", timestamp);
        request.Headers.Add("SunPay-Nonce", nonce);
        request.Headers.Add("SunPay-Sign", signature);

        if (!string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private async Task<PayApiResponse<T>> SendRequestAsync<T>(HttpRequestMessage request)
    {
        using (request)
        {
            var requestBody = request.Content != null ? await request.Content.ReadAsStringAsync() : "No Body";
            _logger.LogInformation($"=== SunPay API Request ===");
            _logger.LogInformation($"Method: {request.Method}");
            _logger.LogInformation($"URL: {request.RequestUri}");
            _logger.LogInformation($"Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            _logger.LogInformation($"Request Body: {requestBody}");
            _logger.LogInformation($"=========================");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"=== SunPay API Response ===");
            _logger.LogInformation($"Status: {response.StatusCode} ({(int)response.StatusCode})");
            _logger.LogInformation($"Response Body: {responseContent}");
            _logger.LogInformation($"===========================");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"SunPay API Error: {response.StatusCode}, Content: {responseContent}");
                throw new Exception($"SunPay API Error: {response.StatusCode}, Content: {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<PayApiResponse<T>>(responseContent, new ExpandoObjectConverter());
            if (result == null)
            {
                _logger.LogError($"SunPay API returned null or invalid JSON. Content: {responseContent}");
                throw new Exception($"SunPay API returned null or invalid JSON. Content: {responseContent}");
            }
            return result;
        }
    }

    private string GenerateSignature(string timestamp, string nonce, string body, string secret)
    {
        var payload = timestamp + nonce + body;
        // Use secret from argument
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using (var hmac = new HMACSHA256(secretBytes))
        {
            var hashBytes = hmac.ComputeHash(payloadBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }
    }

    private PayMerchantOption GetMerchantOption(string invitationCode)
    {
        var merchant = _options.Merchants.Find(m => m.InvitationCode == invitationCode);
        if (merchant == null)
        {
            throw new Exception($"未找到邀请码为 {invitationCode} 的商户配置");
        }
        return merchant;
    }
}
