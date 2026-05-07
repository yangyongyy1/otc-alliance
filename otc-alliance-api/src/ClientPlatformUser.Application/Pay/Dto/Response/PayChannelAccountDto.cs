using Abp.Application.Services.Dto;
using System;
using ClientPlatform.Pay.Entities;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    public class PayChannelAccountDto : EntityDto<Guid>
    {

        [JsonProperty("channel_account_id")]
        public string? ChannelAccountId { get; set; }

        [JsonProperty("reference_id")]
        public string? ReferenceId { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("account_name")]
        public string? AccountName { get; set; }

        [JsonProperty("account_number")]
        public string? AccountNumber { get; set; }

        [JsonProperty("bank_name")]
        public string? BankName { get; set; }

        [JsonProperty("status")]
        public VAStatus Status { get; set; }

        [JsonProperty("rejection_reason")]
        public string? RejectionReason { get; set; }

        [JsonProperty("creation_time")]
        public DateTime CreationTime { get; set; }
    }
}
