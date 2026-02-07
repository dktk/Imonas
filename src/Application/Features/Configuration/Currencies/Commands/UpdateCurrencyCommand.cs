using Application.Features.Configuration.Currencies.Caching;
using Application.Features.Configuration.Currencies.DTOs;

using SG.Common;

namespace Application.Features.Configuration.Currencies.Commands
{
    public class UpdateCurrencyCommand : IRequest<Result<CurrencyDto>>, ICacheInvalidator
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string HtmlCode { get; set; } = string.Empty;

        public string CacheKey => CurrencyCacheKey.GetAllCacheKey;
        public CancellationTokenSource? SharedExpiryTokenSource => CurrencyCacheTokenSource.ResetCacheToken;
    }

    public class UpdateCurrencyCommandHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<UpdateCurrencyCommand, Result<CurrencyDto>>
    {
        public async Task<Result<CurrencyDto>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
        {
            var currency = await context.Currencies
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (currency == null)
                return Result<CurrencyDto>.CreateFailure("Currency not found.");

            // Check for duplicate code (excluding current currency)
            var existingCurrency = await context.Currencies
                .FirstOrDefaultAsync(c => c.Code == request.Code && c.Id != request.Id, cancellationToken);

            if (existingCurrency != null)
                return Result<CurrencyDto>.CreateFailure($"A currency with code '{request.Code}' already exists.");

            currency.Name = request.Name;
            currency.Code = request.Code.ToUpperInvariant();
            currency.HtmlCode = request.HtmlCode;

            await context.SaveChangesAsync(cancellationToken);

            var dto = mapper.Map<CurrencyDto>(currency);
            return Result<CurrencyDto>.CreateSuccess(dto, $"Currency '{currency.Name}' ({currency.Code}) updated successfully.");
        }
    }
}
