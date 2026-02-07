namespace PspConnectors.Sources.SquarePay
{
    /*
     [Id]
      ,[Status]
      ,[InitialAmount]
      ,[InitialCurrency]
      ,[PaymentMethod]
      ,[UserId]
      ,[ProviderTransactionId]
      ,[Type]
      ,[UserEmail]
      ,[ConvertedAmount]
      ,[ConvertedCurrency]
     */
    public class RecordData
    {
        public Guid Id { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string ConvertedCurrency { get; set; }
        public string PaymentMethod { get; set; }
        public int UserId { get; set; }
        public Guid ProviderTransactionId { get; set; }
        public string Type { get; set; }
        public string UserEmail { get; set; }
        public string Status { get; set; }
        public string DeclineMessage { get; set; }
        public DateTime LastModified { get; set; }
    }
}
