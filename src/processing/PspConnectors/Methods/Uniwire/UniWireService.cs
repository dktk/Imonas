using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Domain;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SG.Common;

namespace PspConnectors.Methods.Uniwire
{
    public class UniWireService : BaseMethodService
    {
        public const string PspName = "UniWire";

        private readonly UniWireConfig _config;
        private readonly HttpClient _httpClient;

        public UniWireService(ILogger<UniWireService> logger, IOptions<UniWireConfig> config)
            : base(logger, PspName)
        {
            _config = config.Value;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.BaseUrl.TrimEnd('/') + "/")
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public override async Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            var allTransactions = new List<UniWireTransaction>();
            var page = 1;

            while (true)
            {
                var transactions = await FetchPageAsync(page);
                if (transactions.Length == 0)
                    break;

                // Filter to the requested date range
                var inRange = transactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
                    .ToArray();
                allTransactions.AddRange(inRange);

                // Stop if we've gone past the date range (results are sorted desc by created_at)
                if (transactions.Any(t => t.CreatedAt < from))
                    break;

                page++;
            }

            _logger.LogInformation("Fetched {Count} UniWire transactions between {From} and {To}",
                allTransactions.Count, from, to);

            return allTransactions
                .Select(t => new TargetData
                {
                    Id = t.Id,
                    TxId = t.TxId,
                    Date = t.CreatedAt,
                    Amount = t.Amount?.Paid ?? 0,
                    Currency = t.Amount?.Currency ?? string.Empty,
                    TxStatus = t.Status,
                    Psp = PspName,
                    Email = t.Invoice?.CustomerEmail ?? Constants.NotAvailable,
                    ClientId = 0,
                    Description = t.Invoice?.Description ?? string.Empty,
                    ReferenceCode = t.Invoice?.Id ?? string.Empty,
                })
                .ToArray();
        }

        private async Task<UniWireTransaction[]> FetchPageAsync(int page)
        {
            var endpoint = $"v1/transactions/?p={page}";

            var payload = new
            {
                request = "/v1/transactions/",
                nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                p = page
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));
            var signature = ComputeHmacSha256(payloadBase64, _config.ApiSecret);

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("X-CC-KEY", _config.ApiKey);
            request.Headers.Add("X-CC-PAYLOAD", payloadBase64);
            request.Headers.Add("X-CC-SIGNATURE", signature);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"UniWire API error on page {page}: {response.StatusCode}, Details: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<UniWireTransactionListResponse>();
            return result?.Data ?? [];
        }

        private static string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            return Convert.ToHexStringLower(hash);
        }
    }
}
