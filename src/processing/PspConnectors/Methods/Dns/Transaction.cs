namespace PspConnectors.Methods.Dns
{
    public class Transaction
    {
        public string Txid { get; set; }
        public DateTime CreatedDateServerTZ { get; set; }
        public DateTime? CreatedDateUserTZ { get; set; }
        public DateTime? CreatedDateServerNoTime { get; set; }
        public string BankDateServerTZ { get; set; }
        public string BankDateUserTZ { get; set; }
        public string Merchant { get; set; }
        public int? EndPointID { get; set; }
        public int? ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string Currency { get; set; }
        public string SiteURL { get; set; }
        public string CardType { get; set; }
        public string Ip { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string ProcessorTxID { get; set; }
        public string CardNo { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OrderDescription { get; set; }
        public string MerchantOID { get; set; }
        public decimal? CaptureAmount { get; set; }
        public string ApprovalCode { get; set; }
        public string ViewReceiptURL { get; set; }
        public string BankName { get; set; }
        public string BINCountry { get; set; }
        public string BINCountryCode { get; set; }
        public DateTime? InitialTxBankDateServerTZ { get; set; }
        public decimal? InitialTxAmount { get; set; }
        public DateTime? CreationTime { get; set; }
        public string CustomerIPCountry { get; set; }
    }
}
