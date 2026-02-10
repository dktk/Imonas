using CsvHelper;

using Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PspConnectors.Methods.Noda.NodaCsv
{
    public class NodaPayCsvService : GenericCsvBasedPspService<Transaction>
    {
        public const string PspName = "NodaPay";

        public NodaPayCsvService(IConfiguration configuration, ILogger<NodaPayCsvService> logger) : base(configuration, logger, PspName) { }

        public override Action<CsvContext> CsvConfigurator { get; set; } = _ => { };

        public override Func<Transaction, TargetData> TargetDataConverter { get; set; }
            = record => new TargetData
            {
                Id = record.Id.ToString(),
                Amount = record.Amount,
                Date = record.Date,
                Email = record.Email,
                Psp = PspName,
                Currency = record.Currency,
                TxStatus = record.Status,
                ClientId = record.CustomerId,
                ReferenceCode = record.ReferenceId,
                Description = GetDescription(),

                //switch
                //{
                //    "Done" => Status.Successful,
                //    "Failed" => Status.Failed,
                //    "Awaiting confirmation" => Status.Pending,
                //    "Processing" => Status.Pending
                //},
                TxId = record.OrderId
            };

        private static string GetDescription()
        {
            throw new NotImplementedException();
        }

        public async override Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            return await base.GetTransactionsAsync(from, to);
        }
    }
}
