using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using PspConnectors.Domain;



using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Domain;

//https://docs.paysage.io/en/payment_management/reports/reports_shop/#request

namespace PspConnectors.Methods.Paysage
{
    public class PaysageService : BaseMethodService
    {
        public const string PspName = "Paysage";

        private readonly HttpClient _httpClient;
        private readonly PaysageConfig _pspConfig;
        private readonly TargetService _targetService;

        public PaysageService(IConfiguration configuration,
            ILogger<PaysageService> logger,
            TargetService targetService,
            IOptions<PaysageConfig> pspConfig,
            HttpClient httpClient) : base(logger, PspName)
        {
            _httpClient = httpClient;
            _pspConfig = pspConfig.Value;
            _targetService = targetService;
        }

        public override async Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            var targetData = new List<TargetData>();

            foreach (var config in _pspConfig.Secrets)
            {
                targetData.AddRange(await GetTransactionsByShopAsync(from, to, config.ShopId, config.Secret));
            }

            var result = targetData.ToArray();

            var filePath = await _targetService.Save(GetTargetDataFileName(from, to), result);
            _logger.LogDebug(filePath);

            return result;
        }

        // todo: take into account PaysageReponse.HasMore
        private async Task<TargetData[]> GetTransactionsByShopAsync(DateTime from, DateTime to, string shopId, string secretKey)
        {
            try
            {
                var authToken = Encoding.ASCII.GetBytes($"{shopId}:{secretKey}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("X-Api-Version", _pspConfig.ApiVersion);

                var payload = PaysagePayload.Create(from, to).ToJson();
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var paysageResponse = await _httpClient.PostAsync(_pspConfig.ReportUrl, content);
                paysageResponse.EnsureSuccessStatusCode();

                var response = await paysageResponse.Content.ReadFromJsonAsync<PaysageReponse>();
                var transactions = response?.Transactions;

                return await ConvertTransaction(transactions, from, to);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while processing info for shopId: {shopId}", ex);
            }

            return null;
        }

        private async Task<TargetData[]> ConvertTransaction(Transaction[] transactions, DateTime from, DateTime to)
        {
            var data = transactions
                        ?.Select(x => new TargetData
                        {
                            Id = x.Uid,
                            Amount = x.Amount,
                            Date = x.CreatedAt,
                            Psp = PspName,
                            Email = x.Customer.Email,
                            TxId = x.Tracking_Id,
                            Currency = x.Currency,
                            ClientId = GetClientId(),
                            Description = x.Description,
                            ReferenceCode = GetRefernceCode(),

                            TxStatus = x.Status,
                            //switch
                            //{
                            //    "successful" => Status.Successful,
                            //    "pending" => Status.Pending,
                            //    "expired" => Status.Expired,
                            //    "deleted" => Status.Deleted,
                            //    "error" => Status.Error,

                            //    _ => Status.Failed
                            //}
                        })
                        ?.ToArray();

            return data;
        }

        private string GetRefernceCode()
        {
            throw new NotImplementedException();
        }

        private int GetClientId()
        {
            throw new NotImplementedException();
        }
    }
}
