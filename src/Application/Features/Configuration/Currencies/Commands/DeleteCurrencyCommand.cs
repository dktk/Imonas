using Application.Features.Configuration.Currencies.Caching;

using SG.Common;

namespace Application.Features.Configuration.Currencies.Commands
{
    public class DeleteCurrencyCommand : IRequest<Result<bool>>, ICacheInvalidator
    {
        public int Id { get; set; }

        public string CacheKey => CurrencyCacheKey.GetAllCacheKey;
        public CancellationTokenSource? SharedExpiryTokenSource => CurrencyCacheTokenSource.ResetCacheToken;
    }

    public class DeleteCurrencyCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<DeleteCurrencyCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
        {
            var currency = await context.Currencies
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (currency == null)
                return Result<bool>.CreateFailure("Currency not found.");

            context.Currencies.Remove(currency);
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, $"Currency '{currency.Name}' ({currency.Code}) deleted successfully.");
        }
    }
}
