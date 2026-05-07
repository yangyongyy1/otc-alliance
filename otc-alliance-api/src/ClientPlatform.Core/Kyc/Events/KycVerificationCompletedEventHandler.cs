using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Events.Bus.Handlers;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ClientPlatform.Pay.Dto;
using Rebus.Bus;

namespace ClientPlatform.Kyc.Events
{
    public class KycVerificationCompletedEventHandler : IAsyncEventHandler<KycVerificationCompletedEvent>, ITransientDependency
    {
        private readonly IBus _bus;
        private readonly IConfiguration _configuration;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public KycVerificationCompletedEventHandler(
            IBus bus,
            IConfiguration configuration)
        {
            _bus = bus;
            _configuration = configuration;
        }

        public async Task HandleEventAsync(KycVerificationCompletedEvent eventData)
        {
            Logger.Info($"KycVerificationCompletedEventHandler===接收到事件===UserId={eventData.UserId}===KycApplicantId={eventData.KycApplicantId}===IsApproved={eventData.IsApproved}");

            if (eventData.IsApproved)
            {
                Logger.Info($"KycVerificationCompletedEventHandler===开始发布Rebus消息 CreatePayChannelCustomerEto");

                try
                {
                    // 使用 Rebus 发送
                    await _bus.Send(new CreatePayChannelCustomerEto
                    {
                        UserId = eventData.UserId,
                        KycApplicantId = eventData.KycApplicantId
                    });

                    Logger.Info($"KycVerificationCompletedEventHandler===Rebus消息发送成功===UserId={eventData.UserId}");
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"KycVerificationCompletedEventHandler===Rebus消息发送失败===UserId={eventData.UserId}===Error={ex.Message}===StackTrace={ex.StackTrace}", ex);
                    throw;
                }
            }
            else
            {
                Logger.Info($"KycVerificationCompletedEventHandler===KYC未通过，不创建客户===UserId={eventData.UserId}");
            }
        }
    }
}
