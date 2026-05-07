using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    public class PayChannelAccountPaymentMethodDetailDto
    {
        [JsonProperty("id")]
        public System.Guid Id { get; set; }

        [JsonProperty("payment_type", NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentType { get; set; }

        [JsonProperty("account_detail", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentMethodAccountDetailDto AccountDetail { get; set; }

        [JsonProperty("institution_information", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentMethodInstitutionDto InstitutionInformation { get; set; }

        [JsonProperty("account_holder_address", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentMethodAddressDto AccountHolderAddress { get; set; }
    }

    public class PaymentMethodAccountDetailDto
    {
        [JsonProperty("account_holder_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountHolderName { get; set; }

        [JsonProperty("account_number", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountNumber { get; set; }

        [JsonProperty("account_routing_number", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountRoutingNumber { get; set; }

        [JsonProperty("memo", NullValueHandling = NullValueHandling.Ignore)]
        public string Memo { get; set; }

        [JsonProperty("swift_bic", NullValueHandling = NullValueHandling.Ignore)]
        public string SwiftBic { get; set; }

        [JsonProperty("intermediary_swift_bic", NullValueHandling = NullValueHandling.Ignore)]
        public string IntermediarySwiftBic { get; set; }
    }

    public class PaymentMethodInstitutionDto
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("address_line1", NullValueHandling = NullValueHandling.Ignore)]
        public string AddressLine1 { get; set; }

        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("postal_code", NullValueHandling = NullValueHandling.Ignore)]
        public string PostalCode { get; set; }

        [JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
        public string CountryCode { get; set; }
    }

    public class PaymentMethodAddressDto
    {
        [JsonProperty("address_line1", NullValueHandling = NullValueHandling.Ignore)]
        public string AddressLine1 { get; set; }

        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("postal_code", NullValueHandling = NullValueHandling.Ignore)]
        public string PostalCode { get; set; }

        [JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
        public string CountryCode { get; set; }
    }
}
