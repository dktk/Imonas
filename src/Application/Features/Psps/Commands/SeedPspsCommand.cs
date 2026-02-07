using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class SeedPspsCommand : IRequest<Result<int>>
    {

    }

    public class SeedPspsCommandHandler(IApplicationDbContext context) : IRequestHandler<SeedPspsCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(SeedPspsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var internalSystemNames = new[] { "Omega", "SquarePay" };

                var internalSystems = await context.InternalSystems.Where(x => internalSystemNames.Contains(x.Name)).ToArrayAsync(cancellationToken);

                if (internalSystems.Length == internalSystemNames.Length)
                {
                    return 0.CreateSuccess();
                }

                var seededCurrencies = new [] {
                    new Currency("US Dollar", "USD", "&#36;"),
                    new Currency("Euro", "EUR", "&euro;"),
                    new Currency("Japanese Yen", " JPY", "&yen;"),
                    new Currency("Pound Sterling", " GBP", "&pound;"),
                    new Currency("Chinese Yuan(Renminbi)", "CNY", "&yen;"),
                    new Currency("Swiss Franc", "CHF", "CHF;"),
                    new Currency("Australian Dollar", " AUD", "&#36;"),
                    new Currency("Canadian Dollar", "CAD", "&#36;"),
                    new Currency("Hong Kong Dollar", " HKD", "&#36;"),
                    new Currency("Singapore Dollar", "SGD", "&#36;"),
                    new Currency("Indian Rupee", "INR", "&#8377;"),
                    new Currency("South Korean Won", " KRW", "&#8361;"),
                    new Currency("Swedish Krona", "SEK", "kr"),
                    new Currency("Mexican Peso", "MXN", "&#36;"),
                    new Currency("New Zealand Dollar", " NZD", "&#36;"),
                    new Currency("Norwegian Krone", "NOK", "kr"),
                    new Currency("New Taiwan Dollar", " TWD", " NT&#36;"),
                    new Currency("Brazilian Real", "BRL", "R&#36;"),
                    new Currency("South African Rand", "ZAR", "R"),
                    new Currency("Polish Zloty", " PLN", " z&#322;"),
                    new Currency("Danish Krone", "DKK", "kr"),
                    new Currency("Indonesian Rupiah", "IDR", "Rp"),
                    new Currency("Turkish Lira", "TRY", "&#8378;"),
                    new Currency("Thai Baht", "THB", "&#3647;"),
                    new Currency("Israeli New Shekel", " ILS", "&#8362;")
                };

                context.Currencies.AddRange(seededCurrencies.OrderBy(x => x.Code));

                // todo: replace the hardcoded names here with the *Service.Name. Eg. OmegaSourceService.Name
                //
                var omegaInternalSystem = new InternalSystem
                {
                    Name = "Omega",
                    IsActive = true
                };
                var squarepayInternalSystem = new InternalSystem
                {
                    Name = "SquarePay",
                    IsActive = true
                };

                context.InternalSystems.Add(omegaInternalSystem);
                context.InternalSystems.Add(squarepayInternalSystem);

                context.Psps.AddRange([
                new Psp
                {
                    Code = "BtcBit".ToUpper(),
                    Name = "BtcBit",
                    IsActive = true,
                    IsCsvBased = true,
                    InternalSystem = squarepayInternalSystem
                },

                new Psp
                {
                    Code = "CubixPay".ToUpper(),
                    Name = "CubixPay",
                    IsActive = true,
                    IsCsvBased = true,
                    InternalSystem = squarepayInternalSystem
                },

                new Psp
                {
                    Code = "NummusPay".ToUpper(),
                    Name = "NummusPay",
                    IsActive = true,
                    IsCsvBased = false,
                    InternalSystem = squarepayInternalSystem
                },

                new Psp
                {
                    Code = "Paysage".ToUpper(),
                    Name = "Paysage",
                    IsActive = true,
                    IsCsvBased = true,
                    InternalSystem = squarepayInternalSystem
                },

                new Psp
                {
                    Code = "NodaPay".ToUpper(),
                    Name = "NodaPay",
                    IsActive = true,
                    IsCsvBased = true,
                    InternalSystem = squarepayInternalSystem
                },
            ]);

                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<int>.CreateFailure([ex.Message]);
            }

            return Result<int>.CreateSuccess(0);
        }
    }
}
