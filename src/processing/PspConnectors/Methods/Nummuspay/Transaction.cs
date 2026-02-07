namespace PspConnectors.Methods.Nummuspay
{
    public class Transaction
    {
        public int ID { get; set; }
        public string TransactionCode { get; set; }
        public string ProviderTransactionCode { get; set; }
        public Project Project { get; set; }
        public ProjectGateway ProjectGateWay { get; set; }
        public PublicCheckoutPage PublicCheckoutPage { get; set; }
        public Customer Customer { get; set; }
        public CreditCard CreditCard { get; set; }
        public bool CouponCodeRedeemed { get; set; }
        public List<OrderUnit> OrderUnits { get; set; }
        public string TransactionStatus { get; set; }
        public string TransactionType { get; set; }
        public decimal AmountToReceive { get; set; }
        public decimal Fee { get; set; }
        public decimal Gross { get; set; }
        public decimal AmountReceived { get; set; }
        public string ClientIP { get; set; }
        public string CurrencyCustomer { get; set; }
        public string CurrencyOriginal { get; set; }
        public string CurrencyPaid { get; set; }
        public bool IsDeleted { get; set; }
        public string ProviderResponse { get; set; }
        public string IPCountryName { get; set; }
        public string IPCityName { get; set; }
        public string IPZipCode { get; set; }
        public string IPLatitude { get; set; }
        public string IPLongitude { get; set; }
        public bool IsLive { get; set; }
        public DateTime ProviderDateTime { get; set; }
        public DateTime CreatedDatetime { get; set; }
        public string Metadata { get; set; }
    }

    public class Project
    {
        public int ID { get; set; }
        public string Subdomain { get; set; }
    }

    public class ProjectGateway
    {
        public string FriendlyName { get; set; }
        public int ID { get; set; }
    }

    public class PublicCheckoutPage
    {
        public int ID { get; set; }
        public string PageNickname { get; set; }
    }

    public class Customer
    {
        public int ID { get; set; }
        public string Token { get; set; }
        public bool Active { get; set; }
        public string Email { get; set; }
        public string ProjectSubdomain { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingZip { get; set; }
        public bool UseSameAsBilling { get; set; }
        public string BillingAddress1 { get; set; }
        public string BillingCountry { get; set; }
        public string BillingState { get; set; }
        public string BillingCity { get; set; }
        public string BillingZip { get; set; }
        public bool IsDeleted { get; set; }
        public string CustomerAreaLink { get; set; }
        public List<CreditCard> CreditCards { get; set; }
        public List<object> BankAccounts { get; set; }
        public List<object> ProviderData { get; set; }
    }

    public class CreditCard
    {
        public string Token { get; set; }
        public bool Active { get; set; }
        public string CardType { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Number { get; set; }
        public string BillingAddress1 { get; set; }
        public string Zip { get; set; }
        public string Cvv { get; set; }
    }

    public class OrderUnit
    {
        public string Title { get; set; }
        public int ProductID { get; set; }
        public string ProductType { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Currency { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string BillingModel { get; set; }
        public bool PrepaidUsage { get; set; }
        public List<object> Vendors { get; set; }
    }

}
