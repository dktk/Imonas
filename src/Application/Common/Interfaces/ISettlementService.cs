using Application.Features.Settlement.DTOs;
using Domain.Entities.MedalionData.Gold;
using Domain.Entities.MedalionData.Silver;
using Domain.Entities.Rules;

namespace Application.Common.Interfaces
{
    public interface ISettlementService
    {
        /// <summary>
        /// Executes the settlement matching process for a reconciliation run.
        /// </summary>
        Task<SettlementResultDto> ExecuteSettlementAsync(int runId, SettlementRunOptionsDto options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Matches a single internal payment against all external payments using active rules.
        /// </summary>
        Task<RuleMatchResult?> FindMatchForInternalPaymentAsync(InternalPayment internalPayment, IEnumerable<ExternalPayment> externalPayments, IEnumerable<MatchingRule> rules);

        /// <summary>
        /// Evaluates if two transactions match based on a specific rule.
        /// </summary>
        RuleMatchResult EvaluateRule(MatchingRule rule, InternalPayment internalPayment, ExternalPayment externalPayment);

        /// <summary>
        /// Gets the active matching rules ordered by priority.
        /// </summary>
        Task<List<MatchingRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a settlement record from a match result.
        /// </summary>
        Task<PspSettlement> CreateSettlementAsync(InternalPayment internalPayment, ExternalPayment externalPayment, RuleMatchResult matchResult, int runId, CancellationToken cancellationToken = default);
    }
}
