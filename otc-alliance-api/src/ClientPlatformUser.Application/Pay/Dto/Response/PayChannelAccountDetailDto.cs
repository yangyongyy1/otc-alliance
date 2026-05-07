using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    /// <summary>
    /// 支付渠道账户详情 DTO
    /// 包含账户基本信息和支持的支付方式列表
    /// </summary>
    public class PayChannelAccountDetailDto
    {
        /// <summary>
        /// 账户 ID (系统内部 GUID)
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// 状态 (Active, Pending, etc.)
        /// </summary>
        [JsonProperty("status")]
        public VAStatus Status { get; set; }

        [JsonProperty("account_holder_name")]
        public string AccountHolderName { get; set; }

        /// <summary>
        /// 渠道提供的 Account ID
        /// </summary>
        [JsonProperty("channel_account_id")]
        public string ChannelAccountId { get; set; }

        /// <summary>
        /// 支付方式详情列表
        /// </summary>
        [JsonProperty("payment_methods")]
        public List<PayChannelAccountPaymentMethodDetailDto> PaymentMethods { get; set; }

        /// <summary>
        /// 账户创建时间
        /// </summary>
        [JsonProperty("creation_time")]
        public DateTime CreationTime { get; set; }
    }
}
