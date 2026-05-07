using Abp.Application.Services.Dto;
using System;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    public class PayChannelAccountPaymentMethodDto : EntityDto<Guid>
    {
        [JsonProperty("account_id")]
        public Guid AccountId { get; set; }

        [JsonProperty("payment_type")]
        public string? PaymentType { get; set; }

        // ========== Account Details ==========
        [JsonProperty("account_holder_name")]
        public string? AccountHolderName { get; set; }

        [JsonProperty("account_number")]
        public string? AccountNumber { get; set; }

        [JsonProperty("account_routing_number")]
        public string? AccountRoutingNumber { get; set; }

        [JsonProperty("memo")]
        public string? Memo { get; set; }

        [JsonProperty("swift_bic")]
        public string? SwiftBic { get; set; }

        [JsonProperty("intermediary_swift_bic")]
        public string? IntermediarySwiftBic { get; set; }

        // ========== Institution Info ==========
        [JsonProperty("institution_name")]
        public string? InstitutionName { get; set; }

        [JsonProperty("institution_address_line1")]
        public string? InstitutionAddressLine1 { get; set; }

        [JsonProperty("institution_city")]
        public string? InstitutionCity { get; set; }

        [JsonProperty("institution_state")]
        public string? InstitutionState { get; set; }

        [JsonProperty("institution_postal_code")]
        public string? InstitutionPostalCode { get; set; }

        [JsonProperty("institution_country_code")]
        public string? InstitutionCountryCode { get; set; }

        // ========== Holder Address ==========
        [JsonProperty("holder_address_line1")]
        public string? HolderAddressLine1 { get; set; }

        [JsonProperty("holder_city")]
        public string? HolderCity { get; set; }

        [JsonProperty("holder_state")]
        public string? HolderState { get; set; }

        [JsonProperty("holder_postal_code")]
        public string? HolderPostalCode { get; set; }

        [JsonProperty("holder_country_code")]
        public string? HolderCountryCode { get; set; }
    }
}
