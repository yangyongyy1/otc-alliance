using System.Threading.Tasks;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;

namespace ClientPlatform.Pay;

/// <summary>
/// 支付客户端接口
/// </summary>
public interface IPayClient
{
    #region User Management

    /// <summary>
    /// 获取客户必填项
    /// </summary>
    Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCustomerRequiredFieldsAsync(GetCustomerRequiredFieldsInput input, string invitationCode);
    Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCustomerRequiredFieldsAsync(GetCustomerRequiredFieldsInput input, PayMerchantOption option);

    /// <summary>
    /// 创建客户
    /// </summary>
    Task<PayApiResponse<CreateVACustomerResponse>> CreateCustomerAsync(CreateCustomerInput input, string invitationCode);
    Task<PayApiResponse<CreateVACustomerResponse>> CreateCustomerAsync(CreateCustomerInput input, PayMerchantOption option);

    /// <summary>
    /// 查询客户信息
    /// </summary>
    Task<PayApiResponse<GetVACustomerResponse>> QueryCustomerAsync(QueryCustomerInput input, string invitationCode);
    Task<PayApiResponse<GetVACustomerResponse>> QueryCustomerAsync(QueryCustomerInput input, PayMerchantOption option);
    /// <summary>
    /// 获取用户kyc是否存在
    /// </summary>
    /// <param name="input"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    Task<PayApiResponse<bool>> QueryCustomerKycInfoAsync(QueryCustomerKycInfoInput input, PayMerchantOption option);

    /// <summary>
    /// 上传客户文档
    /// </summary>
    Task<PayApiResponse<UploadCustomerDocResponse>> UploadCustomerDocAsync(UploadCustomerDocInput input, string invitationCode);
    Task<PayApiResponse<UploadCustomerDocResponse>> UploadCustomerDocAsync(UploadCustomerDocInput input, PayMerchantOption option);

    /// <summary>
    /// 通用文件上传 (用于创建客户前获取文件 ID)
    /// </summary>
    Task<PayApiResponse<UploadFileResponse>> UploadFileAsync(UploadFileInput input, string invitationCode);
    Task<PayApiResponse<UploadFileResponse>> UploadFileAsync(UploadFileInput input, PayMerchantOption option);

    #endregion

    #region Account Management

    /// <summary>
    /// 获取账户必填项
    /// </summary>
    Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCreateAccountRequiredFieldsAsync(GetAccountRequiredFieldsInput input, string invitationCode);
    Task<PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>> GetCreateAccountRequiredFieldsAsync(GetAccountRequiredFieldsInput input, PayMerchantOption option);

    /// <summary>
    /// 创建虚拟账户 (个人或企业)
    /// </summary>
    Task<PayApiResponse<CreateAccountResponse>> CreateAccountAsync(CreateAccountInput input, string invitationCode);
    Task<PayApiResponse<CreateAccountResponse>> CreateAccountAsync(CreateAccountInput input, PayMerchantOption option);

    /// <summary>
    /// 获取账户列表
    /// </summary>
    Task<PayApiResponse<System.Collections.Generic.List<GetAccountsResponse>>> GetAccountListAsync(GetAccountListInput input, string invitationCode);
    Task<PayApiResponse<System.Collections.Generic.List<GetAccountsResponse>>> GetAccountListAsync(GetAccountListInput input, PayMerchantOption option);

    /// <summary>
    /// 获取账户详情
    /// </summary>
    Task<PayApiResponse<GetAccountsResponse>> GetAccountDetailAsync(string accountId, string invitationCode);
    Task<PayApiResponse<GetAccountsResponse>> GetAccountDetailAsync(string accountId, PayMerchantOption option);

    #endregion

    #region Local Pay (PAYIN 收银台)

    /// <summary>
    /// 创建 SunPay 本地支付（PAYIN 收银台）订单
    /// </summary>
    /// <param name="input">本地支付下单请求参数</param>
    /// <param name="option">商户配置（包含 Key/Secret）</param>
    /// <returns>包含收银台支付链接等信息的响应</returns>
    Task<PayApiResponse<SunPayLocalPayCreateResponse>> CreateLocalPayOrderAsync(SunPayLocalPayCreateInput input, PayMerchantOption option);

    #endregion
}
