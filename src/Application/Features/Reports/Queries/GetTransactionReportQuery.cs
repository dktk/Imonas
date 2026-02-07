using Domain.Entities.MedalionData.Gold;

namespace Application.Features.Reports.Queries
{
    public class TransactionReportDto
    {
        public int Id { get; set; }
        public string TxId { get; set; } = string.Empty;
        public string ExternalPaymentId { get; set; } = string.Empty;
        public string PspName { get; set; } = string.Empty;
        public int PspId { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string BrandId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime TxDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

        // Settlement info
        public bool IsSettled { get; set; }
        public ReconciliationStatus? SettlementStatus { get; set; }
        public string SettlementStatusDisplay => SettlementStatus?.ToString() ?? "Not Settled";
        public decimal? TotalFees { get; set; }
        public decimal? NetSettlement { get; set; }
        public DateTime? SettlementDate { get; set; }

        // Case info
        public int? CaseId { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseTitle { get; set; }
        public string? CaseStatus { get; set; }
    }

    public enum SettlementFilter
    {
        All = 0,
        Settled = 1,
        Unsettled = 2
    }

    public enum MatchStatusFilter
    {
        All = 0,
        Matched = 1,
        Unmatched = 2,
        PartialMatch = 3
    }

    public class GetTransactionReportQuery : PaginationRequest, IRequest<PaginatedData<TransactionReportDto>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? PspId { get; set; }
        public int? RunId { get; set; }
        public SettlementFilter SettlementFilter { get; set; } = SettlementFilter.All;
        public MatchStatusFilter MatchStatus { get; set; } = MatchStatusFilter.All;
    }

    public class GetTransactionReportQueryHandler(
        IApplicationDbContext context) :
        IRequestHandler<GetTransactionReportQuery, PaginatedData<TransactionReportDto>>
    {
        public async Task<PaginatedData<TransactionReportDto>> Handle(
            GetTransactionReportQuery request,
            CancellationToken cancellationToken)
        {
            // Base query for external payments
            var query = context.ExternalPayments
                .Include(ep => ep.Psp)
                .Where(ep => ep.TxDate >= request.StartDate && ep.TxDate <= request.EndDate.AddDays(1));

            // Filter by PSP if specified
            if (request.PspId.HasValue && request.PspId.Value > 0)
            {
                query = query.Where(ep => ep.PspId == request.PspId.Value);
            }

            // Filter by Run ID if specified - get external payment IDs from settlements for this run
            if (request.RunId.HasValue && request.RunId.Value > 0)
            {
                var runSettlementIds = context.PspSettlements
                    .Where(s => s.ReconciliationRunId == request.RunId.Value)
                    .Select(s => s.ExternalPaymentId);
                query = query.Where(ep => runSettlementIds.Contains(ep.Id));
            }

            // Get external payment IDs for settlement lookup
            var externalPaymentIds = await query.Select(ep => ep.Id).ToListAsync(cancellationToken);

            // Get settlements for these payments
            var settlementsQuery = context.PspSettlements
                .Where(s => externalPaymentIds.Contains(s.ExternalPaymentId));

            // If filtering by run, only get settlements from that run
            if (request.RunId.HasValue && request.RunId.Value > 0)
            {
                settlementsQuery = settlementsQuery.Where(s => s.ReconciliationRunId == request.RunId.Value);
            }

            var settlements = await settlementsQuery
                .Select(s => new
                {
                    s.ExternalPaymentId,
                    s.ReconciliationStatus,
                    s.TotalFees,
                    s.NetSettlement,
                    s.TxDate
                })
                .ToDictionaryAsync(s => s.ExternalPaymentId, cancellationToken);

            // Get cases linked to these payments
            var cases = await context.ExceptionCases
                .Where(c => c.LinkedTransactionId.HasValue && externalPaymentIds.Contains(c.LinkedTransactionId.Value))
                .Select(c => new
                {
                    TransactionId = c.LinkedTransactionId!.Value,
                    c.Id,
                    c.CaseNumber,
                    c.Title,
                    Status = c.Status.ToString()
                })
                .ToDictionaryAsync(c => c.TransactionId, cancellationToken);

            // Apply settlement filter
            IQueryable<int> filteredIds;
            if (request.SettlementFilter == SettlementFilter.Settled)
            {
                filteredIds = context.PspSettlements
                    .Where(s => externalPaymentIds.Contains(s.ExternalPaymentId))
                    .Select(s => s.ExternalPaymentId);
                query = query.Where(ep => filteredIds.Contains(ep.Id));
            }
            else if (request.SettlementFilter == SettlementFilter.Unsettled)
            {
                filteredIds = context.PspSettlements
                    .Where(s => externalPaymentIds.Contains(s.ExternalPaymentId))
                    .Select(s => s.ExternalPaymentId);
                query = query.Where(ep => !filteredIds.Contains(ep.Id));
            }

            // Apply match status filter
            if (request.MatchStatus != MatchStatusFilter.All && request.RunId.HasValue)
            {
                var matchedSettlements = context.PspSettlements
                    .Where(s => s.ReconciliationRunId == request.RunId.Value);

                switch (request.MatchStatus)
                {
                    case MatchStatusFilter.Matched:
                        // Fully matched (score >= 1.0, status = Successful)
                        matchedSettlements = matchedSettlements.Where(s => s.ReconciliationStatus == ReconciliationStatus.Successful);
                        query = query.Where(ep => matchedSettlements.Select(s => s.ExternalPaymentId).Contains(ep.Id));
                        break;
                    case MatchStatusFilter.PartialMatch:
                        // Partial match (status = Pending)
                        matchedSettlements = matchedSettlements.Where(s => s.ReconciliationStatus == ReconciliationStatus.Pending);
                        query = query.Where(ep => matchedSettlements.Select(s => s.ExternalPaymentId).Contains(ep.Id));
                        break;
                    case MatchStatusFilter.Unmatched:
                        // Unmatched - not in settlements for this run
                        var matchedIds = matchedSettlements.Select(s => s.ExternalPaymentId);
                        query = query.Where(ep => !matchedIds.Contains(ep.Id));
                        break;
                }
            }

            // Count total before pagination
            var total = await query.CountAsync(cancellationToken);

            // Apply sorting
            var sortField = string.IsNullOrEmpty(request.Sort) ? "TxDate" : request.Sort;
            var sortOrder = string.IsNullOrEmpty(request.Order) ? "desc" : request.Order;
            query = query.OrderBy($"{sortField} {sortOrder}");

            // Apply pagination
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.Rows <= 0 ? 20 : request.Rows;
            var skip = (page - 1) * pageSize;

            var payments = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(ep => new
                {
                    ep.Id,
                    ep.TxId,
                    ep.ExternalPaymentId,
                    PspName = ep.Psp.Name,
                    ep.PspId,
                    ep.PlayerId,
                    ep.BrandId,
                    ep.Amount,
                    ep.CurrencyCode,
                    ep.TxDate,
                    ep.Status,
                    Action = ep.Action.ToString()
                })
                .ToListAsync(cancellationToken);

            // Map to DTOs with settlement and case info
            var result = payments.Select(ep =>
            {
                var settlement = settlements.GetValueOrDefault(ep.Id);
                var caseInfo = cases.GetValueOrDefault(ep.Id);

                return new TransactionReportDto
                {
                    Id = ep.Id,
                    TxId = ep.TxId,
                    ExternalPaymentId = ep.ExternalPaymentId,
                    PspName = ep.PspName,
                    PspId = ep.PspId,
                    PlayerId = ep.PlayerId,
                    BrandId = ep.BrandId,
                    Amount = ep.Amount,
                    CurrencyCode = ep.CurrencyCode,
                    TxDate = ep.TxDate,
                    Status = ep.Status,
                    Action = ep.Action,
                    IsSettled = settlement != null,
                    SettlementStatus = settlement?.ReconciliationStatus,
                    TotalFees = settlement?.TotalFees,
                    NetSettlement = settlement?.NetSettlement,
                    SettlementDate = settlement?.TxDate,
                    CaseId = caseInfo?.Id,
                    CaseNumber = caseInfo?.CaseNumber,
                    CaseTitle = caseInfo?.Title,
                    CaseStatus = caseInfo?.Status
                };
            }).ToList();

            return new PaginatedData<TransactionReportDto>(result, total);
        }
    }
}
