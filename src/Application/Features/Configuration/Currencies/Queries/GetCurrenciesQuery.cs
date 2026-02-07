using Application.Features.Configuration.Currencies.Caching;
using Application.Features.Configuration.Currencies.DTOs;

namespace Application.Features.Configuration.Currencies.Queries
{
    public class GetCurrenciesQuery : IRequest<IEnumerable<CurrencyDto>>, ICacheable
    {
        public string CacheKey => CurrencyCacheKey.GetAllCacheKey;

        public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions()
            .AddExpirationToken(new CancellationChangeToken(CurrencyCacheTokenSource.ResetCacheToken.Token))
            .SetSlidingExpiration(TimeSpan.FromHours(1));
    }

    public class GetCurrenciesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetCurrenciesQuery, IEnumerable<CurrencyDto>>
    {
        public async Task<IEnumerable<CurrencyDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
        {
            var currencies = await context.Currencies
                .OrderBy(c => c.Code)
                .ToListAsync(cancellationToken);

            return mapper.Map<IEnumerable<CurrencyDto>>(currencies);
        }
    }
}
