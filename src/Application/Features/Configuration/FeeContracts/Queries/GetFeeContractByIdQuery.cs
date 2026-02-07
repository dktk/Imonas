using Application.Features.Configuration.FeeContracts.DTOs;

namespace Application.Features.Configuration.FeeContracts.Queries
{
    public class GetFeeContractByIdQuery : IRequest<FeeContractDto?>
    {
        public int Id { get; set; }
    }

    public class GetFeeContractByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetFeeContractByIdQuery, FeeContractDto?>
    {
        public async Task<FeeContractDto?> Handle(GetFeeContractByIdQuery request, CancellationToken cancellationToken)
        {
            var contract = await context.FeeContracts
                .Include(f => f.Psp)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (contract == null) return null;

            var dto = mapper.Map<FeeContractDto>(contract);
            dto.PspName = contract.Psp?.Name ?? string.Empty;
            return dto;
        }
    }
}
