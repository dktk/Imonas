using CsvHelper.Configuration;

namespace PspConnectors.Methods.GumBallPay
{
    public sealed class TransactionMap : ClassMap<Transaction>
    {
        public TransactionMap()
        {
            Map(m => m.OrderCreated).Name("Order Created");
            Map(m => m.OrderCreatedNoTime).Name("Order Created (no time)").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.InvoiceNo).Name("Invoice No");
            Map(m => m.OrderID).Name("Order ID");
            Map(m => m.RebillTx).Name("Rebill Tx");
            Map(m => m.StatusChangeDate).Name("Status Change Date");
            Map(m => m.Description).Name("Description");
            Map(m => m.OrderAmount).Name("Order Amount");
            Map(m => m.Currency).Name("Currency");
            Map(m => m.TxGateID).Name("Tx Gate ID");
            Map(m => m.CardName).Name("Card Name");
            Map(m => m.CardHolder).Name("Card Holder");
            Map(m => m.CardNumber).Name("Card Number");
            Map(m => m.RoutingNumber).Name("Routing Number");
            Map(m => m.AccountNumber).Name("Account Number");
            Map(m => m.CustomerEmail).Name("Customer Email");
            Map(m => m.CustomerIP).Name("Customer IP");
            Map(m => m.CustomerPhone).Name("Customer Phone");
            Map(m => m.MerchantIP).Name("Merchant IP");
            Map(m => m.MerchantSite).Name("Merchant Site");
            Map(m => m.MerchantID).Name("Merchant ID");
            Map(m => m.Merchant).Name("Merchant");
            Map(m => m.ProjectID).Name("Project ID");
            Map(m => m.Project).Name("Project");
            Map(m => m.EndPointID).Name("End-Point ID");
            Map(m => m.EndPoint).Name("End-Point");
            Map(m => m.ErrorMessage).Name("Error Message");
            Map(m => m.TxID).Name("Tx ID");
            Map(m => m.TxStatus).Name("Tx Status");
            Map(m => m.TxType).Name("Tx Type");
            Map(m => m.TxAmount).Name("Tx Amount");
            Map(m => m.TxCreationDuration).Name("Tx Creation Duration");
            Map(m => m.TxGateID2).Name("Tx Gate ID");
            Map(m => m.TxInfo).Name("Tx Info");
            Map(m => m.FraudScore).Name("Fraud Score");
            Map(m => m.BankName).Name("Bank Name");
            Map(m => m.RequestSource).Name("Request Source");
            Map(m => m.TxGateDescriptor).Name("Tx Gate Descriptor");
            Map(m => m.Destination).Name("Destination");
            Map(m => m.ClientCommission).Name("Client commission");
            Map(m => m.DestinationCardNo).Name("Destination Card No");
            Map(m => m.DestinationBankName).Name("Destination bank name");
            Map(m => m.BINCountryCode).Name("BIN country code");
            Map(m => m.BINCountry).Name("BIN country");
            Map(m => m.DestinationBINCountryCode).Name("Destination BIN country code");
            Map(m => m.DestinationBINCountry).Name("Destination BIN country");
            Map(m => m.CardInformation).Name("Card information");
            Map(m => m.CardAccountType).Name("Card Account Type");
            Map(m => m.CardLevel).Name("Card Level");
            Map(m => m.CardProductDescription).Name("Card Product Description");
            Map(m => m.DestinationCardAccountType).Name("Destination Card Account Type");
            Map(m => m.DestinationCardLevel).Name("Destination Card Level");
            Map(m => m.DestinationCardProductDescription).Name("Destination Card Product Description");
        }
    }
}
