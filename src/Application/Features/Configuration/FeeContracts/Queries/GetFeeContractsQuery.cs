using Application.Features.Configuration.FeeContracts.DTOs;

namespace Application.Features.Configuration.FeeContracts.Queries
{
    public class GetFeeContractsQuery : IRequest<IEnumerable<FeeContractDto>>
    {
    }

    public class GetFeeContractsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetFeeContractsQuery, IEnumerable<FeeContractDto>>
    {
        public async Task<IEnumerable<FeeContractDto>> Handle(GetFeeContractsQuery request, CancellationToken cancellationToken)
        {
            var contracts = await context.FeeContracts
                .Include(f => f.Psp)
                .OrderByDescending(f => f.Created)
                .ToListAsync(cancellationToken);

            return contracts.Select(c =>
            {
                var dto = mapper.Map<FeeContractDto>(c);
                dto.PspName = c.Psp?.Name ?? string.Empty;
                return dto;
            });
        }
    }
}
