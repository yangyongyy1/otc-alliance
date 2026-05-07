using Abp.Application.Services.Dto;
using Newtonsoft.Json;
using System;

namespace ClientPlatform.Pay.Dto.Response
{
    public class CreateUserAccountDto
    {
        [JsonProperty("account_id")]
        public string? AccountId { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("customer_id")]
        public Guid CustomerId { get; set; }

        [JsonProperty("request_id")]
        public Guid? RequestId { get; set; }
    }
}
