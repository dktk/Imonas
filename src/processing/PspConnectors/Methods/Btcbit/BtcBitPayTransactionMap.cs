using CsvHelper.Configuration;
using System.Globalization;

namespace PspConnectors.Methods.Btcbit
{
    public class BtcBitPayTransactionMap : ClassMap<Transaction>
    {
        public BtcBitPayTransactionMap()
        {
            Map(m => m.Id).Name("# id");
            Map(m => m.MerchantId).Name("Merchant Id");
            Map(m => m.Merchant).Name("Merchant");
            Map(m => m.Action).Name("Action");
            Map(m => m.Method).Name("Method");
            Map(m => m.PaymentChannel).Name("Payment Channel");
            Map(m => m.CardOrAccount).Name("Card / Account");
            Map(m => m.Status).Name("Status");
            Map(m => m.OrderAmount).Name("Order Amount").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.PayoutAmount).Name("Payout Amount").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.Currency).Name("Currency");
            Map(m => m.CustomerEmail).Name("Customer Email");
            Map(m => m.CreationDate).Name("Creation Date").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.PaymentDate).Name("Payment Date").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.ExternalId).Name("Ext. ID");
            Map(m => m.OrderNr).Name("Order Nr.");
            Map(m => m.VariableFee).Name("Variable fee").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.FixedFee).Name("Fixed fee").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.CardBin).Name("Card bin");
            Map(m => m.Bank).Name("Bank");
            Map(m => m.CardType).Name("Card type");
            Map(m => m.CardSubtype).Name("Card subtype");
            Map(m => m.CardLevel).Name("Card level");
            Map(m => m.BinCountry).Name("Bin country");
            Map(m => m.ClientId).Name("Client ID");
            Map(m => m.ClientCountry).Name("Client country");
            Map(m => m.AdvSystemMessage).Name("Adv. Syst. message");
        }
    }
}
