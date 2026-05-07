using Abp.Domain.Services;
using System.Threading.Tasks;
using ClientPlatform.AllianceManagement;

namespace ClientPlatform.Pay.Managers
{
    public interface IVARoutingManager : IDomainService
    {
        /// <summary>
        /// 获取商户对应币种的最佳路由 (1v1严格模式)
        /// </summary>
        /// <param name="merchantId">商户ID</param>
        /// <param name="currency">币种</param>
        /// <returns>路由配置实体</returns>
        Task<MerchantChannelCurrency> GetRouteAsync(int merchantId, string currency);
    }
}
