using Domain.Enums;

namespace Application.Features.ReconciliationRuns.Queries
{
    public class RunStatsDto
    {
        public int TotalRuns { get; set; }
        public int CompletedRuns { get; set; }
        public int RunningRuns { get; set; }
        public int FailedRuns { get; set; }
        public decimal AverageMatchRate { get; set; }
        public int TotalRecordsProcessed { get; set; }
        public int TotalMatched { get; set; }
        public int TotalUnmatched { get; set; }
    }

    public class GetRunStatsQuery : IRequest<RunStatsDto>
    {
    }

    public class GetRunStatsQueryHandler(IApplicationDbContext context) :
        IRequestHandler<GetRunStatsQuery, RunStatsDto>
    {
        public async Task<RunStatsDto> Handle(GetRunStatsQuery request, CancellationToken cancellationToken)
        {
            var runs = await context.ReconciliationRuns.ToListAsync(cancellationToken);

            var completedRuns = runs.Where(r => r.Status == RunStatus.Completed).ToList();
            var avgMatchRate = completedRuns.Any() ? completedRuns.Average(r => r.MatchPercentage) : 0;

            return new RunStatsDto
            {
                TotalRuns = runs.Count,
                CompletedRuns = runs.Count(r => r.Status == RunStatus.Completed),
                RunningRuns = runs.Count(r => r.Status == RunStatus.Running),
                FailedRuns = runs.Count(r => r.Status == RunStatus.Failed),
                AverageMatchRate = avgMatchRate,
                TotalRecordsProcessed = runs.Sum(r => r.TotalRecords),
                TotalMatched = runs.Sum(r => r.MatchedRecords),
                TotalUnmatched = runs.Sum(r => r.UnmatchedRecords)
            };
        }
    }
}
