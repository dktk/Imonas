using Application.Features.Configuration.Currencies.Caching;
using Application.Features.Configuration.Currencies.DTOs;

using SG.Common;

namespace Application.Features.Configuration.Currencies.Commands
{
    public class CreateCurrencyCommand : IRequest<Result<CurrencyDto>>, ICacheInvalidator
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string HtmlCode { get; set; } = string.Empty;

        public string CacheKey => CurrencyCacheKey.GetAllCacheKey;
        public CancellationTokenSource? SharedExpiryTokenSource => CurrencyCacheTokenSource.ResetCacheToken;
    }

    public class CreateCurrencyCommandHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<CreateCurrencyCommand, Result<CurrencyDto>>
    {
        public async Task<Result<CurrencyDto>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate code
            var existingCurrency = await context.Currencies
                .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken);

            if (existingCurrency != null)
                return Result<CurrencyDto>.CreateFailure($"A currency with code '{request.Code}' already exists.");

            var currency = new Currency
            {
                Name = request.Name,
                Code = request.Code.ToUpperInvariant(),
                HtmlCode = request.HtmlCode
            };

            context.Currencies.Add(currency);
            await context.SaveChangesAsync(cancellationToken);

            var dto = mapper.Map<CurrencyDto>(currency);
            return Result<CurrencyDto>.CreateSuccess(dto, $"Currency '{currency.Name}' ({currency.Code}) created successfully.");
        }
    }
}
