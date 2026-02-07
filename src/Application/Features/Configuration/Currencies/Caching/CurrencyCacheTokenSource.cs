namespace Application.Features.Configuration.Currencies.Caching
{
    public sealed class CurrencyCacheTokenSource
    {
        static CurrencyCacheTokenSource()
        {
            ResetCacheToken = new CancellationTokenSource();
        }

        public static CancellationTokenSource ResetCacheToken { get; private set; }
    }
}
