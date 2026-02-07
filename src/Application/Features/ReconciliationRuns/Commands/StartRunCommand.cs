using Application.Features.ReconciliationRuns.DTOs;
using Application.Features.Settlement.DTOs;

using SG.Common;

namespace Application.Features.ReconciliationRuns.Commands
{
    public class StartRunCommand : IRequest<Result<ReconciliationRunDto>>
    {
        public string RunName { get; set; } = string.Empty;
        public string RulePackVersion { get; set; } = "1.0.0";
        public string? Description { get; set; }
        public bool IncludeInternalTransactions { get; set; } = true;
        public bool IncludePspTransactions { get; set; } = true;
        public bool IncludeBankTransactions { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool StopAtFirstMatch { get; set; }
        public bool GenerateEvidencePack { get; set; } = true;
        public bool CreateCasesForExceptions { get; set; } = true;
        public int? PspId { get; set; }
        public string? CurrencyCode { get; set; }
        public bool ExecuteSettlement { get; set; } = true;
        public decimal? MinimumMatchScore { get; set; } = 0.8m;
    }

    public class StartRunCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IDateTime dateTime,
        ISettlementService settlementService) :
        IRequestHandler<StartRunCommand, Result<ReconciliationRunDto>>
    {
        public async Task<Result<ReconciliationRunDto>> Handle(StartRunCommand request, CancellationToken cancellationToken)
        {
            var run = new ReconciliationRun
            {
                RunName = request.RunName,
                RulePackVersion = request.RulePackVersion,
                Status = RunStatus.Pending,
                StartedAt = dateTime.Now,
                IsReplayable = true,
                Created = dateTime.Now,
                UserId = currentUserService.UserId ?? "System"
            };

            context.ReconciliationRuns.Add(run);
            await context.SaveChangesAsync(cancellationToken);

            // Execute settlement if requested
            if (request.ExecuteSettlement)
            {
                try
                {
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

                    var settlementResult = await settlementService.ExecuteSettlementAsync(
                        run.Id,
                        options,
                        cancellationToken);

                    // Reload run to get updated stats
                    await context.ReconciliationRuns
                        .Entry(run)
                        .ReloadAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    run.Status = RunStatus.Failed;
                    run.ErrorMessage = ex.Message;
                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            var dto = mapper.Map<ReconciliationRunDto>(run);

            var statusMessage = run.Status switch
            {
                RunStatus.Completed => $"Reconciliation run '{request.RunName}' completed. Matched: {run.MatchedRecords}, Unmatched: {run.UnmatchedRecords}",
                RunStatus.Failed => $"Reconciliation run '{request.RunName}' failed: {run.ErrorMessage}",
                _ => $"Reconciliation run '{request.RunName}' started successfully."
            };

            return Result<ReconciliationRunDto>.BuildSuccess(dto, statusMessage);
        }
    }
}
