using System.Data;

using Domain;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PspConnectors.Sources.Omega
{
    public class OmegaSourceService : ISourceService
    {
        private readonly string _connectionString;
        private readonly ILogger<OmegaSourceService> _logger;


        public const string Name = "Omega";
        public const string System = "Omega";

        public string ServiceName { get; } = Name;


        private const int DataChunkSize = 250;

        public OmegaSourceService(ILogger<OmegaSourceService> logger, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SourceConnection");
            _logger = logger;
        }

        public async Task<InternalTransactionsResult> GetDataByTransactions(string[] transactionIds)
        {
            // todo:
            //var result = new List<SourceData>();

            //var i = 1;
            //foreach (var chunk in transactionIds.Chunk(DataChunkSize))
            //{
            //    _logger.LogInformation($"Reading Source Data using {nameof(OmegaSourceService)}: {i * DataChunkSize}/{transactionIds.Length}");

            //    result.AddRange(await InnerGetDataByTransactions(chunk));

            //    i++;
            //}

            //return result;

            return new InternalTransactionsResult();
        }

        private async Task<List<InternalTransaction>> InnerGetDataByTransactions(string[] transactionIds)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = $""" 
                                select 
                                    Id, 
                                    REF_NUMBER as RefNumber, 
                                    REQUEST_DATE as RequestDate, 
                                    Status, 
                                    Amount, 
                                    Account_Id as AccountId, 
                                    Misc as Description, 
                                    PROVIDER_TRAN_ID as ProviderTxId 
                                FROM [OmegaReplica].[admin_all].[PAYMENT] 
                                where
                                    REF_Number in ('{string.Join("','", transactionIds)}');
                            """;

            var command = connection.CreateCommand();
            command.CommandText = query;

            return await ExecuteCommand(command);
        }

        public async Task<List<InternalTransaction>> GetData(string[] pspName, DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = $"""
                                select Id, REF_NUMBER as RefNumber, REQUEST_DATE as RequestDate, Status, Amount, Account_Id as AccountId, Misc as Description, PROVIDER_TRAN_ID as ProviderTxId FROM [admin_all].[PAYMENT] 
                                    where 
                                        Status in ('COMPLETED', 'FAILED', 'CANCELLED') and
                                        -- Status = 'COMPLETED' and
                                        REQUEST_DATE >= @startDate and 
                                        REQUEST_DATE <= @endDate and 
                                        SUBMETHOD in ({string.Join(",", pspName.Select(x => "'" + x + "'"))})
                            """;

                _logger.LogInformation("Omega Transactions: " + query);

                var command = connection.CreateCommand();
                command.CommandText = query;

                command.Parameters.Add("@startDate", SqlDbType.DateTime);
                command.Parameters["@startDate"].Value = startDate.PrettyDateTime();

                command.Parameters.Add("@endDate", SqlDbType.DateTime);
                command.Parameters["@endDate"].Value = endDate.PrettyDateTime();

                return await ExecuteCommand(command);
            }
        }

        private static async Task<List<InternalTransaction>> ExecuteCommand(SqlCommand command)
        {
            var reader = await command.ExecuteReaderAsync();

            var result = new List<InternalTransaction>();

            while (reader.Read())
            {
                var description = reader.IsDBNull(6) ? null : reader.GetString(6);
                var refNumber = reader.IsDBNull(1) ? null : reader.GetString(1);

                result.Add(new InternalTransaction
                {
                    Id = reader.GetInt32(0).ToString(),
                    RefNumber = refNumber,
                    RequestDate = reader.GetDateTime(2),
                    Status = reader.GetString(3),
                    //switch
                    //{
                    //    // NOTE: let this fail if there are other Statuses

                    //    "COMPLETED" => Status.Successful,
                    //    "PENDING" => Status.Pending,
                    //    "CANCELLED" => Status.Cancelled,
                    //    "FAILED" => Status.Failed
                    //},
                    Amount = reader.GetDecimal(4),
                    AccountId = reader.GetInt32(5),
                    Description = description,
                    ProviderTxId = reader.GetString(7),
                    System = System,
                    CurrencyCode = reader.GetString(8),
                    Email = reader.GetString(9),
                });
            }

            return result;
        }
    }
}
