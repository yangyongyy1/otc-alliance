using System.Threading.Tasks;
using ClientPlatform.Kyc.Dto;

namespace ClientPlatform.Kyc.Channels
{
    /// <summary>
    /// KYC 渠道提供商接口
    /// 定义了所有 KYC 渠道必须实现的统一操作
    /// </summary>
    public interface IKycChannelProvider
    {
        /// <summary>
        /// 渠道名称 (如: Sumsub)
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// 创建申请人
        /// </summary>
        /// <param name="request">创建申请参数</param>
        /// <returns>创建结果</returns>
        Task<CreateApplicantResponse> CreateApplicantAsync(CreateApplicantRequest request);

        /// <summary>
        /// 获取 WebSDK 链接
        /// 生成用于前端跳转或嵌入的认证页面 URL
        /// </summary>
        /// <param name="request">获取链接参数</param>
        /// <returns>链接响应</returns>
        Task<GetWebSdkLinkResponse> GetWebSdkLinkAsync(GetWebSdkLinkRequest request);

        /// <summary>
        /// 处理回调通知
        /// 解析并验证回调数据，更新本地业务状态
        /// </summary>
        /// <param name="requestBody">回调原始 Body</param>
        /// <param name="signature">签名头</param>
        /// <returns></returns>
        Task<KycCallbackResult> HandleCallbackAsync(string requestBody, string signature);
    }
}
