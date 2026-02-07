namespace PspConnectors.Methods.Noda.NodaCsv
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string OrderId { get; set; }
        public string Type { get; set; }
        public string Bank { get; set; }
        public string Shop { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int CustomerId { get; set; }
        public string Email { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public string ReferenceId { get; set; }
        public string RemitterIban { get; set; }
        public string RemitterName { get; set; }
        public Guid OriginalPaymentId { get; set; }
        public string Method { get; set; }
        public string MerchantName { get; set; }
        public DateTime? SettlementDate { get; set; }
        public decimal? FeeAmount { get; set; }
        public decimal? SettlementAmount { get; set; }
    }
}
