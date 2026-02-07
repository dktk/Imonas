namespace PspConnectors.Methods.Paysage
{
    public class Transaction
    {
        public BillingAddress BillingAddress { get; set; }
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
        public CreditCard CreditCard { get; set; }
        public Shop Shop { get; set; }
        public string Uid { get; set; }
        public long Id { get; set; }
        public string PaymentMethodType { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Tracking_Id { get; set; }
        public string Type { get; set; }
        public long OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PaidAt { get; set; }
        public AdditionalData AdditionalData { get; set; }
    }

    public class BillingAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
    }

    public class Customer
    {
        public string Ip { get; set; }
        public string Email { get; set; }
    }

    public class Payment
    {
        public long GatewayId { get; set; }
        public string RefId { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string AuthCode { get; set; }
        public string BankCode { get; set; }
    }

    public class CreditCard
    {
        public string Holder { get; set; }
        public string Brand { get; set; }
        public string Last4 { get; set; }
        public string First1 { get; set; }
        public string Bin { get; set; }
        public string IssuerCountry { get; set; }
        public string IssuerName { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
    }

    public class Shop
    {
        public int Id { get; set; }
    }

    public class AdditionalData
    {
        public Browser Browser { get; set; }
        public TdSecure TdSecure { get; set; }
    }

    public class Browser
    {
        public string UserAgent { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public string Language { get; set; }
    }

    public class TdSecure
    {
        public string AcsUrl { get; set; }
        public string Status { get; set; }
    }

    public class PaysageReponse
    {
        public Transaction[] Transactions { get; set; }
        public int Count { get; set; }
        public bool HasMore {  get; set; }
        public long FirstObjectId { get; set; }
        public long LastObjectId { get; set; }
    }
}