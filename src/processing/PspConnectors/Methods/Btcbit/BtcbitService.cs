using CsvHelper;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain;

namespace PspConnectors.Methods.Btcbit
{
    public class BtcbitService : GenericCsvBasedPspService<Transaction>
    {
        public const string PspName = "BtcBit";
        public const string BuyAction = "BUY";

        public BtcbitService(IConfiguration configuration, ILogger<BtcbitService> logger)
            : base(configuration, logger, PspName) { }

        public override Action<CsvContext> CsvConfigurator { get; set; } = context => { context.RegisterClassMap<BtcBitPayTransactionMap>(); };

        public override Func<Transaction, TargetData> TargetDataConverter { get; set; } =
                record => new TargetData
                {
                    Id = record.Id.ToString(),
                    Amount = record.OrderAmount,
                    Date = record.PaymentDate ?? record.CreationDate,
                    Email = record.CustomerEmail,
                    Psp = PspName,
                    Currency = record.Currency,
                    TxId = record.ExternalId,
                    TxStatus = record.Status,
                    ClientId = record.ClientId,
                    Description = record.AdvSystemMessage,
                    ReferenceCode = record.OrderNr,

                    //switch
                    //{
                    //    "completed" => Status.Successful,
                    //    "failed" => Status.Failed,
                    //    "expired" => Status.Expired
                    //},
                    Action = record.Action == BuyAction ? PaymentAction.Buy : PaymentAction.Sell,
                    PaymentMethod = record.Method,
                    Merchant = record.Merchant,
                };
    }
}
