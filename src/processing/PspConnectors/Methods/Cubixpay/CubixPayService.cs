using CsvHelper;

using Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PspConnectors.Methods.CubixPay
{
    public class CubixPayService : GenericCsvBasedPspService<Transaction>
    {
        public const string PspName = "CubixPay";
        private readonly string[] _files;

        public override Action<CsvContext> CsvConfigurator { get; set; } = context => { context.RegisterClassMap<CubixPayTransactionMap>(); };
        public override Func<Transaction, TargetData> TargetDataConverter { get; set; }
            = record => new TargetData
            {
                Id = record.TransactionUUID,
                Amount = record.Amount,
                Date = record.CreateDate,
                Email = record.CustomerEmail,
                Psp = PspName,
                Currency = record.Currency,
                TxId = record.ReferenceNo,
                TxStatus = record.Status,
                //switch
                //{
                //    "ERROR" => Status.Error,
                //    "APPROVED" => Status.Successful,
                //    "DECLINED" => Status.Declined
                //}
            };

        public CubixPayService(IConfiguration configuration, ILogger<CubixPayService> logger)
            : base(configuration, logger, PspName) { }

        public override async Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            return await base.GetTransactionsAsync(from, to);
        }
    }
}
