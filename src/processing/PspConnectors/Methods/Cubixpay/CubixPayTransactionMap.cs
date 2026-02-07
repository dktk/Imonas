using CsvHelper.Configuration;

namespace PspConnectors.Methods.CubixPay
{
    public class CubixPayTransactionMap : ClassMap<Transaction>
    {
        public CubixPayTransactionMap()
        {
            Map(m => m.Company).Name("Company");
            Map(m => m.TransactionUUID).Name("Transaction UUID");
            Map(m => m.PaymentMethod).Name("Payment Method");
            Map(m => m.Status).Name("Status");
            Map(m => m.Amount).Name("Amount");
            Map(m => m.Currency).Name("Currency");
            Map(m => m.SettlementAmount).Name("Settlement Amount");
            Map(m => m.SettlementCurrency).Name("Settlement Currency");
            Map(m => m.Operation).Name("Operation");
            Map(m => m.CreditCard).Name("Credit Card");
            Map(m => m.ReferenceNo).Name("Reference No");
            Map(m => m.CreateDate).Name("Create Date");
            Map(m => m.UpdateDate).Name("Update Date");
            Map(m => m.CustomerName).Name("Customer Name");
            Map(m => m.CustomerEmail).Name("Customer Email");
            Map(m => m.IpAddress).Name("IP Address");
            Map(m => m.CustomerCountry).Name("Customer Country");
            Map(m => m.CustomerPostcode).Name("customerPostcode");
            Map(m => m.CustomerPhone).Name("Customer Phone");
            Map(m => m.ResponseCode).Name("Response Code");
            Map(m => m.ResponseMessage).Name("Response Message");
            Map(m => m.Descriptor).Name("Descriptor");
            Map(m => m.CustomData).Name("Custom Data");
            Map(m => m.BinCountry).Name("Bin Country");
            Map(m => m.BankCode).Name("Bank Code");
            Map(m => m.AgentInfo).Name("agentInfo");
            Map(m => m.Language).Name("Language");
            Map(m => m.ColorDepth).Name("Color Depth");
            Map(m => m.ScreenHeight).Name("Screen Height");
            Map(m => m.ScreenWidth).Name("Screen Width");
            Map(m => m.ScreenTZ).Name("Screen TZ");
            Map(m => m.JavaEnabled).Name("Java Enabled");
            Map(m => m.AcceptHeader).Name("Accept Header");
        }
    }
}
