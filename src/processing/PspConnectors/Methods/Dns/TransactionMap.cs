using CsvHelper.Configuration;

namespace PspConnectors.Methods.Dns
{
    internal class TransactionMap : ClassMap<Transaction>
    {
        public TransactionMap()
        {
            //DateTime.ParseExact("2024-07-01T00:02:19.00", "yyyy-MM-ddTHH:mm:ss.ff", System.Globalization.CultureInfo.InvariantCulture);

            Map(m => m.Txid).Name("Txid");
            Map(m => m.CreatedDateServerTZ).Name("Created Date (Server TZ)").TypeConverterOption.Format("yyyy-MM-ddTHH:mm:ss.ff");
            Map(m => m.CreatedDateUserTZ).Name("Created Date (User TZ)").TypeConverterOption.Format("yyyy-MM-ddTHH:mm:ss.ff");
            Map(m => m.CreatedDateServerNoTime).Name("Created Date (Server TZ, no time)");
            Map(m => m.BankDateServerTZ).Name("Bank Date (Server TZ)");
            Map(m => m.BankDateUserTZ).Name("Bank Date (User TZ)");
            Map(m => m.Merchant).Name("Merchant");
            Map(m => m.EndPointID).Name("End-Point ID");
            Map(m => m.ProjectID).Name("Project ID");
            Map(m => m.ProjectName).Name("Project Name");
            Map(m => m.Currency).Name("Currency");
            Map(m => m.SiteURL).Name("Site URL");
            Map(m => m.CardType).Name("Card type");
            Map(m => m.Ip).Name("Ip");
            Map(m => m.Type).Name("Type");
            Map(m => m.Status).Name("Status");
            Map(m => m.Amount).Name("Amount");
            Map(m => m.ProcessorTxID).Name("Processor tx ID");
            Map(m => m.CardNo).Name("Card No");
            Map(m => m.Name).Name("Name");
            Map(m => m.Email).Name("Email");
            Map(m => m.Phone).Name("Phone");
            Map(m => m.Address).Name("Address");
            Map(m => m.Zip).Name("Zip");
            Map(m => m.City).Name("City");
            Map(m => m.State).Name("State");
            Map(m => m.Country).Name("Country");
            Map(m => m.FirstName).Name("First Name");
            Map(m => m.LastName).Name("Last Name");
            Map(m => m.OrderDescription).Name("Order Description");
            Map(m => m.MerchantOID).Name("Merchant OID");
            Map(m => m.CaptureAmount).Name("Capture amount");
            Map(m => m.ApprovalCode).Name("Approval Code");
            Map(m => m.ViewReceiptURL).Name("View Receipt URL");
            Map(m => m.BankName).Name("Bank name");
            Map(m => m.BINCountry).Name("BIN country");
            Map(m => m.BINCountryCode).Name("BIN country code");
            Map(m => m.InitialTxBankDateServerTZ).Name("Initial tx bank date (Server TZ)").TypeConverterOption.Format("yyyy-MM-ddTHH:mm:ss.ff");
            Map(m => m.InitialTxAmount).Name("Initial tx Amount");
            Map(m => m.CreationTime).Name("Creation time").TypeConverterOption.Format("yyyy-MM-ddTHH:mm:ss.ff");
            Map(m => m.CustomerIPCountry).Name("Customer IP address country");
        }
    }
}
