using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace ClientPlatform.AllianceManagement
{
    public class MerchantSubCodeManager : DomainService
    {
        private readonly IRepository<MerchantSubCode, int> _merchantSubCodeRepository;
        private readonly IRepository<Merchant, int> _merchantRepository;
        public MerchantSubCodeManager(IRepository<MerchantSubCode, int> merchantSubCodeRepository, IRepository<Merchant, int> merchantRepository)
        {
            _merchantSubCodeRepository = merchantSubCodeRepository;
            _merchantRepository = merchantRepository;
        }

        /// <summary>
        /// 根据子码获取商户
        /// </summary>
        /// <param name="subCode"></param>
        /// <returns></returns>
        public async Task<Merchant> GetMerchantBySubCode(string subCode)
        {
            var merchantSubCode = await _merchantSubCodeRepository.FirstOrDefaultAsync(n => n.SubCode == subCode);
            if (merchantSubCode == null)
            {
                //去主表找
                var merchant = await _merchantRepository.GetAllIncluding(n => n.Alliance).Where(n => n.RelationCode == subCode).FirstOrDefaultAsync();
                if (merchant == null)
                {
                    return null;
                }
                return merchant;
            }
            else
            {
                //   var merchant = await _merchantRepository.GetAllIncluding(n => n.Alliance).Where(n => n.RelationCode == subCode).FirstOrDefaultAsync();
               var merchant = await _merchantRepository.GetAllIncluding(n => n.Alliance).Where(n => n.Id == merchantSubCode.MerchantId).FirstOrDefaultAsync();
                return merchant;
            }
        }


    }
}