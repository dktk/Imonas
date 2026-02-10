using System.Text.Json.Serialization;

namespace PspConnectors.Methods.Rastpay
{
    public class TransactionWrapper
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("result")]
        public List<Transaction> Result { get; set; }

        // todo: USE this property
        [JsonPropertyName("hasMore")]
        public bool HasMore { get; set; }
    }

    public class Transaction
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("referenceId")]
        public string ReferenceId { get; set; }

        [JsonPropertyName("paymentType")]
        public string PaymentType { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("internalState")]
        public string InternalState { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("customerAmount")]
        public decimal CustomerAmount { get; set; }

        [JsonPropertyName("customerCurrency")]
        public string CustomerCurrency { get; set; }

        [JsonPropertyName("externalResultCode")]
        public string ExternalResultCode { get; set; }

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("customer")]
        public Customer Customer { get; set; }

        [JsonPropertyName("billingAddress")]
        public BillingAddress BillingAddress { get; set; }

        [JsonPropertyName("terminalName")]
        public string TerminalName { get; set; }
    }

    public class Customer
    {
        [JsonPropertyName("referenceId")]
        public string ReferenceId { get; set; }

        [JsonPropertyName("citizenshipCountryCode")]
        public string CitizenshipCountryCode { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("kycStatus")]
        public bool KycStatus { get; set; }

        [JsonPropertyName("paymentInstrumentKycStatus")]
        public bool PaymentInstrumentKycStatus { get; set; }
    }

    public class BillingAddress
    {
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }
    }
}
