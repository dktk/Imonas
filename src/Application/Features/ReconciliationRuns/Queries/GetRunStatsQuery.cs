namespace Application.Features.ReconciliationRuns.Queries
{
    public class RunStatsDto
    {
        public int TotalRuns { get; set; }
        public int CompletedRuns { get; set; }
        public int RunningRuns { get; set; }
        public int FailedRuns { get; set; }
        public decimal AverageMatchRate { get; set; }
        public int InternalRecordsMatched { get; set; }
        public int TotalMatchedCount => InternalRecordsMatched + ExternalRecordsMatched;
        public int InternalRecordsUnmatched { get; set; }
        public int ExternalRecordsMatched { get; set; }
        public int TotalUnmatchedCount => InternalRecordsUnmatched + ExternalRecordsUnmatched;
        public int ExternalRecordsUnmatched { get; set; }
        public int TotalRecordsCount => TotalMatchedCount + TotalUnmatchedCount;
    }

    public class GetRunStatsQuery : IRequest<RunStatsDto>
    {
    }

    public class GetRunStatsQueryHandler(IApplicationDbContext context) :
        IRequestHandler<GetRunStatsQuery, RunStatsDto>
    {
        public async Task<RunStatsDto> Handle(GetRunStatsQuery request, CancellationToken cancellationToken)
        {
            var runs = await context.ReconciliationRuns

                .Where(r => !r.IsArchived)
                .ToListAsync(cancellationToken);

            var completedRuns = runs.Where(r => r.Status == RunStatus.Completed).ToList();
            var avgMatchRate = completedRuns.Any() ? completedRuns.Average(r => r.MatchPercentage) : 0;

            return new RunStatsDto
            {
                TotalRuns = runs.Count,
                CompletedRuns = runs.Count(r => r.Status == RunStatus.Completed),
                RunningRuns = runs.Count(r => r.Status == RunStatus.Running),
                FailedRuns = runs.Count(r => r.Status == RunStatus.Failed),
                AverageMatchRate = avgMatchRate,

                InternalRecordsMatched = runs.Sum(r => r.InternalMatchedRecordsCount),
                InternalRecordsUnmatched = runs.Sum(r => r.InternalUnmatchedRecordsCount),

                ExternalRecordsMatched = runs.Sum(r => r.ExternalMatchedRecordsCount),
                ExternalRecordsUnmatched = runs.Sum(r => r.ExternalUnmatchedRecordsCount)
            };
        }
    }
}
