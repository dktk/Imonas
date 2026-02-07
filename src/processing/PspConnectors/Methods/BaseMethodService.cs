using Domain;

using Microsoft.Extensions.Logging;

namespace PspConnectors.Methods
{
    public abstract class BaseMethodService : IMethodService
    {
        protected readonly string _pspName;
        protected readonly ILogger<BaseMethodService> _logger;

        protected BaseMethodService(ILogger<BaseMethodService> logger, string pspName)
        {
            _pspName = pspName;
            _logger = logger;
        }

        public string GetTargetDataFileName(DateTime from, DateTime to) => $"{_pspName}_{from.CompressedPrettyDateTime()}_{to.CompressedPrettyDateTime()}.json";
        public abstract Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to);

        public virtual Task<TargetData[]> GetTransactionsAsync(byte[] content)
        {
            return Task.FromResult<TargetData[]>([]);
        }
    }
}
