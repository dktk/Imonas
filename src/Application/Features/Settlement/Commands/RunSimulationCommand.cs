using Application.Features.Settlement.DTOs;
using SG.Common;

namespace Application.Features.Settlement.Commands
{
    public class RunSimulationCommand : IRequest<Result<SimulationResultDto>>
    {
        public int? PspId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CurrencyCode { get; set; }
        public bool StopAtFirstMatch { get; set; } = true;
        public decimal? MinimumMatchScore { get; set; } = 0.8m;
        public List<int> CandidateRuleIds { get; set; } = new();
        public decimal FalsePositiveThreshold { get; set; } = 0.7m;
    }

    public class RunSimulationCommandHandler(
        ISettlementService settlementService,
        ICurrentUserService currentUserService) :
        IRequestHandler<RunSimulationCommand, Result<SimulationResultDto>>
    {
        public async Task<Result<SimulationResultDto>> Handle(
            RunSimulationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request.CandidateRuleIds == null || !request.CandidateRuleIds.Any())
                {
                    return Result<SimulationResultDto>.BuildFailure(
                        "At least one candidate rule must be selected for simulation.");
                }

                var options = new SettlementRunOptionsDto
                {
                    PspId = request.PspId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CurrencyCode = request.CurrencyCode,
                    StopAtFirstMatch = request.StopAtFirstMatch,
                    MinimumMatchScore = request.MinimumMatchScore,
                    CurrentUserId = currentUserService.UserId
                };

                var result = await settlementService.SimulateAsync(
                    options,
                    request.CandidateRuleIds,
                    request.FalsePositiveThreshold,
                    cancellationToken);

                return Result<SimulationResultDto>.BuildSuccess(
                    result,
                    $"Simulation completed in {result.Duration.TotalSeconds:F1}s. " +
                    $"Baseline: {result.Baseline.MatchedCount} matched. " +
                    $"Candidate: {result.Candidate.MatchedCount} matched. " +
                    $"Lift: {result.Lift.MatchedDelta:+#;-#;0} matches.");
            }
            catch (Exception ex)
            {
                return Result<SimulationResultDto>.BuildFailure($"Simulation failed: {ex.Message}");
            }
        }
    }
}
