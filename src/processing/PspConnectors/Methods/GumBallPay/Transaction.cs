namespace PspConnectors.Methods.GumBallPay
{
    public class Transaction
    {
        public DateTime OrderCreated { get; set; }
        public DateTime OrderCreatedNoTime { get; set; }
        public string InvoiceNo { get; set; }
        public int OrderID { get; set; }
        public string RebillTx { get; set; }
        public DateTime StatusChangeDate { get; set; }
        public string Description { get; set; }
        public decimal OrderAmount { get; set; }
        public string Currency { get; set; }
        public int TxGateID { get; set; }
        public string CardName { get; set; }
        public string CardHolder { get; set; }
        public string CardNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerIP { get; set; }
        public string CustomerPhone { get; set; }
        public string MerchantIP { get; set; }
        public string MerchantSite { get; set; }
        public int MerchantID { get; set; }
        public string Merchant { get; set; }
        public int ProjectID { get; set; }
        public string Project { get; set; }
        public int EndPointID { get; set; }
        public string EndPoint { get; set; }
        public string ErrorMessage { get; set; }
        public int TxID { get; set; }
        public string TxStatus { get; set; }
        public string TxType { get; set; }
        public decimal TxAmount { get; set; }
        public TimeSpan TxCreationDuration { get; set; }
        public int TxGateID2 { get; set; }
        public string TxInfo { get; set; }
        public int? FraudScore { get; set; }
        public string BankName { get; set; }
        public string RequestSource { get; set; }
        public string TxGateDescriptor { get; set; }
        public string Destination { get; set; }
        public decimal ClientCommission { get; set; }
        public string DestinationCardNo { get; set; }
        public string DestinationBankName { get; set; }
        public string BINCountryCode { get; set; }
        public string BINCountry { get; set; }
        public string DestinationBINCountryCode { get; set; }
        public string DestinationBINCountry { get; set; }
        public string CardInformation { get; set; }
        public string CardAccountType { get; set; }
        public string CardLevel { get; set; }
        public string CardProductDescription { get; set; }
        public string DestinationCardAccountType { get; set; }
        public string DestinationCardLevel { get; set; }
        public string DestinationCardProductDescription { get; set; }
    }
}
