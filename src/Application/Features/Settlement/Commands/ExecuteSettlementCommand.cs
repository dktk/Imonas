using Application.Features.Settlement.DTOs;

using SG.Common;

namespace Application.Features.Settlement.Commands
{
    public class ExecuteSettlementCommand : IRequest<Result<SettlementResultDto>>
    {
        public int RunId { get; set; }
        public int? PspId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CurrencyCode { get; set; }
        public bool StopAtFirstMatch { get; set; } = true;
        public bool CreateCasesForExceptions { get; set; } = true;
        public decimal? MinimumMatchScore { get; set; } = 0.8m;
    }

    public class ExecuteSettlementCommandHandler(
        ISettlementService settlementService,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService) :
        IRequestHandler<ExecuteSettlementCommand, Result<SettlementResultDto>>
    {
        public async Task<Result<SettlementResultDto>> Handle(
            ExecuteSettlementCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Verify run exists
                var run = await dbContext.ReconciliationRuns
                    .FindAsync(new object[] { request.RunId }, cancellationToken);

                if (run == null)
                {
                    return Result<SettlementResultDto>.BuildFailure($"Reconciliation run {request.RunId} not found.");
                }

                // Check if run is already completed or running
                if (run.Status == Domain.Enums.RunStatus.Running)
                {
                    return Result<SettlementResultDto>.BuildFailure("This reconciliation run is already in progress.");
                }

                if (run.Status == Domain.Enums.RunStatus.Completed)
                {
                    return Result<SettlementResultDto>.BuildFailure("This reconciliation run has already been completed. Create a new run to process more transactions.");
                }

                var options = new SettlementRunOptionsDto
                {
                    PspId = request.PspId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CurrencyCode = request.CurrencyCode,
                    StopAtFirstMatch = request.StopAtFirstMatch,
                    CreateCasesForExceptions = request.CreateCasesForExceptions,
                    MinimumMatchScore = request.MinimumMatchScore,
                    CurrentUserId = currentUserService.UserId
                };

                var result = await settlementService.ExecuteSettlementAsync(
                    request.RunId,
                    options,
                    cancellationToken);

                return Result<SettlementResultDto>.BuildSuccess(
                    result,
                    $"Settlement completed. Matched: {result.MatchedCount}, Partial: {result.PartialMatchCount}, Unmatched: {result.InternalUnmatchedCount + result.ExternalUnmatchedCount}");
            }
            catch (Exception ex)
            {
                return Result<SettlementResultDto>.BuildFailure($"Settlement execution failed: {ex.Message}");
            }
        }
    }
}
