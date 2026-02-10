using Application.Features.ReconciliationRuns.DTOs;

namespace Application.Features.ReconciliationRuns.Queries
{
    public class GetRunByIdQuery : IRequest<ReconciliationRunDetailsDto?>
    {
        public int Id { get; set; }
    }

    public class GetRunByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetRunByIdQuery, ReconciliationRunDetailsDto?>
    {
        public async Task<ReconciliationRunDetailsDto?> Handle(GetRunByIdQuery request, CancellationToken cancellationToken)
        {
            var run = await context.ReconciliationRuns
                .Include(r => r.Metrics)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (run == null)
                return null;

            var dto = mapper.Map<ReconciliationRunDetailsDto>(run);

            return dto;
        }
    }
}
