using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientPlatform.Pay.Channels.SunPay.Dto
{
    public class SunPayCollectionCallbackDto
    {
        [JsonProperty("biz_status")]
        public string BizStatus { get; set; }

        [JsonProperty("biz_type")]
        public string BizType { get; set; }

        [JsonProperty("data")]
        public SunPayCollectionCallbackData Data { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public class SunPayCollectionCallbackData
    {
        [JsonProperty("order_no")]
        public string OrderNo { get; set; }

        [JsonProperty("out_order_no")]
        public string OutOrderNo { get; set; }

        [JsonProperty("out_user_id")]
        public string OutUserId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("creation_time")]
        public long? CreationTime { get; set; }

        [JsonProperty("completion_time")]
        public long? CompletionTime { get; set; }

        // --- Settlement Fields ---

        [JsonProperty("settlement_status")]
        public string SettlementStatus { get; set; }

        [JsonProperty("settlement_currency")]
        public string SettlementCurrency { get; set; }

        [JsonProperty("settlement_amount")]
        public decimal? SettlementAmount { get; set; }

        [JsonProperty("settlement_fee")]
        public decimal? SettlementFee { get; set; }

        [JsonProperty("settlement_fee_currency")]
        public string SettlementFeeCurrency { get; set; }

        [JsonProperty("settlement_time")]
        public long? SettlementTime { get; set; }


        [JsonProperty("sender")]
        public SunPayCollectionSender Sender { get; set; }

        [JsonProperty("recipient")]
        public SunPayCollectionRecipient Recipient { get; set; }
    }

    public class SunPayCollectionSender
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }

        [JsonProperty("iban")]
        public string Iban { get; set; }

        [JsonProperty("swift_bic")]
        public string SwiftBic { get; set; }
    }

    public class SunPayCollectionRecipient
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("iban")]
        public string Iban { get; set; }

        [JsonProperty("swift_bic")]
        public string SwiftBic { get; set; }

        [JsonProperty("bank_country")]
        public string BankCountry { get; set; }

        [JsonProperty("bank_address")]
        public string BankAddress { get; set; }

        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }

        [JsonProperty("bank_name")]
        public string BankName { get; set; }

        [JsonProperty("bank_account_holder_name")]
        public string BankAccountHolderName { get; set; }
    }
}
