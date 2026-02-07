using Domain.Enums;

namespace Application.Features.Cases.Queries
{
    public class CaseStatsDto
    {
        public int TotalCases { get; set; }
        public int OpenCases { get; set; }
        public int InProgressCases { get; set; }
        public int PendingReviewCases { get; set; }
        public int ClosedCases { get; set; }
        public int CriticalCases { get; set; }
        public int HighCases { get; set; }
        public int MediumCases { get; set; }
        public int LowCases { get; set; }
        public int OverdueCases { get; set; }
        public decimal TotalVarianceAmount { get; set; }
    }

    public class GetCaseStatsQuery : IRequest<CaseStatsDto>
    {
        public int? RunId { get; set; }
    }

    public class GetCaseStatsQueryHandler(IApplicationDbContext context) :
        IRequestHandler<GetCaseStatsQuery, CaseStatsDto>
    {
        public async Task<CaseStatsDto> Handle(GetCaseStatsQuery request, CancellationToken cancellationToken)
        {
            var query = context.ExceptionCases.AsQueryable();

            if (request.RunId.HasValue)
            {
                query = query.Where(c => c.ReconciliationRunId == request.RunId.Value);
            }

            var cases = await query.ToListAsync(cancellationToken);

            return new CaseStatsDto
            {
                TotalCases = cases.Count,
                OpenCases = cases.Count(c => c.Status == CaseStatus.Open),
                InProgressCases = cases.Count(c => c.Status == CaseStatus.InProgress),
                PendingReviewCases = cases.Count(c => c.Status == CaseStatus.PendingReview),
                ClosedCases = cases.Count(c => c.Status == CaseStatus.Closed),
                CriticalCases = cases.Count(c => c.Severity == CaseSeverity.Critical),
                HighCases = cases.Count(c => c.Severity == CaseSeverity.High),
                MediumCases = cases.Count(c => c.Severity == CaseSeverity.Medium),
                LowCases = cases.Count(c => c.Severity == CaseSeverity.Low),
                OverdueCases = cases.Count(c => c.DueDate.HasValue && c.DueDate.Value < DateTime.UtcNow && c.Status != CaseStatus.Closed),
                TotalVarianceAmount = cases.Where(c => c.Status != CaseStatus.Closed && c.VarianceAmount.HasValue).Sum(c => c.VarianceAmount!.Value)
            };
        }
    }
}
