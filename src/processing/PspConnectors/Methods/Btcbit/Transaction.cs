namespace PspConnectors.Methods.Btcbit
{
    public class Transaction
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string Merchant { get; set; }
        public string Action { get; set; }
        public string Method { get; set; }
        public string PaymentChannel { get; set; }
        public string CardOrAccount { get; set; }
        public string Status { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public string Currency { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string ExternalId { get; set; }
        public string OrderNr { get; set; }
        public decimal VariableFee { get; set; }
        public decimal FixedFee { get; set; }
        public string CardBin { get; set; }
        public string Bank { get; set; }
        public string CardType { get; set; }
        public string CardSubtype { get; set; }
        public string CardLevel { get; set; }
        public string BinCountry { get; set; }
        public int ClientId { get; set; }
        public string ClientCountry { get; set; }
        public string AdvSystemMessage { get; set; }
    }
}