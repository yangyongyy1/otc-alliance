using System;
using System.Threading.Tasks;
using Abp.Dependency;
using ClientPlatform.Kyc.Channels.Sumsub;
using ClientPlatform.Kyc.Dto;
using Rebus.Handlers;

namespace ClientPlatform.RabbitMq.Handlers
{
    /// <summary>
    /// Sumsub 文档图片保存消息处理器
    /// </summary>
    public class SumsubDocumentImageHandler : IHandleMessages<SumsubSaveSumsubDocumentImageEto>, ITransientDependency
    {
        private readonly SumsubChannelProvider _sumsubChannelProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sumsubChannelProvider"></param>
        public SumsubDocumentImageHandler(SumsubChannelProvider sumsubChannelProvider)
        {
            _sumsubChannelProvider = sumsubChannelProvider;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Handle(SumsubSaveSumsubDocumentImageEto message)
        {
            if (Guid.TryParse(message.id, out var kycApplicantId))
            {
                await _sumsubChannelProvider.SaveSumsubDocument(message.applicantId, message.inspectionId, kycApplicantId);
            }
        }
    }
}
