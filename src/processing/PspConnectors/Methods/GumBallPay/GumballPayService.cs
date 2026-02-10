using CsvHelper;

using Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PspConnectors.Methods.GumBallPay
{
    public class GumballPayService : GenericCsvBasedPspService<Transaction>
    {
        public const string PspName = "GumBallPay";

        public GumballPayService(IConfiguration configuration, ILogger<GumballPayService> logger) : base(configuration, logger, PspName) { }

        public override Action<CsvContext> CsvConfigurator { get; set; } = context => context.RegisterClassMap<TransactionMap>();

        public override Func<Transaction, TargetData> TargetDataConverter { get; set; }
            = record => new TargetData
            {
                Id = record.TxID.ToString(),
                TxId = record.InvoiceNo,
                Amount = record.OrderAmount,
                Date = record.OrderCreated,
                Email = record.CustomerEmail,
                Currency = record.Currency,
                Psp = PspName,
                TxStatus = record.TxStatus,
                ClientId = GetClientId(),
                Description = record.Description,
                ReferenceCode = GetReference(),

                //switch
                //{
                //    "approved" => Status.Successful,
                //    "filtered" => Status.Filtered,
                //    "declined" => Status.Declined,
                //}
            };

        private static int GetClientId()
        {
            throw new NotImplementedException();
        }

        private static string GetReference()
        {
            throw new NotImplementedException();
        }
    }
}
