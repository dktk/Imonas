using System.Net.Http.Headers;
using System.Net.Http.Json;

using Domain;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PspConnectors.Methods.Nummuspay
{
    // https://api.nummuspay.com/docs/index#!/Companies/Companies_GetProducts
    public class NummusPayService : BaseMethodService
    {
        public const string PspName = "NummusPay";

        private const string BaseUrl = "https://api.nummuspay.com/v2/";
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public NummusPayService(ILogger<NummusPayService> logger, IOptions<NummuspayConfig> config)
            : base(logger, PspName)
        {
            _apiKey = config.Value.ApiKey;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async override Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            var endpoint = "transactions";
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadFromJsonAsync<Transaction[]>();

                return transactions
                            .Select(x => new TargetData
                            {
                                Id = x.ID.ToString(),
                                Amount = x.AmountReceived,
                                Date = x.CreatedDatetime,
                                Email = x.Customer.Email,
                                Psp = PspName,
                                Currency = GetCurrency(x.CurrencyOriginal, x.CurrencyPaid, x.CurrencyCustomer),
                                TxId = x.ID.ToString(),
                                TxStatus = x.TransactionStatus,
                                //switch
                                //{
                                //    "Successful" => Status.Successful
                                //}
                            }).ToArray();
            }
            else
            {
                // Handle error response accordingly
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error retrieving transactions: {response.StatusCode}, Details: {errorContent}");
            }
        }

        private string GetCurrency(string currencyOriginal, string currencyPaid, string currencyCustomer)
        {
            // todo:
            return "EUR";
        }
    }
}
