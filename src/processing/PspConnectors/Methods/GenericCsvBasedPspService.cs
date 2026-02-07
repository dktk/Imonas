using System.Collections.Generic;

using CsvHelper;

using Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PspConnectors.Services;

namespace PspConnectors.Methods
{
    /// <summary>
    /// Use it when we get CSV data from a PSP.
    /// </summary>
    public abstract class GenericCsvBasedPspService<TTransaction> : BaseMethodService
    {
        private readonly IConfiguration _configuration;

        public GenericCsvBasedPspService(IConfiguration configuration, ILogger<GenericCsvBasedPspService<TTransaction>> logger, string pspName)
            : base(logger, pspName)
        {
            _configuration = configuration;
        }

        public abstract Action<CsvContext> CsvConfigurator { set; get; }
        public abstract Func<TTransaction, TargetData> TargetDataConverter { set; get; }

        public override async Task<TargetData[]> GetTransactionsAsync(byte[] content)
        {
            var result = CsvService.ReadCsv<TTransaction>(content, CsvConfigurator);

            return await Task.FromResult(result.Select(x => TargetDataConverter(x)).ToArray());
        }

        public override async Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            var results = new List<TargetData>();

            foreach (var file in _configuration.GetUploadedFiles(_pspName))
            {
                var records = CsvService.ReadCsv<TTransaction>(file, CsvConfigurator);

                for (int i = 0; i < records.Count; i++)
                {
                    results.Add(TargetDataConverter(records[i]));
                }
            }

            await Task.CompletedTask;

            return results.ToArray();
        }
    }
}
