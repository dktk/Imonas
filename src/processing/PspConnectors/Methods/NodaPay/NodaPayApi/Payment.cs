using System.Text.Json.Serialization;

namespace PspConnectors.Methods.Noda.NodaPay
{
    public class Payment
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("merchantPaymentId")]
        public string MerchantPaymentId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("finalisedDate")]
        public DateTime FinalisedDate { get; set; }
    }

    public class NodaPaymentResponse
    {
        [JsonPropertyName("payments")]
        public List<Payment> Payments { get; set; }

        [JsonPropertyName("continuationToken")]
        public string ContinuationToken { get; set; }
    }
}