namespace Application.Features.Configuration.Currencies.Caching
{
    public static class CurrencyCacheKey
    {
        public const string GetAllCacheKey = "all-currencies";

        public static string GetByCodeCacheKey(string code)
        {
            return $"currency-{code}";
        }
    }
}
