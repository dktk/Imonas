using Azure.Messaging.EventGrid.SystemEvents;

using CsvHelper;

using Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PspConnectors.Methods.Skrill
{
    public class SkrillService : GenericCsvBasedPspService<Transaction>
    {
        public const string PspName = "Skrill";

        public SkrillService(IConfiguration configuration, ILogger<SkrillService> logger)
            : base(configuration, logger, PspName) { }


        public override Action<CsvContext> CsvConfigurator { get; set; }
            = context => context.RegisterClassMap<TransactionMap>();

        public override Func<Transaction, TargetData> TargetDataConverter { get; set; }
            = record => new TargetData
            {
                Id = record.ID.ToString(),
                TxId = record.Reference,
                Amount = record.AmountSent ?? 0,
                Date = record.TimeCET,
                Email = record.AccountID.ToString(),
                Psp = PspName,
                Currency = record.Currency,
                TxStatus = record.Status,
                ClientId = record.AccountID,
                Description = record.TransactionDetails,
                ReferenceCode = record.Reference,

                //switch
                //{
                //    "processed" => Status.Successful,
                //}
            };

        public override async Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            var results = await base.GetTransactionsAsync(from, to);
            var x = results.DistinctBy(x => x.TxStatus).ToArray();

            return results
                        .Where(x => !string.IsNullOrWhiteSpace(x.TxId))
                        .ToArray();
        }
    }
}
