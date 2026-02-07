using System.Data;

using Domain;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SG.Common;

namespace PspConnectors.Sources.SquarePay
{
    public class SquarePaySourceService : ISourceService
    {
        private readonly ILogger<SquarePaySourceService> _logger;

        public const string Name = "SquarePay";
        public const string System = "SquarePay";

        private readonly IConfiguration _configuration;
        private const string SqlTransactionsByDateCommandText = """
            SELECT
              CAST([Id] AS VARCHAR(36)) AS [Id]
              ,[Status]
              ,[PaymentMethod]
              ,[UserId]
              ,[ProviderTransactionId]
              ,[UserEmail]
              ,[InitialAmount]
              ,[InitialCurrency]
              ,Status
              ,DeclineMessage
              ,LastModified
          FROM [dbo].[Transactions]
          where 
            ProviderTransactionId in ({0})
          """;

        private const string SqlTransactionsCommandText = """
            SELECT [Id]
                ,[Status]
                ,[PaymentMethod]
                ,[UserId]
                ,[ProviderTransactionId]
                ,[UserEmail]
                ,[InitialAmount]
                ,[InitialCurrency]
                ,Status
                ,DeclineMessage
                ,LastModified
            FROM [dbo].[Transactions]
            where
                Id in ({0})
            """;

        public string ServiceName { get; } = Name;

        public SquarePaySourceService(IConfiguration configuration, ILogger<SquarePaySourceService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<InternalTransactionsResult> GetDataByTransactions(string[] transactionIds)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("SquarePayConnection")))
            {
                try
                {
                    var paramNames = transactionIds.Select((_, i) => $"@id{i}").ToArray();

                    var command = connection.CreateCommand();
                    command.CommandText = string.Format(SqlTransactionsByDateCommandText, string.Join(", ", paramNames));

                    for (int i = 0; i < transactionIds.Length; i++)
                    {
                        // Explicit type helps avoid implicit conversions and plan issues.
                        command.Parameters.Add(paramNames[i], SqlDbType.VarChar).Value = transactionIds[i];
                    }

                    await connection.OpenAsync();

                    var reader = await command.ExecuteReaderAsync();

                    var result = new List<InternalTransaction>();

                    while (reader.Read())
                    {
                        result.Add(Adapt(reader));
                    }

                    var (found, _) = result.Split(x => transactionIds.Contains(x.ProviderTxId));

                    var lst = transactionIds.ToList();
                    foreach (var item in found)
                    {
                        if (lst.Contains(item.ProviderTxId))
                        {
                            lst.Remove(item.ProviderTxId);
                        }
                    }

                    return new InternalTransactionsResult
                    {
                        Found = found.ToArray(),
                        NotFoundTransactionIds = lst
                    };
                }
                catch (Exception ex)
                {
                    return new InternalTransactionsResult();
                }
            }
        }

        private static InternalTransaction Adapt(SqlDataReader reader)
        {
            var messageIdx = reader.GetOrdinal("DeclineMessage");

            return new InternalTransaction
            {
                AccountId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("InitialAmount")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("InitialCurrency")),

                Description = reader.IsDBNull(messageIdx) ? string.Empty : reader.GetString(messageIdx),
                RequestDate = reader.GetDateTime(reader.GetOrdinal("LastModified")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                //switch
                //{
                //    "Completed" => Status.Successful,
                //    "Declined" => Status.Failed
                //},

                // the external ID (Noda, Skrill, etc, etc)
                ProviderTxId = reader.GetString(reader.GetOrdinal("ProviderTransactionId")),

                // the RefNumber on the Omega Transaction
                RefNumber = reader.GetString(reader.GetOrdinal("Id")),

                Email = reader.GetString(reader.GetOrdinal("UserEmail")),

                System = System,

                Id = reader.GetString(reader.GetOrdinal("Id")),
            };
        }
    }
}
