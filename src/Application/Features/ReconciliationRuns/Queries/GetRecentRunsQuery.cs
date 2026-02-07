using Application.Features.ReconciliationRuns.DTOs;

namespace Application.Features.ReconciliationRuns.Queries
{
    public class GetRecentRunsQuery : IRequest<IEnumerable<ReconciliationRunDto>>
    {
        public int Count { get; set; } = 50;
    }

    public class GetRecentRunsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetRecentRunsQuery, IEnumerable<ReconciliationRunDto>>
    {
        public async Task<IEnumerable<ReconciliationRunDto>> Handle(GetRecentRunsQuery request, CancellationToken cancellationToken)
        {
            return await context.ReconciliationRuns
                .OrderByDescending(r => r.StartedAt)
                .Take(request.Count)
                .ProjectTo<ReconciliationRunDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
