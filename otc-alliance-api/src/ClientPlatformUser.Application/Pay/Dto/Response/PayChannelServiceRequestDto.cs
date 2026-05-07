using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Newtonsoft.Json;
using System;
using ClientPlatform.Pay.Entities;

namespace ClientPlatform.Pay.Dto.Response
{
    [AutoMapFrom(typeof(PayChannelServiceRequest))]
    public class PayChannelServiceRequestDto : EntityDto<Guid>
    {
        public int UserId { get; set; }

        public string? Currency { get; set; }

        [JsonProperty("requestPayload")]
        public string? RequestPayload { get; set; }

        [JsonProperty("status")]
        public PayChannelServiceRequestStatus Status { get; set; }

        [JsonIgnore]
        public string StatusDescription => Status.ToString();

        [JsonIgnore] // 隐藏失败步骤细节
        public PayChannelServiceRequestFailStep FailStep { get; set; }

        [JsonIgnore]
        public string FailStepDescription => FailStep.ToString();

        public string? FailReason { get; set; }

        public string? CustomerId { get; set; }

        public string? AccountId { get; set; }

        public int RetryCount { get; set; }

        public DateTime CreationTime { get; set; }



        /// <summary>
        /// 请求原始数据 (用于重试回显)
        /// </summary>
        public string? RequestData { get; set; }
    }
}
