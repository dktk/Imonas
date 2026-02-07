using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PspConnectors.Methods.Rastpay
{
    public class RastPayClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly RastPayConfig _options;

        public RastPayClient(IOptions<RastPayConfig> options, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = options.Value.BaseUrl;
            _options = options.Value;
        }

        public async Task<List<Transaction>> GetPayments(DateTime from, DateTime to)
        {
            var result = new List<Transaction>();

            foreach (var secretKey in _options.SecretKeys)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AuthToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/payments?created.gte={from.ToString("yyyy-MM-ddTHH:mm:ss")}&created.lt={to.ToString("yyyy-MM-ddTHH:mm:ss")}");

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadFromJsonAsync<TransactionWrapper>();
                result.AddRange(responseContent.Result);
            }

            return result;
        }
    }
}