using Domain;

using Microsoft.Extensions.Logging;



namespace PspConnectors.Methods.Rastpay
{
    public class RastPayService : BaseMethodService
    {
        public const string PspName = "RastPay";
        private readonly RastPayClient _rastPayClient;

        public RastPayService(RastPayClient rastPayClient, ILogger<RastPayService> logger)
            : base(logger, PspName)
        {
            _rastPayClient = rastPayClient;
        }

        public async override Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            var payments = await _rastPayClient.GetPayments(from, to);

            return payments.Select(x => new TargetData
            {
                Id = x.Id,
                Amount = x.Amount,
                Date = DateTime.MinValue,
                Email = x.Customer.Email,
                Psp = PspName,
                Currency = x.Currency,
                TxId = x.ReferenceId,
                TxStatus = x.State,
                //switch
                //{
                //    "COMPLETED" => Status.Successful,
                //    "CANCELLED" => Status.Cancelled,
                //    "DECLINED" => Status.Declined
                //}
            }).ToArray();
        }
    }
}
