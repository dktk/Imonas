using PspConnectors.Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using SG.Common;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Domain;

//https://docs.noda.live/reference/check-payment-status

namespace PspConnectors.Methods.Noda.NodaPay
{
    public class NodaPayApiService : BaseMethodService
    {
        private enum NodaStatus
        {
            New,
            Processing,
            Failed,
            Done
        }

        public const string PspName = "NodaPay";

        private readonly HttpClient _httpClient;
        private readonly NodaPayConfig _pspConfig;
        private const string ReportsUrl = "https://api.noda.live/api/payments?datefrom={0}&dateto={1}&Limit=100&status={2}";

        public NodaPayApiService(IConfiguration configuration,
            ILogger<NodaPayApiService> logger,
            IOptions<NodaPayConfig> pspConfig,
            HttpClient httpClient) : base(logger, PspName)
        {
            _httpClient = httpClient;
            _pspConfig = pspConfig.Value;
        }

        // NOTE:
        // NOT my brightest moment
        // Calin be smarter in the future and do this better
        // in the meantime, just enjoy your stay in ROME or go to bed
        public override async Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)        
        {
            var result = new List<TargetData>();

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add(Constants.XApiKey, _pspConfig.ApiKey);

            DateTime? lastTransactionDate = null;
            var successfullTransactions = Array.Empty<TargetData>();

            var urlGetters = new List<Func<DateTime?, DateTime, string>> { GetSuccessfullTransactionsUrl, GetFailedTransactionsUrl };

            foreach (var urlGetter in urlGetters)
            {
                do
                {
                    var url = urlGetter(from, to);
                    _logger.LogInformation($"Retrieving Noda data from: {url}");
                    (lastTransactionDate, successfullTransactions) = await GetNodaTransactions(url);

                    if (successfullTransactions.Length == 1 && result.Any(x => x.TxId == successfullTransactions[0].TxId))
                    {
                        break;
                    }

                    result.AddRange(successfullTransactions);

                    if (lastTransactionDate.HasValue)
                    {
                        to = lastTransactionDate.Value.AddMilliseconds(-1);
                    }
                }
                while (lastTransactionDate != null);
            }

            return result.ToArray();
        }

        private async Task<(DateTime?, TargetData[])> GetNodaTransactions(string url)
        {
            var nodaResponse = await _httpClient.GetAsync(url);
            nodaResponse.EnsureSuccessStatusCode();

            var response = await nodaResponse?.Content?.ReadFromJsonAsync<NodaPaymentResponse>();

            // transactions are ordered in descending ordeer
            var lastTransactionData = response?.Payments?.LastOrDefault()?.CreatedDate;

            return (lastTransactionData, ConvertResponse(response?.Payments));
        }

        private TargetData[] ConvertResponse(List<Payment> payments)
        {
            return payments?.Select(x => new TargetData
            {
                Id = x.Id,
                Amount = x.Amount,
                Date = x.CreatedDate,
                Email = Constants.NotAvailable,
                Psp = PspName,
                Currency = x.Currency,
                TxId = x.MerchantPaymentId,
                TxStatus = x.Status,
                //switch
                //{
                //    "Done" => Status.Successful,
                //    "Failed" => Status.Failed,
                //    "New" => Status.Pending,
                //    "Processing" => Status.Pending
                //}
            })?.ToArray() ?? Array.Empty<TargetData>();
        }

        private static string GetSuccessfullTransactionsUrl(DateTime? from, DateTime to)
        {
            return string.Format(ReportsUrl, from?.MongoDateTime() ?? to.MongoDateTime(), to.MongoDateTime(), nameof(NodaStatus.Done));
        }

        private static string GetFailedTransactionsUrl(DateTime? from, DateTime to)
        {
            return string.Format(ReportsUrl, from?.MongoDateTime() ?? to.MongoDateTime(), to.MongoDateTime(), nameof(NodaStatus.Failed));
        }
    }
}
