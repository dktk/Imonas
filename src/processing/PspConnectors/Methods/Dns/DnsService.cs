using CsvHelper;

using Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace PspConnectors.Methods.Dns
{
    // TODO: https://doc.dns-pay.com/integration/API_commands/services_transaction-report.html#services-transaction-report

    public class DnsService : GenericCsvBasedPspService<Transaction>
    {
        public const string PspName = "Payneteasy";
        
        public DnsService(IConfiguration configuration, ILogger<DnsService> logger)
            : base(configuration, logger, PspName) { }

        public override Action<CsvContext> CsvConfigurator { get; set; }
            = context => context.RegisterClassMap<TransactionMap>();

        public override Func<Transaction, TargetData> TargetDataConverter { get; set; }
            = record => new TargetData
            {
                Id = record.Txid,
                TxId = record.MerchantOID,
                Amount = record.Amount,
                Date = record.CreatedDateServerTZ,
                Email = record.Email,
                Psp = PspName,
                Currency = record.Currency,
                TxStatus = record.Status,
                //switch
                //{
                //    "approved" => Status.Successful,
                //    "declined" => Status.Declined,
                //    "filtered" => Status.Filtered
                //}
            };
    }
}
