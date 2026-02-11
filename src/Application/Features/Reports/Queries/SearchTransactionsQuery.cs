using Domain.Entities.MedalionData.Gold;

namespace Application.Features.Reports.Queries
{
    public class TransactionSearchResultDto
    {
        public int Id { get; set; }
        public string TxId { get; set; } = string.Empty;
        public string ExternalPaymentId { get; set; } = string.Empty;
        public string PspName { get; set; } = string.Empty;
        public int PspId { get; set; }
        public int ClientId { get; set; }
        public string BrandId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime TxDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ExternalSystem { get; set; } = string.Empty;

        // Settlement info
        public bool IsSettled { get; set; }
        public ReconciliationStatus? SettlementStatus { get; set; }
        public string SettlementStatusDisplay => SettlementStatus?.ToString() ?? "Not Settled";
        public decimal? TotalFees { get; set; }
        public decimal? NetSettlement { get; set; }
        public DateTime? SettlementDate { get; set; }
        public int? ReconciliationRunId { get; set; }

        // Case info
        public int? CaseId { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseTitle { get; set; }
        public string? CaseStatus { get; set; }
    }

    public class SearchTransactionsQuery : PaginationRequest, IRequest<PaginatedData<TransactionSearchResultDto>>
    {
        // Search criteria - any of these can be used
        public string? TransactionId { get; set; }
        public string? ExternalPaymentId { get; set; }
        public string? PlayerId { get; set; }
        public string? Reference { get; set; }
        public int? PspId { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Status { get; set; }
        public string? CurrencyCode { get; set; }
    }

    public class SearchTransactionsQueryHandler(
        IApplicationDbContext context) :
        IRequestHandler<SearchTransactionsQuery, PaginatedData<TransactionSearchResultDto>>
    {
        public async Task<PaginatedData<TransactionSearchResultDto>> Handle(
            SearchTransactionsQuery request,
            CancellationToken cancellationToken)
        {
            // Start with base query
            var query = context.ExternalPayments
                .Include(ep => ep.Psp)
                .AsQueryable();

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(request.TransactionId))
            {
                var txId = request.TransactionId.Trim();
                query = query.Where(ep => ep.TxId.Contains(txId));
            }

            if (!string.IsNullOrWhiteSpace(request.ExternalPaymentId))
            {
                var extId = request.ExternalPaymentId.Trim();
                query = query.Where(ep => ep.ExternalPaymentId.Contains(extId));
            }

            if (!string.IsNullOrWhiteSpace(request.PlayerId))
            {
                var playerId = int.Parse(request.PlayerId.Trim());
                query = query.Where(ep => ep.ClientId == playerId);
            }

            if (request.PspId.HasValue && request.PspId.Value > 0)
            {
                query = query.Where(ep => ep.PspId == request.PspId.Value);
            }

            if (request.AmountFrom.HasValue)
            {
                query = query.Where(ep => ep.Amount >= request.AmountFrom.Value);
            }

            if (request.AmountTo.HasValue)
            {
                query = query.Where(ep => ep.Amount <= request.AmountTo.Value);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(ep => ep.TxDate >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(ep => ep.TxDate <= request.DateTo.Value.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(ep => ep.Status == request.Status);
            }

            if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
            {
                query = query.Where(ep => ep.CurrencyCode == request.CurrencyCode);
            }

            // Get IDs for settlement/case lookup
            var matchingIds = await query.Select(ep => ep.Id).ToListAsync(cancellationToken);

            if (matchingIds.Count == 0)
            {
                return new PaginatedData<TransactionSearchResultDto>(new List<TransactionSearchResultDto>(), 0);
            }

            // Get settlements
            var settlements = await context.PspSettlements
                .Where(s => matchingIds.Contains(s.ExternalPaymentId))
                .Select(s => new
                {
                    s.ExternalPaymentId,
                    s.ReconciliationStatus,
                    s.TotalFees,
                    s.NetSettlement,
                    s.TxDate,
                    s.ReconciliationRunId
                })
                .ToDictionaryAsync(s => s.ExternalPaymentId, cancellationToken);

            // Get cases
            var cases = await context.ExceptionCases
                .Where(c => c.ExternalTransactionId.HasValue && matchingIds.Contains(c.ExternalTransactionId.Value))
                .Select(c => new
                {
                    TransactionId = c.ExternalTransactionId!.Value,
                    c.Id,
                    c.CaseNumber,
                    c.Title,
                    Status = c.Status.ToString()
                })
                .ToDictionaryAsync(c => c.TransactionId, cancellationToken);

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
                    ep.ClientId,
                    ep.BrandId,
                    ep.Amount,
                    ep.CurrencyCode,
                    ep.TxDate,
                    ep.Status,
                    Action = ep.Action.ToString(),
                    ep.ExternalSystem
                })
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var result = payments.Select(ep =>
            {
                var settlement = settlements.GetValueOrDefault(ep.Id);
                var caseInfo = cases.GetValueOrDefault(ep.Id);

                return new TransactionSearchResultDto
                {
                    Id = ep.Id,
                    TxId = ep.TxId,
                    ExternalPaymentId = ep.ExternalPaymentId,
                    PspName = ep.PspName,
                    PspId = ep.PspId,
                    ClientId = ep.ClientId,
                    BrandId = ep.BrandId,
                    Amount = ep.Amount,
                    CurrencyCode = ep.CurrencyCode,
                    TxDate = ep.TxDate,
                    Status = ep.Status,
                    Action = ep.Action,
                    ExternalSystem = ep.ExternalSystem,
                    IsSettled = settlement != null,
                    SettlementStatus = settlement?.ReconciliationStatus,
                    TotalFees = settlement?.TotalFees,
                    NetSettlement = settlement?.NetSettlement,
                    SettlementDate = settlement?.TxDate,
                    ReconciliationRunId = settlement?.ReconciliationRunId,
                    CaseId = caseInfo?.Id,
                    CaseNumber = caseInfo?.CaseNumber,
                    CaseTitle = caseInfo?.Title,
                    CaseStatus = caseInfo?.Status
                };
            }).ToList();

            return new PaginatedData<TransactionSearchResultDto>(result, total);
        }
    }
}
