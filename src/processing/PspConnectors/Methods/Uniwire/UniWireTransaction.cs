using System.Text.Json.Serialization;

namespace PspConnectors.Methods.Uniwire
{
    public class UniWireTransactionListResponse
    {
        [JsonPropertyName("data")]
        public UniWireTransaction[] Data { get; set; } = [];

        [JsonPropertyName("pagination")]
        public UniWirePagination? Pagination { get; set; }
    }

    public class UniWirePagination
    {
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
    }

    public class UniWireTransaction
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("kind")]
        public string Kind { get; set; } = string.Empty;

        [JsonPropertyName("txid")]
        public string TxId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public UniWireAmount? Amount { get; set; }

        [JsonPropertyName("invoice")]
        public UniWireInvoice? Invoice { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("confirmations")]
        public int Confirmations { get; set; }

        [JsonPropertyName("risk_level")]
        public string? RiskLevel { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("executed_at")]
        public DateTime? ExecutedAt { get; set; }

        [JsonPropertyName("confirmed_at")]
        public DateTime? ConfirmedAt { get; set; }

        [JsonPropertyName("currency_rates")]
        public Dictionary<string, decimal>? CurrencyRates { get; set; }

        [JsonPropertyName("zero_conf_status")]
        public string? ZeroConfStatus { get; set; }

        [JsonPropertyName("risk_data")]
        public object? RiskData { get; set; }
    }

    public class UniWireAmount
    {
        [JsonPropertyName("paid")]
        public decimal Paid { get; set; }

        [JsonPropertyName("paid_total")]
        public decimal? PaidTotal { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;
    }

    public class UniWireInvoice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("profile_id")]
        public string? ProfileId { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("customer_email")]
        public string? CustomerEmail { get; set; }
    }
}
