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
using ClientPlatform.AllianceManagement;

namespace ClientPlatform.Pay;

/// <summary>
/// 支付客户端实现（基于 Merchant 对象），用于与 Sun Pay API 进行通信
/// </summary>
public class MerchantPayClient : ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<MerchantPayClient> _logger;
    private readonly PayClientHeaderOptions _headerOptions;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">支付客户端配置选项（仅用于获取 BaseUrl）</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="httpClient">HTTP 客户端 (可选，用于测试注入)</param>
    public MerchantPayClient(IOptions<PayClientOptions> options, ILogger<MerchantPayClient> logger, HttpClient httpClient = null)
    {
        _baseUrl = options.Value.BaseUrl;
        _headerOptions = options.Value.Headers ?? new PayClientHeaderOptions();
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient();
        
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
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
    /// <param name="input">查询参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCustomerRequiredFieldsAsync(GetCustomerRequiredFieldsInput input)
    {
        ValidateInput(input);
        return await GetAsync<System.Collections.Generic.List<ChannelRequiredFieldResponse>>("/api/v3/VA/VACustomerRequiredFields", input);
    }

    /// <summary>
    /// 创建客户
    /// </summary>
    /// <param name="input">创建客户所需参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<CreateVACustomerResponse>> CreateCustomerAsync(CreateCustomerInput input)
    {
        ValidateInput(input);
        return await PostAsync<CreateVACustomerResponse>("/api/v3/VA/Customer", input);
    }

    /// <summary>
    /// 查询客户信息
    /// </summary>
    /// <param name="input">查询参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<GetVACustomerResponse>> QueryCustomerAsync(QueryCustomerInput input)
    {
        ValidateInput(input);
        return await GetAsync<GetVACustomerResponse>("/api/v3/VA/Customer", input);
    }

    /// <summary>
    /// 上传客户文档
    /// </summary>
    /// <param name="input">上传参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<UploadCustomerDocResponse>> UploadCustomerDocAsync(UploadCustomerDocInput input)
    {
        ValidateInput(input);
        return await PostMultipartAsync<UploadCustomerDocResponse>("/api/v3/VA/UploadCustomerDoc", input);
    }

    #endregion

    #region Account Management

    /// <summary>
    /// 获取账户必填项
    /// </summary>
    /// <param name="input">查询参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCreateAccountRequiredFieldsAsync(GetAccountRequiredFieldsInput input)
    {
        ValidateInput(input);
        return await GetAsync<System.Collections.Generic.List<ChannelRequiredFieldResponse>>("/api/v3/VA/AccountRequiredFields", input);
    }

    /// <summary>
    /// 创建虚拟账户 (个人或企业)
    /// </summary>
    /// <param name="input">创建账户所需参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<CreateAccountResponse>> CreateAccountAsync(CreateAccountInput input)
    {
        ValidateInput(input);
        return await PostAsync<CreateAccountResponse>("/api/v3/VA/Account", input);
    }

    /// <summary>
    /// 获取账户列表
    /// </summary>
    /// <param name="input">查询参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<System.Collections.Generic.List<GetAccountsResponse>>> GetAccountListAsync(GetAccountListInput input)
    {
        ValidateInput(input);
        return await GetAsync<System.Collections.Generic.List<GetAccountsResponse>>("/api/v3/VA/Accounts", input);
    }

    /// <summary>
    /// 获取账户详情
    /// </summary>
    /// <param name="input">查询参数（需包含 MerchantKey 和 MerchantSecret，以及 account_id）</param>
    /// <returns>API 响应结果</returns>
    public async Task<PayApiResponse<GetAccountsResponse>> GetAccountDetailAsync(GetAccountListInput input)
    {
        ValidateInput(input);
        if (string.IsNullOrWhiteSpace(input.AccountId))
        {
            throw new ArgumentException("AccountId 不能为空", nameof(input));
        }
        return await GetAsync<GetAccountsResponse>("/api/v3/VA/Account", input);
    }

    #endregion

    #region Callback Config

    /// <summary>
    /// 配置 VA 业务回调（同名更新）
    /// Method: POST /api/v3/VA/CallbackConfig
    /// </summary>
    public async Task<PayApiResponse<object>> UpsertBusinessApiCallBackConfigAsync(UpsertBusinessApiCallBackConfigInput input)
    {
        ValidateInput(input);
        return await PostAsync<object>("/api/v3/VA/CallbackConfig", input);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 从 Merchant 对象设置认证信息到 input
    /// </summary>
    /// <typeparam name="T">Input 类型</typeparam>
    /// <param name="input">请求输入对象</param>
    /// <param name="merchant">商户对象</param>
    /// <param name="merchantSecret">商户密钥</param>
    /// <returns>设置后的 input 对象</returns>
    public static T SetMerchantAuth<T>(T input, Merchant merchant, string merchantSecret) where T : BasePayRequest
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }
        if (merchant == null)
        {
            throw new ArgumentNullException(nameof(merchant));
        }
        if (string.IsNullOrWhiteSpace(merchant.Key))
        {
            throw new ArgumentException("商户 Key 不能为空", nameof(merchant));
        }
        if (string.IsNullOrWhiteSpace(merchantSecret))
        {
            throw new ArgumentException("商户 Secret 不能为空", nameof(merchantSecret));
        }

        input.MerchantKey = merchant.Key;
        input.MerchantSecret = merchantSecret;
        return input;
    }

    #endregion

    private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <summary>
    /// 通用 POST 请求方法
    /// </summary>
    /// <typeparam name="T">响应的数据类型</typeparam>
    /// <param name="url">请求 URL</param>
    /// <param name="input">请求数据（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    private async Task<PayApiResponse<T>> PostAsync<T>(string url, BasePayRequest input)
    {
        var jsonBody = JsonConvert.SerializeObject(input, _jsonSerializerSettings);

        var request = CreateRequest(HttpMethod.Post, url, input.MerchantKey, input.MerchantSecret, jsonBody);
        
        _logger.LogInformation($"Sending POST request to {url}. Body: {jsonBody}");

        return await SendRequestAsync<T>(request);
    }

    /// <summary>
    /// 通用 GET 请求方法
    /// </summary>
    /// <typeparam name="T">响应的数据类型</typeparam>
    /// <param name="url">请求 URL</param>
    /// <param name="input">查询参数对象（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    private async Task<PayApiResponse<T>> GetAsync<T>(string url, BasePayRequest input)
    {
        // 创建查询参数字典，排除 MerchantKey 和 MerchantSecret
        var queryParams = new System.Collections.Generic.Dictionary<string, string>();
        var json = JsonConvert.SerializeObject(input, _jsonSerializerSettings);
        var dict = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(json);
        
        if (dict != null && dict.Count > 0)
        {
            foreach (var kvp in dict)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    queryParams[kvp.Key] = kvp.Value;
                }
            }
        }

        if (queryParams.Count > 0)
        {
            var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={System.Net.WebUtility.UrlEncode(p.Value)}"));
            url += "?" + queryString;
        }

        // GET 请求 body 为空
        var request = CreateRequest(HttpMethod.Get, url, input.MerchantKey, input.MerchantSecret, "");
        
        _logger.LogInformation($"Sending GET request to {url}");

        return await SendRequestAsync<T>(request);
    }

    /// <summary>
    /// 创建带有签名和认证头的 HTTP 请求
    /// </summary>
    /// <param name="method">HTTP 方法</param>
    /// <param name="url">请求 URL</param>
    /// <param name="merchantKey">商户 Key</param>
    /// <param name="merchantSecret">商户 Secret</param>
    /// <param name="body">请求体 (JSON 字符串)</param>
    /// <returns>HttpRequestMessage 对象</returns>
    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string merchantKey, string merchantSecret, string body = "")
    {
        var request = new HttpRequestMessage(method, url);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");

        // 签名生成: HMAC-SHA256(timestamp + nonce + body)
        var signature = GenerateSignature(timestamp, nonce, body, merchantSecret);

        request.Headers.Add(_headerOptions.KeyHeaderName, merchantKey);
        request.Headers.Add(_headerOptions.TimestampHeaderName, timestamp);
        request.Headers.Add(_headerOptions.NonceHeaderName, nonce);
        request.Headers.Add(_headerOptions.SignHeaderName, signature);

        if (!string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return request;
    }

    /// <summary>
    /// 通用 Multipart POST 请求方法 (用于文件上传)
    /// </summary>
    /// <typeparam name="T">响应的数据类型</typeparam>
    /// <param name="url">请求 URL</param>
    /// <param name="input">上传参数（需包含 MerchantKey 和 MerchantSecret）</param>
    /// <returns>API 响应结果</returns>
    private async Task<PayApiResponse<T>> PostMultipartAsync<T>(string url, UploadCustomerDocInput input)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");

        // 签名生成: HMAC-SHA256(timestamp + nonce + body)
        // 注意: 文件上传时 body 为空字符串参与签名 (需确认 API 文档)
        // 根据 Postman 脚本，文件上传时 body 似乎不参与签名或者有特殊处理
        // 假设 body 为空字符串
        var signature = GenerateSignature(timestamp, nonce, "", input.MerchantSecret);

        request.Headers.Add(_headerOptions.KeyHeaderName, input.MerchantKey);
        request.Headers.Add(_headerOptions.TimestampHeaderName, timestamp);
        request.Headers.Add(_headerOptions.NonceHeaderName, nonce);
        request.Headers.Add(_headerOptions.SignHeaderName, signature);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(input.CustomerId), "customer_id");
        content.Add(new StringContent(input.DocumentType), "document_type");
        
        if (input.FileStream != null)
        {
            var fileContent = new StreamContent(input.FileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream"); // 或根据文件类型设置
            content.Add(fileContent, "document", input.FileName ?? "document.jpg");
        }

        request.Content = content;

        _logger.LogInformation($"Sending Multipart POST request to {url}. CustomerId: {input.CustomerId}, DocumentType: {input.DocumentType}");

        return await SendRequestAsync<T>(request);
    }

    private async Task<PayApiResponse<T>> SendRequestAsync<T>(HttpRequestMessage request)
    {
        using (request)
        {
            // 记录请求详情
            var requestBody = request.Content != null ? await request.Content.ReadAsStringAsync() : "No Body";
            _logger.LogInformation($"=== SunPay API Request ===");
            _logger.LogInformation($"Method: {request.Method}");
            _logger.LogInformation($"URL: {request.RequestUri}");
            _logger.LogInformation($"Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            _logger.LogInformation($"Request Body: {requestBody}");
            _logger.LogInformation($"=========================");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // 记录响应详情
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

    /// <summary>
    /// 生成 HMAC-SHA256 签名
    /// </summary>
    /// <param name="timestamp">时间戳</param>
    /// <param name="nonce">随机字符串</param>
    /// <param name="body">请求体</param>
    /// <param name="secret">商户密钥</param>
    /// <returns>大写十六进制签名字符串</returns>
    private string GenerateSignature(string timestamp, string nonce, string body, string secret)
    {
        var payload = timestamp + nonce + body;
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using (var hmac = new HMACSHA256(secretBytes))
        {
            var hashBytes = hmac.ComputeHash(payloadBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }
    }

    /// <summary>
    /// 验证 input 对象中的认证信息
    /// </summary>
    /// <param name="input">请求输入对象</param>
    private void ValidateInput(BasePayRequest input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input), "请求参数不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.MerchantKey))
        {
            throw new ArgumentException("MerchantKey 不能为空，请使用 MerchantPayClient.SetMerchantAuth 方法设置认证信息", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.MerchantSecret))
        {
            throw new ArgumentException("MerchantSecret 不能为空，请使用 MerchantPayClient.SetMerchantAuth 方法设置认证信息", nameof(input));
        }
    }
}
