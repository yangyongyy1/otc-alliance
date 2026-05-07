using Abp.BackgroundJobs;
using Abp.Dependency;
using System.Threading.Tasks;

namespace ClientPlatform.Kyc.Jobs
{
    /// <summary>
    /// KYC 验证完成回调任务
    /// </summary>
    public class CustomerKycVerificationCompletedCallBackJob : AsyncBackgroundJob<CustomerKycVerificationCompletedCallBackJobArgs>, ITransientDependency
    {
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="args">参数</param>
        public override async Task ExecuteAsync(CustomerKycVerificationCompletedCallBackJobArgs args)
        {
            // TODO: 实现 KYC 验证完成后的回调逻辑
            await Task.CompletedTask;
        }
    }
}
