namespace PspConnectors.Methods.CubixPay
{
    public class Transaction
    {
        public string Company { get; set; }
        public string TransactionUUID { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal SettlementAmount { get; set; }
        public string SettlementCurrency { get; set; }
        public string Operation { get; set; }
        public string CreditCard { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string IpAddress { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerPostcode { get; set; }
        public string CustomerPhone { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string Descriptor { get; set; }
        public string CustomData { get; set; }
        public string BinCountry { get; set; }
        public string BankCode { get; set; }
        public string AgentInfo { get; set; }
        public string Language { get; set; }
        public int ColorDepth { get; set; }
        public int ScreenHeight { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenTZ { get; set; }
        public bool JavaEnabled { get; set; }
        public string AcceptHeader { get; set; }
    }
}
