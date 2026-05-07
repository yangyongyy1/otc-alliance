using System;
using Abp.Events.Bus;

namespace ClientPlatform.Pay.Dto
{
    public class CreatePayChannelAccountEto : EventData
    {
        public Guid RequestId { get; set; }
    }
}
