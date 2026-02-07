using CsvHelper.Configuration;
using System.Globalization;

namespace PspConnectors.Methods.Skrill
{
    public class TransactionMap : ClassMap<Transaction>
    {
        public TransactionMap()
        {
            //02 Feb 25 00:00
            Map(m => m.ID).Name("ID");
            Map(m => m.TimeCET).Name("Time (CET)").TypeConverterOption.Format("dd MMM yy HH:mm").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.Type).Name("Type");
            Map(m => m.TransactionDetails).Name("Transaction Details");
            Map(m => m.MinusAmount).Name("[-]").Optional();
            Map(m => m.PlusAmount).Name("[+]").Optional();
            Map(m => m.Status).Name("Status");
            Map(m => m.Balance).Name("Balance");
            Map(m => m.Reference).Name("Reference").Optional();
            Map(m => m.AmountSent).Name("Amount Sent").Optional();
            Map(m => m.CurrencySent).Name("Currency Sent").Optional();
            Map(m => m.MoreInformation).Name("More Information").Optional();
            Map(m => m.CorrespondingTransactionID).Name("ID of the corresponding Skrill transaction").Optional();
            Map(m => m.PaymentInstrument).Name("Payment Instrument").Optional();
            Map(m => m.Country).Name("Country");
            Map(m => m.IpCountry).Name("IpCountry").Optional();
            Map(m => m.InstrumentCountry).Name("Instrument Country").Optional();
            Map(m => m.AccountID).Name("Account ID");
            Map(m => m.Currency).Name("Currency");
            Map(m => m.Region).Name("Region");
        }
    }
}