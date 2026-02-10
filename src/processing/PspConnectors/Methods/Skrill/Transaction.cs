namespace PspConnectors.Methods.Skrill
{
    public class Transaction
    {
        public long ID { get; set; }
        public DateTime TimeCET { get; set; }
        public string Type { get; set; }
        public string TransactionDetails { get; set; }
        public decimal? MinusAmount { get; set; }
        public decimal? PlusAmount { get; set; }
        public string Status { get; set; }
        public decimal Balance { get; set; }
        public string Reference { get; set; }
        public decimal? AmountSent { get; set; }
        public string CurrencySent { get; set; }
        public string MoreInformation { get; set; }
        public long? CorrespondingTransactionID { get; set; }
        public string PaymentInstrument { get; set; }
        public string Country { get; set; }
        public string IpCountry { get; set; }
        public string InstrumentCountry { get; set; }
        public int AccountID { get; set; }
        public string Currency { get; set; }
        public string Region { get; set; }
    }
}
