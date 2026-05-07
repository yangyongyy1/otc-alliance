using System.Collections.Generic;
using System.Threading.Tasks;
using ClientPlatform.Pay.Models;

namespace ClientPlatform.Pay
{
    /// <summary>
    /// 渠道元数据提供者接口
    /// 用于获取动态表单字段定义
    /// </summary>
    public interface IChannelMetadataProvider
    {
        /// <summary>
        /// 获取创建虚拟账户所需的字段定义
        /// </summary>
        /// <param name="currency">币种</param>
        /// <param name="channelCode">渠道路由代码 (子产品线)</param>
        /// <param name="merchantOption">商户配置 (用于 API 调用)</param>
        /// <param name="customerId">渠道客户ID (可选)</param>
        /// <returns>字段定义列表</returns>
        Task<List<FieldDefinition>> GetRequiredFieldsAsync(string currency, string channelCode, PayMerchantOption merchantOption = null, string customerId = null);
    }
}
