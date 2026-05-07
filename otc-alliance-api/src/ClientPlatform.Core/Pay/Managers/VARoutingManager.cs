using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ClientPlatform.AllianceManagement;

namespace ClientPlatform.Pay.Managers
{
    public class VARoutingManager : DomainService, IVARoutingManager
    {
        private readonly IRepository<MerchantChannelCurrency> _merchantChannelCurrencyRepository;

        public VARoutingManager(IRepository<MerchantChannelCurrency> merchantChannelCurrencyRepository)
        {
            _merchantChannelCurrencyRepository = merchantChannelCurrencyRepository;
        }

        public async Task<MerchantChannelCurrency> GetRouteAsync(int merchantId, string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new UserFriendlyException("Currency cannot be empty");
            }

            var route = await _merchantChannelCurrencyRepository.GetAll()
                .FirstOrDefaultAsync(x => x.MerchantId == merchantId &&
                                          x.Currency == currency &&
                                          x.OpenClose == OpenClose.Open);

            if (route == null)
            {
                throw new UserFriendlyException($"No available channel found for currency {currency} (Merchant: {merchantId})");
            }

            return route;
        }
    }
}
